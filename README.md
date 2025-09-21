# 🌐 Social Network Server

Backend part of a social network built on .NET 8 using Clean Architecture principles.

## 📋 Table of Contents

- [Project Description](#project-description)
- [Architecture](#architecture)
- [Technologies](#technologies)
- [Features](#features)
- [Installation & Setup](#installation--setup)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Database](#database)
- [Authentication](#authentication)
- [Real-time Chat](#real-time-chat)

## 🎯 Project Description

This project is the backend part of a social network that provides APIs for managing users, posts, comments, and real-time chat. The system is built using Clean Architecture, ensuring flexibility, testability, and ease of code maintenance.

## 🏗️ Architecture

The project uses Clean Architecture with the following layers:

```
📁 SocialNetwork.API (Presentation Layer)
├── Controllers/ - API Controllers
├── Hubs/ - SignalR Hubs for real-time communication
├── Middleware/ - Custom middleware
└── Extensions/ - Configuration extensions

📁 SocialNetwork.Application (Application Layer)
├── DTO/ - Data Transfer Objects
├── Interfaces/ - Service contracts
├── Service/ - Business logic
└── Mappings/ - AutoMapper profiles

📁 SocialNetwork.Domain (Domain Layer)
├── Entities/ - Domain models
├── Enums/ - Enumerations
└── Interfaces/ - Domain contracts

📁 SocialNetwork.Infrastructure (Infrastructure Layer)
├── Repos/ - Repositories
├── Configurations/ - EF Core configurations
├── Security/ - JWT providers
└── SocialDbContext.cs - Database context
```

## 🛠️ Technologies

- **.NET 8** - Main framework
- **Entity Framework Core 9.0** - ORM for database operations
- **SQL Server** - Database
- **SignalR** - Real-time communication
- **JWT** - Authentication and authorization
- **AutoMapper** - Object mapping
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Testing

## ⚡ Features

### 🔐 Authentication & Users
- User registration
- Login with JWT tokens
- User profile management
- User ban system

### 📝 Posts & Comments
- Create, view and edit posts
- Comment on posts
- Post ban system
- Display all posts and comments

### 💬 Real-time Chat
- Private chats between users
- Group chats
- Channels
- Real-time messaging via SignalR

### 🔧 Administrative Functions
- User and post banning
- Access rights management

## 🚀 Installation & Setup

### Prerequisites
- .NET 8 SDK
- SQL Server (local or Azure)
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Clone the repository**
```bash
git clone <repository-url>
cd SocialNetwork_Server
```

2. **Install dependencies**
```bash
dotnet restore
```

3. **Configure database**
   - Update connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=SocialNetworkDb;Integrated Security=True;TrustServerCertificate=True"
   }
   ```

4. **Apply migrations**
```bash
dotnet ef database update --project SocialNetwork.Infrastructure --startup-project SocialNetwork.API
```

5. **Run the application**
```bash
dotnet run --project SocialNetwork.API
```

The application will be available at: `https://localhost:7000`

## 📚 API Documentation

After running the application, Swagger documentation is available at:
- **Swagger UI**: `https://localhost:7000/swagger`

### Main Endpoints:

#### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login

#### Posts
- `GET /api/post` - Get all posts
- `GET /api/post/{id}` - Get post by ID
- `POST /api/post` - Create new post (requires authentication)
- `DELETE /api/post/{id}` - Ban post (admin only)

#### Users
- `GET /api/user` - Get all users
- `GET /api/user/{id}` - Get user by ID

#### Comments
- `GET /api/comment` - Get all comments
- `POST /api/comment` - Create comment (requires authentication)

#### Real-time Chat
- `POST /chatHub` - SignalR hub for real-time communication

## 📁 Project Structure

```
SocialNetwork_Server/
├── SocialNetwork.API/                 # Web API project
│   ├── Controllers/                   # API Controllers
│   ├── Hubs/                         # SignalR Hubs
│   ├── Middleware/                   # Custom middleware
│   ├── Extensions/                   # Extensions
│   └── Program.cs                    # Entry point
├── SocialNetwork.Application/         # Business logic
│   ├── DTO/                         # Data Transfer Objects
│   ├── Interfaces/                  # Service contracts
│   ├── Service/                     # Service implementations
│   └── Mappings/                    # AutoMapper profiles
├── SocialNetwork.Domain/             # Domain layer
│   ├── Entities/                    # Domain models
│   ├── Enums/                       # Enumerations
│   └── Interfaces/                  # Domain contracts
├── SocialNetwork.Infrastructure/     # Infrastructure layer
│   ├── Repos/                       # Repositories
│   ├── Configurations/              # EF Core configurations
│   ├── Security/                    # JWT providers
│   └── SocialDbContext.cs           # Database context
└── SocialNetwork.Tests/             # Tests
```

## 🗄️ Database

### Main Entities:

- **User** - System users
- **Post** - User posts
- **Comment** - Post comments
- **Chat** - Chats (private, group, channels)
- **Message** - Chat messages
- **UserChat** - User-Chat relationship

### Migrations:
The project uses Entity Framework Core migrations for database schema management. All migrations are located in the `SocialNetwork.Infrastructure/Migrations/` folder.

## 🔐 Authentication

The system uses JWT (JSON Web Tokens) for authorization:

- **Secret Key**: Configured in `appsettings.json`
- **Expiration**: 12 hours (configurable)
- **Claims**: User ID, Email, Role

## 💬 Real-time Chat

The chat system is built on SignalR and supports:

- **Private chats** - one-on-one
- **Group chats** - multiple users
- **Channels** - public channels

### Connecting to Chat:
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

// Join chat
await connection.invoke("JoinChat", chatId, userId);

// Send message
await connection.invoke("SendMessage", chatId, "Hello!");
```

### Logging
The project uses the built-in .NET logging system with different levels:
- **Information** - General information
- **Warning** - Warnings
- **Error** - Errors

## 📝 License

This project is developed for educational purposes.

