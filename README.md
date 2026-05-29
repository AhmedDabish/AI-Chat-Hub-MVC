<img width="1918" height="870" alt="image" src="https://github.com/user-attachments/assets/0f592e25-03da-4b46-89a8-7e113894b6e1" />
 
# AI Chat Hub 🤖

A modern, fully-featured AI chat web application built with ASP.NET Core and integrated with multiple LLM providers.

### 📋 Overview

AI Chat Hub is a sophisticated chat application that allows users to interact with multiple AI providers (OpenAI, Groq, Hugging Face) through a beautiful, modern interface. Features include multiple chat management, persistent storage, and a responsive design.

### ✨ Key Features

- 💬 **Multi-Provider Support** - Seamlessly switch between OpenAI, Groq, and Hugging Face models
- 📁 **Chat Management** - Create, manage, and organize multiple conversations
- 💾 **Persistent Storage** - All chats automatically saved to JSON file
- 🏷️ **Auto-Naming** - Chat titles generated from first user message
- 📱 **Responsive Design** - Works beautifully on desktop, tablet, and mobile
- 🎨 **Modern UI** - Clean, intuitive interface inspired by ChatGPT
- ⚡ **Real-time Updates** - Instant message display with loading indicators
- 🔄 **Chat Controls** - Edit, delete, and regenerate messages
- 📎 **File Upload Support** - Attach files to messages
- 🌙 **Dark Theme** - Eye-friendly dark mode interface

### 🛠️ Technology Stack

#### Backend
| Technology | Version | Purpose |
|-----------|---------|---------|
| **ASP.NET Core** | 10.0 | Web application framework and runtime |
| **C#** | 12.0 | Primary backend programming language |
| **.NET CLI** | Latest | Build, run, and manage the project |
| **MVC Pattern** | - | Application architecture and separation of concerns |


#### Frontend
| Technology | Version | Purpose |
|-----------|---------|---------|
| **HTML5** | - | Semantic markup and structure |
| **CSS3** | - | Modern styling with gradients and animations |
| **JavaScript (ES6+)** | - | Interactive frontend functionality |
| **Razor Pages** | - | Dynamic server-side rendering |

#### APIs & LLM Integration
| Service | Purpose | Model(s) |
|---------|---------|----------|
| **OpenAI API** | Primary LLM provider | gpt-4o-mini |
| **Groq API** | Fast LLM inference | llama-3.3-70b-versatile |
| **Hugging Face API** | Alternative provider | Phi-3-mini |

#### Data & Storage
| Technology | Purpose |
|-----------|---------|
| **JSON** | Chat persistence and configuration storage |
| **File System** | Local data storage (chats.json) |

#### Development Tools
| Tool | Version | Purpose |
|------|---------|---------|
| **Visual Studio Code** | Latest | Code editor and IDE |
| **Git** | 2.x | Version control system |
| **NuGet** | Latest | .NET package manager |
| **OpenAI SDK** | Latest | Official OpenAI API client library |

### 🏗️ Project Structure

```
AIChat/
├── Controllers/
│   └── HomeController.cs          # Request handling and business logic
├── Models/
│   ├── ChatViewModel.cs            # View model for chat data
│   └── ChatData.cs                 # Chat persistence model
├── Views/
│   ├── Home/
│   │   └── Index.cshtml            # Main chat interface
│   └── Shared/
│       └── _Layout.cshtml          # Master layout template
├── wwwroot/
│   ├── css/
│   │   └── chat.css                # Stylesheet (gradients, animations, dark theme)
│   └── js/
│       └── site.js                 # Frontend interactivity
├── Properties/
│   └── launchSettings.json         # Development server configuration
├── appsettings.json                # Application configuration
├── apiconfig.json                  # API keys for LLM providers
├── chats.json                      # Persistent chat storage
└── Program.cs                      # Application entry point
```

### 🚀 Getting Started

#### Prerequisites
- .NET 10.0 SDK or later
- API keys from: OpenAI, Groq, or Hugging Face
- Visual Studio Code or any text editor
- Git (for version control)

#### Installation

1. **Clone the repository**
```bash
git clone <repository-url>
cd Day2
```

2. **Configure API Keys**
Create `AIChat/apiconfig.json`:
```json
{
  "openai": {
    "apiKey": "your-openai-key-here"
  },
  "groq": {
    "apiKeys": ["your-groq-key-here"],
    "model": "llama-3.3-70b-versatile",
    "baseUrl": "https://api.groq.com/openai/v1"
  },
  "huggingFace": {
    "tokens": ["your-huggingface-token-here"],
    "model": "gpt2",
    "baseUrl": "https://api-inference.huggingface.co/models/"
  }
}
```

3. **Restore Dependencies**
```bash
cd AIChat
dotnet restore
```

4. **Build the Project**
```bash
dotnet build
```

5. **Run the Application**
```bash
dotnet run
```

6. **Access the App**
Open your browser and navigate to `http://localhost:5292`

### 📖 Usage

#### Creating a New Chat
1. Click the **"New Chat"** button in the left sidebar
2. Start typing your first message
3. The chat title will be automatically generated from your first prompt
4. Click **Send** or press Enter

#### Managing Chats
- **View Recent Chats** - All your conversations appear in the Recents section
- **Switch Chats** - Click any chat to load its history
- **Clear History** - Click the **⋮** menu button next to Recents, then select "Clear History"

#### Chat Features
- **Edit Message** - Click Edit button to modify your last message
- **Delete Message** - Remove the last message pair
- **Regenerate Response** - Get a new AI response to the same prompt
- **Attach Files** - Click the 📎 button to attach files
- **Change Provider** - Select different AI providers from the dropdown
  
### 💾 Data Storage

All chats are automatically saved to `chats.json` with the following structure:
```json
[
  {
    "id": "unique-chat-id",
    "title": "First message preview...",
    "createdAt": "2026-04-20T10:30:00Z",
    "messages": [
      {
        "role": "user",
        "content": "Your message here"
      },
      {
        "role": "assistant",
        "content": "AI response here"
      }
    ]
  }
]
```

### 🚧 Future Enhancements

- [ ] User authentication and cloud sync
- [ ] Export conversations as PDF
- [ ] Voice input/output support
- [ ] Custom system prompts
- [ ] Chat folders and organization
- [ ] Search and filter functionality
- [ ] Dark/Light theme toggle
- [ ] API usage analytics



