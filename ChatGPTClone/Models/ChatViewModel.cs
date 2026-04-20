using System.Collections.Generic;

namespace ChatGPTClone.Models
{
    public class ChatViewModel
    {
        public List<Message> Messages { get; set; } = new List<Message>();
        public string? NewMessage { get; set; }
        public string CurrentChatId { get; set; } = "default";
        public string ChatTitle { get; set; } = "New Chat";
    }

    public class Message
    {
        public string? Role { get; set; } // user or assistant
        public string? Content { get; set; }
    }

    public class ChatData
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = "New Chat";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}