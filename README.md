[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-green.svg)](https://docs.microsoft.com/en-us/ef/core/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-orange.svg)](https://docs.microsoft.com/en-us/aspnet/core/signalr/)


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
- [CI/CD](#cicd)
- [Current Application Version](#current-application-version)

## 🎯 Project Description

This project is the backend part of a social network that provides APIs for managing users, posts, comments, and real-time chat. The system is built using Clean Architecture, ensuring flexibility, testability, and ease of code maintenance.

## 🏗️ Architecture

The project uses Clean Architecture with the following layers:

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
- Node.js and npm (for frontend)
- Google OAuth credentials and Azure Blob Storage (if needed)

### Installation Steps

1. **Clone the repository**
```bash
git clone <repository-url>
cd SocialNetwork_Server
``` 

2. **Install backend dependencies**
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

5. **Run the backend application**
```bash
dotnet run --project SocialNetwork.API
``` 

The application will be available at: `https://localhost:7000`

### Running the Frontend Client

1. **Navigate to the frontend directory**
```bash
cd frontend/socialnetwork.client
``` 

2. **Install frontend dependencies**
```bash
npm install
``` 

3. **Run the frontend application**
```bash
npm run dev
``` 

You can set the API URL in the `.env.local` file:
```
VITE_API_BASE=https://localhost:7000
```

### Running Tests

To run the tests, execute:
```bash
dotnet test backend/SocialNetwork.Tests/SocialNetwork.Tests.csproj
``` 

### Database Migrations

Migrations are stored in `backend/SocialNetwork.Infrastructure/Migrations`. To apply the latest schema, run:
```bash
dotnet ef database update --project backend/SocialNetwork.Infrastructure --startup-project backend/SocialNetwork.API
``` 

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
MySocialNetwork/
├── backend/
│   ├── SocialNetwork.API/                 # Web API project
│   ├── SocialNetwork.Application/         # Business logic
│   ├── SocialNetwork.Domain/             # Domain layer
│   ├── SocialNetwork.Infrastructure/     # Infrastructure layer
│   └── SocialNetwork.Tests/             # Tests
└── frontend/
    ├── socialnetwork.client/             # React client
    └── SocialNetwork.mobile/              # Mobile client
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

## 🗳️ CI/CD

For automating the verification and delivery of changes, it is recommended to use a CI/CD pipeline. The repository does not contain configurations for a specific CI provider, so the steps below describe the necessary process regardless of the platform.

### Continuous Integration

For each pull request and push to the main branch, the pipeline should execute:

1. Restore backend dependencies.
2. Build all projects on `.NET 8`.
3. Run tests in `SocialNetwork.Tests`.
4. Install frontend dependencies using `npm ci`.
5. Lint the frontend using `npm run lint`.
6. Build the frontend for production using `npm run build`.

Example CI commands:
```bash
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test backend/SocialNetwork.Tests/SocialNetwork.Tests.csproj --configuration Release --no-build

cd frontend/socialnetwork.client
npm ci
npm run lint
npm run build
```

### Continuous Delivery/Deployment

After successful completion of CI, you can deploy the backend, frontend, and database to the target environment. Before deployment, ensure to configure secrets and environment variables for the connection string, JWT, Google OAuth, Azure Blob Storage, and `VITE_API_BASE`.

Entity Framework Core migrations should be applied carefully during deployment:
```bash
dotnet ef database update --project backend/SocialNetwork.Infrastructure --startup-project backend/SocialNetwork.API
```

### Current Stack

- Backend: `.NET 8`, ASP.NET Core Web API, Clean Architecture.
- Data: SQL Server, Entity Framework Core `9.0.8`.
- Authentication: JWT Bearer and Google Authentication.
- Real-time: ASP.NET Core SignalR for chats and notifications.
- API Documentation: Swagger/OpenAPI.
- Web: React `19`, Vite `7`, React Router `7`.
- File Storage: Azure Blob Storage.

### Implemented Features

- User registration and login via JWT.
- Google login integration.
- User profiles, roles, banning, and access management.
- Posts, comments, and reactions.
- Friends and user search functionality.
- Private, group chats, and channels.
- Real-time messaging via SignalR.
- Real-time notifications.
- Uploading avatars and other files to Azure Blob Storage.
- Light and dark themes for the web client.
- Emoji support in messages.
- Swagger UI for API testing.

## 📝 License

This project is developed for educational purposes.

