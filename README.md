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
