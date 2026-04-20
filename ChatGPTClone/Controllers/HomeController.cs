using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ChatGPTClone.Models;
using OpenAI;
using System.Text.Json;
using OpenAI.Chat;
using System.IO;
using System.Net.Http;
using System.ClientModel;

namespace ChatGPTClone.Controllers;

public class HomeController : Controller
{
    private readonly OpenAIClient _openaiClient;
    private readonly string? _hfToken;
    private readonly string? _hfModel;
    private readonly string? _hfBaseUrl;
    private readonly string? _groqKey;
    private readonly string? _groqModel;
    private static readonly string _chatStorePath = Path.Combine(Directory.GetCurrentDirectory(), "chats.json");
    private static Dictionary<string, ChatData> _chats = new();
    private static string _currentChatId = "default";
    private static bool _storeLoaded = false;
    private static readonly object _lock = new();

    public HomeController()
    {
        LoadChatStore();

        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "apiconfig.json");
        var configJson = System.IO.File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(configJson);
        if (config == null) throw new Exception("Config file not found or invalid");
        var openai = config["openai"];
        var openaiKey = openai.GetProperty("apiKey").GetString();
        _openaiClient = new OpenAIClient(openaiKey!);

        var hf = config["huggingFace"];
        _hfToken = hf.GetProperty("tokens")[0].GetString();
        _hfModel = hf.GetProperty("model").GetString();
        _hfBaseUrl = hf.GetProperty("baseUrl").GetString();

