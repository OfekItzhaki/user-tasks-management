# Task Management System

A full-stack web application for managing user tasks with a .NET Core backend, React frontend, SQL Server database, and Windows Service with RabbitMQ integration.

## Quick Links

- 🆕 **[FRESH_MACHINE_SETUP.md](instructions/FRESH_MACHINE_SETUP.md)** - **Start here if setting up on a new machine**
- 📦 **[INSTALL.md](instructions/INSTALL.md)** - First time setup
- 🚀 **[RUN.md](instructions/RUN.md)** - How to run the application
- ⚙️ **[CONFIG.md](instructions/CONFIG.md)** - Connection strings and secrets (env / User Secrets)
- 🔧 **[TROUBLESHOOTING.md](instructions/TROUBLESHOOTING.md)** - Common issues and fixes

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Database Schema](#database-schema)
- [API Documentation](#api-documentation)
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
│  - Mappings (manual entity-to-DTO conversion)                                    │
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

- ✅ **Full CRUD operations** for tasks (Create, Read, Update, Delete)
- ✅ **User management** with contact details (Full Name, Email, Telephone)
- ✅ **Multiple tags per task** (N:N relationship) with visual color coding
- ✅ **Priority levels** (Low, Medium, High, Critical) with visual indicators
- ✅ **Due date tracking** with validation
- ✅ **Comprehensive validation** (frontend Yup + backend FluentValidation)
- ✅ **Search & Filtering** by title, description, priority, user, tags
- ✅ **Sorting** by title, due date, priority, creation date
- ✅ **Pagination** for efficient data handling
- ✅ **Responsive React UI** with dark mode support
- ✅ **Windows Service** for automated task reminders
- ✅ **RabbitMQ integration** for message queuing
- ✅ **Unit and integration tests**
- ✅ **Clean Architecture** with CQRS pattern
- ✅ **Custom hooks** for reusable logic (React)

## Technology Stack

### Backend
- **.NET 8.0** - Core framework
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Database
- **MediatR** - CQRS implementation
- **FluentValidation** - Validation
- **Manual mapping** - Entity-to-DTO conversion in handlers
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

### For Docker Setup (Recommended)

The setup script (`.\setup-docker.ps1`) will check for these automatically:

1. **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verify: `dotnet --version` (should show 8.0.x)
2. **Node.js 20.19+ or 22.12+** and **npm** - [Download](https://nodejs.org/)
   - Verify: `node --version` and `npm --version`
3. **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
   - Includes Docker Compose
   - Verify: `docker --version` and `docker compose version`
4. **Visual Studio 2022** or **VS Code** (optional, for development)

**That's it!** No need to install SQL Server or RabbitMQ - Docker handles it.

### For Local Setup (Traditional)

If using `.\setup.ps1` instead:

1. **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **SQL Server LocalDB** (recommended) or SQL Server Express
   - Usually comes with Visual Studio
   - Or download: [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
   - Verify: `sqllocaldb info mssqllocaldb`
3. **Node.js 20.19+ or 22.12+** and **npm** - [Download](https://nodejs.org/)
4. **RabbitMQ** (optional, for Windows Service)
   - **Recommended**: Use Docker: `docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management`
   - Or install locally: [RabbitMQ Downloads](https://www.rabbitmq.com/download.html)
5. **Visual Studio 2022** or **VS Code** (optional, for development)

## Setup Instructions

> **💡 Tip**: If you're on a new machine, **double-click `First setup.bat`** or run `.\First setup.ps1` to get started! (see [Quick Start](#-quick-start) above)

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

### 3. Seed Initial Data (Recommended)

After starting the API, seed the database with sample users and tags:

**Option 1: Using Swagger UI** (easiest)
1. Navigate to: http://localhost:5063/swagger
2. Find the `POST /api/seed` endpoint
3. Click "Try it out" → "Execute"

**Option 2: Using curl/PowerShell**
```powershell
Invoke-WebRequest -Uri "http://localhost:5063/api/seed" -Method POST
```

This will create sample users, tags, and tasks for testing.

### 4. RabbitMQ Setup

**Option A: Using Docker Compose (Recommended)**
- RabbitMQ is automatically started with `.\setup-docker.ps1`
- Or manually: `docker compose up -d rabbitmq`
- Management UI: http://localhost:15672 (guest/guest)

**Option B: Using Docker (Standalone)**
```powershell
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

**Option C: Local Installation**
- Windows: Download and install from [RabbitMQ Downloads](https://www.rabbitmq.com/download.html)

**Verify RabbitMQ is running**:
- Management UI: http://localhost:15672 (guest/guest)
- Default port: 5672

**Update configuration** (if needed):
- `src/TaskManagement.WindowsService/appsettings.json`
  ```json
  "RabbitMQ": {
    "HostName": "localhost"  // Use "rabbitmq" if running in Docker network
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

The Windows Service monitors tasks and sends reminders for overdue tasks via RabbitMQ.

**Queue Name**: `Remainder`

**Log Format**: `"Hi your Task is due {TaskTitle}"`

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

**Note**: The service will:
- Check for overdue tasks every minute
- Publish reminders to the "Remainder" queue
- Subscribe to the queue and log each reminder message

## Database Schema

### Tables

#### Tasks
- `Id` (int, PK)
- `Title` (string, max 200, required)
- `Description` (string, max 1000, required)
- `DueDate` (datetime, required)
- `Priority` (int, required) - Enum: Low(1), Medium(2), High(3), Critical(4)
- `CreatedByUserId` (int, FK to Users) - User who created the task
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

#### UserTasks (Junction Table - Many-to-Many)
- `TaskId` (int, FK to Tasks, PK)
- `UserId` (int, FK to Users, PK)
- `Role` (int, required) - Enum: Owner(1), Assignee(2), Watcher(3)
- `AssignedAt` (datetime, required)

#### TaskTags (Junction Table)
- `TaskId` (int, FK to Tasks, PK)
- `TagId` (int, FK to Tags, PK)

### Entity Relationships

- **Task** ↔ **User**: Many-to-Many (via UserTasks junction table with roles)
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

## 🚀 Quick Start

### 🆕 Easiest Way: First Setup (Recommended)

**Double-click `First setup.bat`** in the project root folder, or run:

```powershell
.\First setup.ps1
```

This will:
1. Prompt you to choose between **Docker** or **Local** setup
2. Run the fully automated setup script for your choice
3. Handle everything automatically (database, dependencies, builds, migrations, services)

**That's it!** No need to run separate scripts. Everything is self-contained.

### Option 1: Docker Setup (Recommended) 🐳

**No need to install SQL Server or RabbitMQ locally!** Uses Docker Compose:

**Via First Setup:**
- Double-click `First setup.bat` → Choose option `1` (Docker)

**Or run directly:**
```powershell
.\scripts\quick-start\quick-start-docker-automated.ps1
```

This single command will:
- ✅ **Check prerequisites** (.NET 8.0 SDK, Node.js 20+, Docker Desktop)
- ✅ **Start Docker Desktop** (if not running, with auto-fix for stuck states)
- ✅ **Start Docker services** (SQL Server and RabbitMQ in containers)
- ✅ **Install missing tools** (dotnet-ef tool automatically)
- ✅ **Update connection strings** (for API and Windows Service)
- ✅ **Set up database** (restore packages, build projects, create database and run migrations)
- ✅ **Install frontend dependencies** (npm packages)
- ✅ **Build the solution** (compile all projects)
- ✅ **Start all services** (API, Frontend, Windows Service)

**That's it!** Everything runs automatically. After it completes, you'll have:
- Frontend running at: http://localhost:5173
- API Swagger at: http://localhost:5063/swagger
- Windows Service running (Development mode)
- SQL Server in Docker: localhost:1433
- RabbitMQ in Docker: localhost:5672 (Management UI: http://localhost:15672)

**Benefits:**
- ✅ No local SQL Server installation needed
- ✅ No local RabbitMQ installation needed
- ✅ Consistent environment across machines
- ✅ Easy cleanup: `docker compose down`
- ✅ Fully self-contained - no need for separate setup scripts

### Option 2: Local Setup (Without Docker)

If you prefer local installations or don't have Docker:

**Via First Setup:**
- Double-click `First setup.bat` → Choose option `2` (Local)

**Or run directly:**
```powershell
.\scripts\quick-start\quick-start-local-automated.ps1
```

This requires:
- SQL Server LocalDB (comes with Visual Studio) or SQL Server Express
- RabbitMQ installed locally or via Docker (optional)

The script will:
- ✅ **Check prerequisites** (.NET 8.0 SDK, Node.js 20+, LocalDB)
- ✅ **Start LocalDB** automatically
- ✅ **Install missing tools** (dotnet-ef tool automatically)
- ✅ **Set up database** (restore packages, build projects, create database and run migrations)
- ✅ **Install frontend dependencies** (npm packages)
- ✅ **Build the solution** (compile all projects)
- ✅ **Start all services** (API, Frontend, Windows Service)

**Benefits:**
- ✅ Fully self-contained - no need for separate setup scripts
- ✅ Works without Docker Desktop
- ✅ Uses LocalDB (usually already installed with Visual Studio)

### For Existing Setup (Quick Run)

If everything is already set up, just start the services:

```powershell
.\scripts\start-all.ps1
```

This will start all services without running setup checks.

**For Docker setup**, make sure Docker services are running:
```powershell
docker compose -f docker\docker-compose.yml up -d
```

### 📖 Manual Setup (If Scripts Don't Work)

If you prefer manual setup or the scripts don't work, follow these steps:

1. **Install Prerequisites** (if not already installed):
   - [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
   - [Node.js 20+](https://nodejs.org/)
   - SQL Server LocalDB (comes with Visual Studio) or [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
   - [RabbitMQ](https://www.rabbitmq.com/download.html) or use Docker: `docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management`

2. **Install dotnet-ef tool**:
   ```powershell
   dotnet tool install --global dotnet-ef
   ```

3. **Set up Database**:
   ```powershell
   cd src\TaskManagement.API
   dotnet ef database update --project ..\TaskManagement.Infrastructure
   ```

4. **Install Frontend Dependencies**:
   ```powershell
   cd src\TaskManagement.Web
   npm install
   ```

5. **Build Solution**:
   ```powershell
   cd ..\..
   dotnet build
   ```

6. **Start Services** (see [Running the Application](#running-the-application) section below)

See [QUICK_START.md](QUICK_START.md) for more detailed manual setup instructions.

## Running the Application

### Development Mode

#### Using Docker Setup

1. **Start Docker services** (if not already running):
   ```powershell
   docker compose up -d
   ```
   This starts SQL Server and RabbitMQ in containers.

2. **Start the API**:
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
   - API Swagger: http://localhost:5063/swagger

## Testing

### Run All Tests (Backend + Frontend)

Run both backend and frontend tests with a single command:

```powershell
.\run-all-tests.ps1
```

This will:
- Run all .NET backend tests (xUnit)
- Run all frontend tests (Vitest + React Testing Library)
- Show a summary of results

### Backend Tests

```powershell
cd src/TaskManagement.Tests
dotnet test
```

### Frontend Tests

```powershell
cd src/TaskManagement.Web
npm run test
```

For interactive testing:
```powershell
npm run test:ui
```

For coverage:
```powershell
npm run test:coverage
```

### Test Coverage

- **Backend Unit Tests**: Validators, Handlers
- **Backend Integration Tests**: API Controllers
- **Frontend Component Tests**: TaskForm, TaskList, TaskFilters
- **Frontend Security Tests**: XSS protection, HTML encoding

### Example Test

```bash
# Run specific backend test
dotnet test --filter "FullyQualifiedName~CreateTaskDtoValidatorTests"

# Run specific frontend test
npm run test -- TaskForm.test.tsx
```

## SQL Query

### Tasks with at least 2 tags, sorted by number of tags descending

This SQL query is **implemented as a stored procedure** and executable via the API endpoint: `GET /api/tasks/with-multiple-tags?minTagCount=2`

**Stored Procedure: `GetTasksWithMultipleTags`**

```sql
CREATE PROCEDURE [dbo].[GetTasksWithMultipleTags]
    @MinTagCount INT = 2
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.Id,
        t.Title,
        t.Description,
        t.DueDate,
        t.Priority,
        COUNT(DISTINCT tt.TagId) AS TagCount,
        STRING_AGG(tag.Name, ', ') WITHIN GROUP (ORDER BY tag.Name) AS TagNames,
        COUNT(DISTINCT ut.UserId) AS UserCount,
        STRING_AGG(u.FullName, ', ') WITHIN GROUP (ORDER BY u.FullName) AS AssignedUsers
    FROM Tasks t
    INNER JOIN TaskTags tt ON t.Id = tt.TaskId
    INNER JOIN Tags tag ON tt.TagId = tag.Id
    LEFT JOIN UserTasks ut ON t.Id = ut.TaskId
    LEFT JOIN Users u ON ut.UserId = u.Id
    GROUP BY t.Id, t.Title, t.Description, t.DueDate, t.Priority
    HAVING COUNT(DISTINCT tt.TagId) >= @MinTagCount
    ORDER BY TagCount DESC;
END
```

**Implementation:**
- ✅ **Stored Procedure**: `GetTasksWithMultipleTags` (created via EF Core migration)
- ✅ **Executed via API**: `GET /api/tasks/with-multiple-tags`
- ✅ **Query Handler**: `GetTasksWithMultipleTagsQueryHandler` calls the stored procedure
- ✅ **Configurable**: `minTagCount` parameter (default: 2)
- ✅ **Swagger Documentation**: Available in Swagger UI with example response

**API Usage:**
```bash
# Get tasks with at least 2 tags (default)
GET /api/tasks/with-multiple-tags

# Get tasks with at least 3 tags
GET /api/tasks/with-multiple-tags?minTagCount=3
```

**Response Example:**
```json
[
  {
    "id": 1,
    "title": "Task with Multiple Tags",
    "description": "This task has multiple tags",
    "dueDate": "2024-12-31T00:00:00Z",
    "priority": 2,
    "tagCount": 3,
    "tagNames": "Urgent, Frontend, Backend",
    "userCount": 2,
    "assignedUsers": "John Doe, Jane Smith"
  }
]
```

**Query Details:**
- **Stored Procedure**: `GetTasksWithMultipleTags` with parameter `@MinTagCount`
- Joins Tasks with TaskTags, Tags, UserTasks, and Users tables
- Groups by task properties
- Filters tasks with specified minimum number of tags (HAVING clause)
- Aggregates tag names and assigned users into comma-separated strings using `STRING_AGG` with ordering
- Sorted by number of tags in descending order
- Shows both tag count and user count
- Tag names and user names are sorted alphabetically within each aggregation

**Testing via Swagger:**
1. Start the API: `cd src/TaskManagement.API && dotnet run`
2. Navigate to Swagger UI: `https://localhost:7000/swagger` or `http://localhost:5063/swagger`
3. Find endpoint: `GET /api/tasks/with-multiple-tags`
4. Click "Try it out"
5. Enter `minTagCount` (default: 2)
6. Click "Execute"
7. View the response with example data

## Additional Improvements & Future Enhancements

The following improvements could enhance the application further:

### High Priority
1. **Users API Endpoint** - Add `/api/users` endpoint to fetch users dynamically (currently using mock data in frontend)
2. **Task Status** - Add status field (Not Started, In Progress, Completed, Cancelled) with workflow management
3. **Pagination** - Implement pagination for tasks list to handle large datasets efficiently
4. **Search & Filtering** - Add search by title/description and filters by priority, due date, users, tags
5. **Sorting** - Add sorting options (by due date, priority, creation date, etc.)

### Medium Priority
6. **Task Comments** - Allow users to add comments/notes to tasks for collaboration
7. **Task History/Audit Log** - Track changes to tasks (who changed what and when)
8. **Email Notifications** - Send email notifications for task assignments and due date reminders
9. **File Attachments** - Allow attaching files to tasks
10. **Task Templates** - Create reusable task templates for common workflows

### Nice to Have
11. **Recurring Tasks** - Support for recurring/repeating tasks
12. **Task Dependencies** - Link tasks that depend on each other
13. **Export Functionality** - Export tasks to CSV, PDF, or Excel
14. **Dashboard/Analytics** - Visual dashboard with task statistics and charts
15. **Real-time Updates** - WebSocket support for real-time task updates
16. **Mobile App** - Native mobile application
17. **Task Categories/Projects** - Organize tasks into projects or categories
18. **Time Tracking** - Track time spent on tasks
19. **Better Error Handling** - More detailed error messages and user-friendly error pages
20. **Loading States** - Better loading indicators and skeleton screens

### Technical Improvements
- Add API versioning
- Implement caching (Redis) for frequently accessed data
- Add rate limiting to API endpoints
- Implement comprehensive logging and monitoring
- Add health checks endpoint
- Implement API authentication/authorization (JWT)
- Add unit test coverage for frontend components
- Add E2E tests with Playwright or Cypress
- Implement CI/CD pipeline
- Add Docker containerization
- Add database seeding scripts
- **Backend API enhancements** - Add pagination, search, and sorting parameters to GET /api/tasks endpoint
- **User Management CRUD** - Full CRUD operations for users (currently only seed data)
- **Tag Management CRUD** - Full CRUD operations for tags (currently only seed data)
- **Bulk Operations** - Bulk delete, update, and assign tasks
- **Security Hardening** - Input sanitization, XSS/CSRF protection
- **Performance Optimization** - Database indexing strategy, query optimization
- **Accessibility** - Keyboard navigation, screen reader support, ARIA labels
- **Internationalization** - Multi-language support

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

### Script Execution Policy Error

If you get: `cannot be loaded because running scripts is disabled on this system`

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

Then run `.\setup.ps1` again.

### Database Connection Errors

**Problem**: Cannot connect to database

**Solutions**:

**For Docker Setup:**
1. **Verify Docker services are running**:
   ```powershell
   docker compose ps
   docker compose logs sqlserver
   ```

2. **Check connection string** in `src/TaskManagement.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```

3. **Restart SQL Server container**:
   ```powershell
   docker compose restart sqlserver
   ```

4. **Re-run migrations**:
   ```powershell
   cd src\TaskManagement.API
   dotnet ef database update --project ..\TaskManagement.Infrastructure
   ```

**For Local Setup:**
1. **Verify SQL Server LocalDB is running**:
   ```powershell
   sqllocaldb start mssqllocaldb
   sqllocaldb info mssqllocaldb
   ```

2. **Check connection string** in `src/TaskManagement.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

3. **Re-run migrations**:
   ```powershell
   cd src\TaskManagement.API
   dotnet ef database update --project ..\TaskManagement.Infrastructure
   ```

### RabbitMQ Connection Errors

**Problem**: Windows Service can't connect to RabbitMQ

**Solutions**:

**For Docker Setup:**
1. **Check if RabbitMQ container is running**:
   ```powershell
   docker compose ps
   docker compose logs rabbitmq
   ```

2. **Start RabbitMQ container**:
   ```powershell
   docker compose up -d rabbitmq
   ```

3. **Verify RabbitMQ Management UI**: http://localhost:15672 (guest/guest)

4. **Check configuration** in `src/TaskManagement.WindowsService/appsettings.json`:
   ```json
   "RabbitMQ": {
     "HostName": "localhost"  // Use "rabbitmq" if running in Docker network
   }
   ```

**For Local Setup:**
1. **Check if RabbitMQ is running**:
   ```powershell
   docker ps --filter "name=rabbit"
   ```

2. **Start RabbitMQ Docker container**:
   ```powershell
   docker start some-rabbit
   # Or create new container:
   docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

3. **Verify RabbitMQ Management UI**: http://localhost:15672 (guest/guest)

4. **Check configuration** in `src/TaskManagement.WindowsService/appsettings.json`:
   ```json
   "RabbitMQ": {
     "HostName": "localhost"
   }
   ```

### Port Already in Use

**Problem**: Port 5063 (API) or 5173 (Frontend) is already in use

**Solutions**:
1. **Find and stop the process using the port**:
   ```powershell
   # Find process on port 5063
   netstat -ano | findstr :5063
   # Kill the process (replace PID with actual process ID)
   taskkill /PID <PID> /F
   ```

2. **Or change ports** in:
   - API: `src/TaskManagement.API/Properties/launchSettings.json`
   - Frontend: `src/TaskManagement.Web/vite.config.ts`

### CORS Errors in Browser

**Problem**: Frontend can't connect to API (CORS error)

**Solutions**:
1. **Check API is running**: http://localhost:5063/swagger
2. **Verify CORS configuration** in `src/TaskManagement.API/Program.cs`
3. **Check API URL** in `src/TaskManagement.Web/src/services/api.ts`:
   ```typescript
   const API_BASE_URL = import.meta.env.VITE_API_URL || (import.meta.env.DEV ? '/api' : 'https://localhost:7000/api');
   ```

### SSL Certificate Errors

**Problem**: Browser shows SSL certificate warning

**Solutions**:
1. **Trust development certificate**:
   ```powershell
   dotnet dev-certs https --trust
   ```

2. **Or use HTTP instead of HTTPS** (update API URL in frontend)

### Windows Service Not Starting

**Problem**: Windows Service fails to start

**Solutions**:
1. **Run in Development Mode** (console app) instead:
   ```powershell
   cd src\TaskManagement.WindowsService
   dotnet run
   ```

2. **Check Event Viewer** for errors:
   ```powershell
   eventvwr.msc
   # Navigate to: Windows Logs > Application
   ```

3. **Verify connection string** in `src/TaskManagement.WindowsService/appsettings.json`

### Frontend Build Errors

**Problem**: `npm install` or `npm run dev` fails

**Solutions**:
1. **Clear npm cache**:
   ```powershell
   npm cache clean --force
   ```

2. **Delete node_modules and reinstall**:
   ```powershell
   cd src\TaskManagement.Web
   Remove-Item -Recurse -Force node_modules
   Remove-Item package-lock.json
   npm install
   ```

3. **Check Node.js version** (should be 20.19+ or 22.12+):
   ```powershell
   node --version
   ```

### Still Having Issues?

1. **Check logs**:
   - API: Check console output
   - Frontend: Check browser console (F12)
   - Windows Service: Check console or Event Viewer

2. **Verify all prerequisites** are installed:
   ```powershell
   dotnet --version
   node --version
   npm --version
   sqllocaldb info mssqllocaldb
   ```

3. **Try manual setup** (see [Manual Setup](#-manual-setup-if-scripts-dont-work) section)

4. **Check GitHub Issues** or create a new issue with:
   - Error messages
   - Steps to reproduce
   - Your environment (OS, .NET version, Node version)

## Project Structure

```
UserTasks/
├── src/
│   ├── TaskManagement.API/          # REST API layer (.NET Core)
│   ├── TaskManagement.Application/   # CQRS commands/queries (MediatR)
│   ├── TaskManagement.Domain/        # Domain entities and enums
│   ├── TaskManagement.Infrastructure/ # EF Core, RabbitMQ, Data access
│   ├── TaskManagement.WindowsService/ # Background service for reminders
│   ├── TaskManagement.Web/          # React frontend (TypeScript)
│   └── TaskManagement.Tests/        # Unit & integration tests
├── First setup.bat                   # First-time setup launcher (double-click to run)
├── First setup.ps1                  # First-time setup script (prompts for Docker/Local)
├── scripts/
│   ├── quick-start/
│   │   ├── quick-start-docker-automated.ps1  # Fully automated Docker setup
│   │   └── quick-start-local-automated.ps1   # Fully automated Local setup
│   ├── start-all.ps1                # Start all services (existing setup)
│   └── setup.ps1                    # Legacy setup script (still works)
├── README.md                        # This file
└── instructions/                    # Detailed documentation
```

## Getting Help

### Documentation Files
- **README.md** (this file) - Overview and quick start
- **QUICK_START.md** - Detailed manual setup instructions
- **REQUIREMENTS_REVIEW.md** - Requirements compliance checklist
- **PROJECT_RECOMMENDATIONS.md** - Future enhancement suggestions
- **SETUP_IMPROVEMENTS.md** - Setup automation improvements analysis

### Common Commands

```powershell
# First-time setup (easiest - double-click First setup.bat or run this)
.\First setup.ps1

# Or run quick-start scripts directly:
.\scripts\quick-start\quick-start-docker-automated.ps1  # Docker setup
.\scripts\quick-start\quick-start-local-automated.ps1   # Local setup

# Quick start (existing setup)
.\scripts\start-all.ps1

# Database migrations
cd src\TaskManagement.API
dotnet ef database update --project ..\TaskManagement.Infrastructure

# Run tests
.\scripts\run-all-tests.ps1

# Build solution
dotnet build
```

## License

This project is created for educational/demonstration purposes.

## Author
Ofek Itzhaki

---

**Need help?** Check the [Troubleshooting](#troubleshooting) section above or refer to the project documentation files.
