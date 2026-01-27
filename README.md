# Task Management System

A full-stack web application for managing user tasks with a .NET Core backend, React frontend, SQL Server database, and Windows Service with RabbitMQ integration.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Database Schema](#database-schema)
- [API Documentation](#api-documentation)
- [Running the Application](#running-the-application)
- [Testing](#testing)
- [SQL Query](#sql-query)

## Overview

This application provides a comprehensive task management system where users can:
- Create, read, update, and delete tasks
- Assign tasks to users with full contact details
- Tag tasks with multiple tags (N:N relationship)
- Set priorities and due dates
- Receive automated reminders for overdue tasks via RabbitMQ

## Architecture

The application follows **Clean Architecture** principles with **CQRS (Command Query Responsibility Segregation)** pattern:

```
┌─────────────────────────────────────────────────────────────┐
│                    React Frontend                            │
│  (Redux Toolkit for State Management)                       │
│  - Task CRUD UI                                              │
│  - Form Validation (Yup + React Hook Form)                 │
│  - Tag Management (Multiple Dropdowns)                       │
└──────────────────────┬──────────────────────────────────────┘
                        │ HTTP/REST
┌──────────────────────▼──────────────────────────────────────┐
│              TaskManagement.API (.NET Core)                 │
│  - RESTful Controllers                                       │
│  - Validation Middleware                                     │
│  - CORS Configuration                                        │
└──────────────────────┬──────────────────────────────────────┘
                        │
┌──────────────────────▼──────────────────────────────────────┐
│         TaskManagement.Application (CQRS)                    │
│  ┌──────────────┐         ┌──────────────┐                 │
│  │   Commands   │         │    Queries   │                 │
│  │ - CreateTask │         │ - GetTasks   │                 │
│  │ - UpdateTask │         │ - GetTask    │                 │
│  │ - DeleteTask │         │ - GetTags    │                 │
│  └──────────────┘         └──────────────┘                 │
│  - DTOs (Request/Response)                                  │
│  - Validators (FluentValidation)                            │
│  - Mappings (AutoMapper)                                    │
└──────────────────────┬──────────────────────────────────────┘
                        │
┌──────────────────────▼──────────────────────────────────────┐
│         TaskManagement.Domain                                │
│  - Entities: Task, Tag, TaskTag, User                       │
│  - Enums: Priority                                           │
└──────────────────────┬──────────────────────────────────────┘
                        │
┌──────────────────────▼──────────────────────────────────────┐
│      TaskManagement.Infrastructure                          │
│  - DbContext (Entity Framework Core)                        │
│  - RabbitMQ Integration                                      │
│  - Configuration                                             │
└──────────────────────┬──────────────────────────────────────┘
                        │
┌──────────────────────▼──────────────────────────────────────┐
│              SQL Server Database                             │
│  - Tasks, Tags, TaskTags, Users Tables                      │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│         TaskManagement.WindowsService                       │
│  - Background Service                                        │
│  - RabbitMQ Publisher (Check Due Dates)                     │
│  - RabbitMQ Consumer (Process Reminders)                   │
│  - Logging                                                   │
└─────────────────────────────────────────────────────────────┘
```

### Key Design Patterns

- **CQRS**: Separates read and write operations using MediatR
- **Repository Pattern**: Abstraction over data access (via DbContext)
- **Dependency Injection**: Throughout all layers
- **Validation Pipeline**: FluentValidation with MediatR behaviors

## Features

- ✅ Full CRUD operations for tasks
- ✅ User management with contact details (Full Name, Email, Telephone)
- ✅ Multiple tags per task (N:N relationship)
- ✅ Priority levels (Low, Medium, High, Critical)
- ✅ Due date tracking
- ✅ Comprehensive field validation (frontend and backend)
- ✅ Responsive React UI
- ✅ Windows Service for automated task reminders
- ✅ RabbitMQ integration for message queuing
- ✅ Unit and integration tests

## Technology Stack

### Backend
- **.NET 8.0** - Core framework
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Database
- **MediatR** - CQRS implementation
- **FluentValidation** - Validation
- **AutoMapper** - Object mapping
- **RabbitMQ.Client** - Message queuing

### Frontend
- **React 18** - UI framework
- **TypeScript** - Type safety
- **Redux Toolkit** - State management
- **React Hook Form** - Form handling
- **Yup** - Schema validation
- **Axios** - HTTP client
- **Vite** - Build tool

### Testing
- **xUnit** - Testing framework
- **Moq** - Mocking
- **FluentAssertions** - Assertions
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

## Prerequisites

Before you begin, ensure you have the following installed:

1. **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **SQL Server** (LocalDB or full instance)
3. **Node.js 20+** and **npm** - [Download](https://nodejs.org/)
4. **RabbitMQ Server** - [Download](https://www.rabbitmq.com/download.html)
5. **Visual Studio 2022** or **VS Code** (optional, for development)

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd UserTasks
```

### 2. Database Setup

#### Option A: Using LocalDB (Default)

The connection string in `appsettings.json` uses LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

#### Option B: Using SQL Server

Update the connection string in:
- `src/TaskManagement.API/appsettings.json`
- `src/TaskManagement.WindowsService/appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagementDb;Integrated Security=True;TrustServerCertificate=True"
}
```

#### Create Database and Run Migrations

```bash
cd src/TaskManagement.API
dotnet ef migrations add InitialCreate --project ../TaskManagement.Infrastructure
dotnet ef database update --project ../TaskManagement.Infrastructure
```

### 3. Seed Initial Data (Optional)

You can seed the database with sample users and tags. Create a seed script or use the API to create initial data.

### 4. RabbitMQ Setup

1. **Install RabbitMQ**:
   - Windows: Download and install from [RabbitMQ Downloads](https://www.rabbitmq.com/download.html)
   - Or use Docker: `docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management`

2. **Verify RabbitMQ is running**:
   - Management UI: http://localhost:15672 (guest/guest)
   - Default port: 5672

3. **Update configuration** (if needed):
   - `src/TaskManagement.WindowsService/appsettings.json`
   ```json
   "RabbitMQ": {
     "HostName": "localhost"
   }
   ```

### 5. Backend Setup

```bash
# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run the API
cd src/TaskManagement.API
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:7000/swagger`

### 6. Frontend Setup

```bash
# Navigate to frontend directory
cd src/TaskManagement.Web

# Install dependencies
npm install

# Update API URL if needed (src/TaskManagement.Web/src/services/api.ts)
# Default: 'https://localhost:7000/api'

# Run the development server
npm run dev
```

The React app will be available at `http://localhost:5173`

### 7. Windows Service Setup (Optional)

To run the Windows Service for task reminders:

```bash
cd src/TaskManagement.WindowsService
dotnet run
```

Or publish and install as a Windows Service:

```bash
dotnet publish -c Release
# Follow Windows Service installation instructions
```

## Database Schema

### Tables

#### Tasks
- `Id` (int, PK)
- `Title` (string, max 200, required)
- `Description` (string, max 1000, required)
- `DueDate` (datetime, required)
- `Priority` (int, required) - Enum: Low(1), Medium(2), High(3), Critical(4)
- `UserId` (int, FK to Users)
- `CreatedAt` (datetime)
- `UpdatedAt` (datetime)

#### Users
- `Id` (int, PK)
- `FullName` (string, max 200, required)
- `Telephone` (string, max 20, required)
- `Email` (string, max 200, required, unique)
- `CreatedAt` (datetime)
- `UpdatedAt` (datetime)

#### Tags
- `Id` (int, PK)
- `Name` (string, max 100, required, unique)
- `Color` (string, max 20, optional)
- `CreatedAt` (datetime)

#### TaskTags (Junction Table)
- `TaskId` (int, FK to Tasks, PK)
- `TagId` (int, FK to Tags, PK)

### Entity Relationships

- **Task** → **User**: Many-to-One (Task belongs to one User)
- **Task** ↔ **Tag**: Many-to-Many (via TaskTags junction table)

## API Documentation

### Base URL
```
https://localhost:7000/api
```

### Endpoints

#### Tasks

- **GET** `/api/tasks` - Get all tasks
- **GET** `/api/tasks/{id}` - Get task by ID
- **POST** `/api/tasks` - Create a new task
- **PUT** `/api/tasks/{id}` - Update a task
- **DELETE** `/api/tasks/{id}` - Delete a task

#### Tags

- **GET** `/api/tags` - Get all tags

### Request/Response Examples

#### Create Task

**Request:**
```json
POST /api/tasks
{
  "title": "Complete project documentation",
  "description": "Write comprehensive README and API documentation",
  "dueDate": "2024-02-15T00:00:00",
  "priority": 3,
  "userId": 1,
  "tagIds": [1, 2, 3]
}
```

**Response:**
```json
{
  "id": 1,
  "title": "Complete project documentation",
  "description": "Write comprehensive README and API documentation",
  "dueDate": "2024-02-15T00:00:00",
  "priority": 3,
  "user": {
    "id": 1,
    "fullName": "John Doe",
    "telephone": "123-456-7890",
    "email": "john@example.com"
  },
  "tags": [
    { "id": 1, "name": "Documentation", "color": "#3498db" },
    { "id": 2, "name": "High Priority", "color": "#e74c3c" }
  ]
}
```

## Running the Application

### Development Mode

1. **Start SQL Server** (LocalDB starts automatically)

2. **Start RabbitMQ**:
   ```bash
   # If using Docker
   docker start some-rabbit
   ```

3. **Start the API**:
   ```bash
   cd src/TaskManagement.API
   dotnet run
   ```

4. **Start the Windows Service** (in a separate terminal):
   ```bash
   cd src/TaskManagement.WindowsService
   dotnet run
   ```

5. **Start the React App** (in a separate terminal):
   ```bash
   cd src/TaskManagement.Web
   npm run dev
   ```

6. **Access the application**:
   - Frontend: http://localhost:5173
   - API Swagger: https://localhost:7000/swagger

## Testing

### Run Backend Tests

```bash
cd src/TaskManagement.Tests
dotnet test
```

### Test Coverage

- **Unit Tests**: Validators, Handlers
- **Integration Tests**: API Controllers

### Example Test

```bash
# Run specific test
dotnet test --filter "FullyQualifiedName~CreateTaskDtoValidatorTests"
```

## SQL Query

### Tasks with at least 2 tags, sorted by number of tags descending

```sql
SELECT 
    t.Id,
    t.Title,
    t.Description,
    t.DueDate,
    t.Priority,
    COUNT(tt.TagId) AS TagCount,
    STRING_AGG(tag.Name, ', ') AS TagNames
FROM Tasks t
INNER JOIN TaskTags tt ON t.Id = tt.TaskId
INNER JOIN Tags tag ON tt.TagId = tag.Id
GROUP BY t.Id, t.Title, t.Description, t.DueDate, t.Priority
HAVING COUNT(tt.TagId) >= 2
ORDER BY TagCount DESC;
```

This query:
- Joins Tasks with TaskTags and Tags tables
- Groups by task properties
- Filters tasks with 2 or more tags (HAVING clause)
- Aggregates tag names into a comma-separated string
- Sorts by tag count in descending order

## Project Structure

```
UserTasks/
├── src/
│   ├── TaskManagement.API/          # REST API layer
│   ├── TaskManagement.Application/  # CQRS commands/queries
│   ├── TaskManagement.Domain/       # Domain entities
│   ├── TaskManagement.Infrastructure/ # EF, RabbitMQ
│   ├── TaskManagement.WindowsService/ # Background service
│   ├── TaskManagement.Web/         # React frontend
│   └── TaskManagement.Tests/       # Unit & integration tests
└── README.md
```

## Troubleshooting

### Common Issues

1. **Database connection errors**:
   - Verify SQL Server is running
   - Check connection string in `appsettings.json`
   - Ensure database exists or run migrations

2. **RabbitMQ connection errors**:
   - Verify RabbitMQ is running: `docker ps` or check Windows services
   - Check port 5672 is not blocked
   - Verify hostname in configuration

3. **CORS errors in browser**:
   - Check CORS configuration in `Program.cs`
   - Verify frontend URL is in allowed origins

4. **API not accessible from frontend**:
   - Check API URL in `src/TaskManagement.Web/src/services/api.ts`
   - Verify SSL certificate (may need to trust dev certificate)

## License

This project is created for educational/demonstration purposes.

## Author

Full-Stack Developer Home Assignment

---

For questions or issues, please refer to the project documentation or create an issue in the repository.