        var groq = config["groq"];
        _groqKey = groq.GetProperty("apiKeys")[0].GetString();
        _groqModel = groq.GetProperty("model").GetString();
    }

    public IActionResult Index()
    {
        lock (_lock)
        {
            if (!_chats.Any())
            {
                _chats[_currentChatId] = new ChatData
                {
                    Id = _currentChatId,
                    Title = "New Chat",
                    CreatedAt = DateTime.UtcNow
                };
                SaveChatStore();
            }

            if (!_chats.ContainsKey(_currentChatId))
            {
                _currentChatId = _chats.Keys.First();
            }

            var currentChat = _chats[_currentChatId];
            var messages = currentChat.Messages.Select(m => new Message { Role = m.Role, Content = m.Content }).ToList();
            return View(new ChatViewModel
            {
                Messages = messages,
                CurrentChatId = currentChat.Id,
                ChatTitle = currentChat.Title
            });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SendMessage(string message, string provider, IFormFile file)
    {
        try
        {
            lock (_lock)
            {
                if (!_chats.ContainsKey(_currentChatId))
                {
                    _chats[_currentChatId] = new ChatData
                    {
                        Id = _currentChatId,
                        Title = "New Chat",
                        CreatedAt = DateTime.UtcNow
                    };
                }
            }

            var currentChat = _chats[_currentChatId];
            string fullMessage = message;
            if (file != null)
            {
                fullMessage += $" [File: {file.FileName}]";
            }

            if (string.IsNullOrWhiteSpace(fullMessage))
                return Json(new { success = false, error = "Message cannot be empty" });

            var isFirstMessage = currentChat.Messages.Count == 0 && !string.IsNullOrWhiteSpace(message);
            currentChat.Messages.Add(new Message { Role = "user", Content = fullMessage });
            string assistantMessage = "";

            try
            {
                if (provider == "openai")
                {
                    var chatMessages = currentChat.Messages.Select(m =>
                        m.Role == "user" ? (ChatMessage)new UserChatMessage(m.Content) :
                        new AssistantChatMessage(m.Content)
                    ).ToList();
                    var chatClient = _openaiClient.GetChatClient("gpt-4o-mini");
                    var response = await chatClient.CompleteChatAsync(chatMessages);
                    assistantMessage = response.Value.Content[0].Text;
                }
                else if (provider == "groq")
                {
                    var groqClient = new OpenAIClient(
                        new ApiKeyCredential(_groqKey!),
                        new OpenAIClientOptions { Endpoint = new Uri("https://api.groq.com/openai/v1") }
                    );
                    var chatMessages = currentChat.Messages.Select(m =>
                        m.Role == "user" ? (ChatMessage)new UserChatMessage(m.Content) :
                        new AssistantChatMessage(m.Content)
                    ).ToList();
                    var chatClient = groqClient.GetChatClient(_groqModel!);
                    var response = await chatClient.CompleteChatAsync(chatMessages);
                    assistantMessage = response.Value.Content[0].Text;
                }
                else if (provider == "huggingface")
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _hfToken);
                    var url = $"{_hfBaseUrl}{_hfModel}";
                    var body = new { inputs = fullMessage };
                    var json = JsonSerializer.Serialize(body);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url, content);
                    var result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        currentChat.Messages.RemoveAt(currentChat.Messages.Count - 1);
                        return Json(new { success = false, error = $"Hugging Face Error: {result}" });
                    }

                    var parsed = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(result);
                    assistantMessage = parsed?[0]?["generated_text"] ?? "No response received";
                }
                else
                {
                    currentChat.Messages.RemoveAt(currentChat.Messages.Count - 1);
                    return Json(new { success = false, error = "Unknown provider" });
                }
            }
            catch (Exception ex)
            {
                currentChat.Messages.RemoveAt(currentChat.Messages.Count - 1);
                return Json(new { success = false, error = $"Provider Error: {ex.Message}" });
            }

            if (isFirstMessage)
            {
                currentChat.Title = GetChatTitle(message);
            }

            currentChat.Messages.Add(new Message { Role = "assistant", Content = assistantMessage });
            SaveChatStore();
            return Json(new { success = true, response = assistantMessage });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = $"Server Error: {ex.Message}" });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult ClearHistory()
    {
        if (_chats.ContainsKey(_currentChatId))
        {
            _chats[_currentChatId].Messages.Clear();
            SaveChatStore();
        }
        return Json(new { success = true, message = "Chat history cleared" });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteLastMessage()
    {
        if (_chats.ContainsKey(_currentChatId) && _chats[_currentChatId].Messages.Count >= 2)
        {
            var messages = _chats[_currentChatId].Messages;
            messages.RemoveAt(messages.Count - 1);
            messages.RemoveAt(messages.Count - 1);
            SaveChatStore();
            return Json(new { success = true, message = "Last message pair deleted" });
        }
        return Json(new { success = false, error = "No messages to delete" });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult EditLastMessage(string newMessage)
    {
        if (_chats.ContainsKey(_currentChatId))
        {
            var messages = _chats[_currentChatId].Messages;
            if (messages.Count >= 2 && messages[messages.Count - 2].Role == "user")
            {
                messages.RemoveAt(messages.Count - 1);
                messages.RemoveAt(messages.Count - 1);
                messages.Add(new Message { Role = "user", Content = newMessage });
                SaveChatStore();
                return Json(new { success = true, message = "Last message updated" });
            }
        }
        return Json(new { success = false, error = "Cannot edit message" });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult NewChat()
    {
        _currentChatId = Guid.NewGuid().ToString();
        _chats[_currentChatId] = new ChatData
        {
            Id = _currentChatId,
            Title = "New Chat",
            CreatedAt = DateTime.UtcNow,
            Messages = new List<Message>()
        };
        SaveChatStore();
        return Json(new { success = true, chatId = _currentChatId });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult SwitchChat(string chatId)
    {
        if (_chats.ContainsKey(chatId))
        {
            _currentChatId = chatId;
            return Json(new { success = true });
        }
        return Json(new { success = false, error = "Chat not found" });
    }

    [HttpGet]
    public IActionResult GetChats()
    {
        var chatList = _chats.Values
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new { id = c.Id, title = string.IsNullOrWhiteSpace(c.Title) ? "New Chat" : c.Title })
            .ToList();
        return Json(chatList);
    }

    private static void LoadChatStore()
    {
        lock (_lock)
        {
            if (_storeLoaded) return;
            if (System.IO.File.Exists(_chatStorePath))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(_chatStorePath);
                    var chats = JsonSerializer.Deserialize<List<ChatData>>(json);
                    if (chats != null)
                    {
                        _chats = chats.ToDictionary(c => c.Id);
                        if (_chats.Any())
                        {
                            _currentChatId = _chats.Keys.First();
                        }
                    }
                }
                catch
                {
                    _chats = new Dictionary<string, ChatData>();
                }
            }
            _storeLoaded = true;
        }
    }

    private static void SaveChatStore()
    {
        lock (_lock)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var list = _chats.Values.OrderBy(c => c.CreatedAt).ToList();
            System.IO.File.WriteAllText(_chatStorePath, JsonSerializer.Serialize(list, options));
        }
    }

    private static string GetChatTitle(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return "New Chat";
        }

        var firstLine = prompt.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? prompt.Trim();
        if (firstLine.Length <= 35)
        {
            return firstLine;
        }

        var cut = firstLine.Substring(0, 35);
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > 10)
        {
            cut = cut.Substring(0, lastSpace);
        }

        return cut + "...";
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
