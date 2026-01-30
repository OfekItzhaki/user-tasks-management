# Project Architecture

This document describes the architecture of the Task Management System, which follows **Clean Architecture** principles with **CQRS (Command Query Responsibility Segregation)** pattern.

## Overview

The application is structured in layers, with dependencies pointing inward toward the core domain. Each layer has a single responsibility and communicates through well-defined interfaces.

## Project Layers

### 1. TaskManagement.Web (Frontend)

**Purpose**: User interface layer

**Technologies**:
- React + TypeScript
- Redux Toolkit for state management
- React Hook Form + Yup for form validation
- Axios for HTTP calls to API

**Responsibilities**:
- Render user interface
- Handle user interactions
- Manage client-side state
- Validate user input
- Communicate with backend API

---

### 2. TaskManagement.API (Presentation Layer)

**Purpose**: REST API entry point

**Technologies**:
- ASP.NET Core
- Swagger/OpenAPI
- CORS middleware
- Validation middleware

**Responsibilities**:
- Expose REST endpoints (Tasks, Tags, Users, Seed)
- Handle HTTP requests/responses
- Configure CORS
- Apply validation middleware
- Delegate business logic to Application layer
- Provide API documentation (Swagger)

**Endpoints**:
- `GET/POST/PUT/DELETE /api/tasks`
- `GET /api/tags`
- `GET /api/users`
- `POST /api/seed`

---

### 3. TaskManagement.Application (Business Logic Layer)

**Purpose**: Application business logic and orchestration

**Technologies**:
- MediatR (CQRS implementation)
- FluentValidation
- Manual entity-to-DTO mapping (in handlers)
- Input sanitization utilities

**Responsibilities**:
- Implement CQRS pattern (Commands and Queries)
- Validate business rules
- Map between DTOs and domain entities
- Sanitize user input
- Orchestrate domain operations

**Commands** (Write operations):
- `CreateTaskCommand` - Create a new task
- `UpdateTaskCommand` - Update an existing task
- `DeleteTaskCommand` - Delete a task

**Queries** (Read operations):
- `GetTasksQuery` - Get all tasks with filtering/sorting
- `GetTaskByIdQuery` - Get a single task by ID
- `GetTagsQuery` - Get all tags
- `GetTasksWithMultipleTagsQuery` - Get tasks with multiple tags

**Components**:
- DTOs (Data Transfer Objects) for request/response
- Validators (FluentValidation) for input validation
- Manual mappings in handlers for entity-to-DTO conversion
- Handlers (MediatR) for command/query processing

---

### 4. TaskManagement.Domain (Core Domain Layer)

**Purpose**: Core business entities and rules

**Technologies**:
- Pure C# classes (no external dependencies)

**Responsibilities**:
- Define domain entities
- Define business rules
- Define enums and value objects
- Represent the core business model

**Entities**:
- `Task` - Task entity with title, description, due date, priority
- `User` - User entity with full name, email, telephone
- `Tag` - Tag entity with name and color
- `TaskTag` - Junction table for Task-Tag many-to-many relationship
- `UserTask` - Junction table for Task-User many-to-many relationship with roles

**Enums**:
- `Priority` - Task priority levels (Low, Medium, High, Critical)
- `UserTaskRole` - User roles in tasks (Owner, Assignee, Watcher)

**Key Principle**: This layer has **no dependencies** on other layers. It is the core of the application.

---

### 5. TaskManagement.Infrastructure (Data Access Layer)

**Purpose**: External concerns and data persistence

**Technologies**:
- Entity Framework Core
- SQL Server
- RabbitMQ.Client

**Responsibilities**:
- Database access via EF Core DbContext
- Database migrations
- RabbitMQ message queue integration
- Configuration management
- Implement interfaces defined in Application layer

**Components**:
- `TaskManagementDbContext` - EF Core database context
- `IRabbitMQService` - RabbitMQ integration service
- Migrations - Database schema management
- Connection string configuration

---

### 6. TaskManagement.WindowsService (Background Service)

**Purpose**: Background processing and task reminders

**Technologies**:
- .NET Background Service
- RabbitMQ.Client

**Responsibilities**:
- Run as a background service
- Check for overdue tasks periodically (every minute)
- Publish reminder messages to RabbitMQ queue
- Consume reminder messages from RabbitMQ queue
- Log reminder notifications

**Features**:
- Automated task reminder system
- RabbitMQ publisher/consumer pattern
- Configurable check interval

---

### 7. TaskManagement.Tests (Testing Layer)

**Purpose**: Automated testing

**Technologies**:
- xUnit - Testing framework
- Moq - Mocking framework
- FluentAssertions - Assertion library
- Microsoft.AspNetCore.Mvc.Testing - Integration testing

**Responsibilities**:
- Unit tests for handlers and validators
- Integration tests for API controllers
- Test business logic in isolation
- Verify API endpoints work correctly

**Test Types**:
- **Unit Tests**: Test individual components (handlers, validators)
- **Integration Tests**: Test API endpoints end-to-end

---

## Data Flow

```
┌─────────────────┐
│   Frontend      │
│  (React/TS)     │
└────────┬────────┘
         │ HTTP/REST
         ▼
┌─────────────────┐
│      API        │
│  (Controllers)  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Application    │
│  (CQRS/MediatR) │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Domain       │
│   (Entities)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Infrastructure  │
│  (EF Core/DB)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Database      │
│  (SQL Server)   │
└─────────────────┘

         │
         ▼
┌─────────────────┐
│ Windows Service │
│  (Background)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    RabbitMQ     │
│  (Message Queue)│
└─────────────────┘
```

## Key Design Patterns

### 1. Clean Architecture

**Principle**: Dependencies point inward toward the core domain.

- **Domain** has no dependencies (innermost layer)
- **Application** depends only on Domain
- **Infrastructure** depends on Domain and Application
- **API** depends on Application (and indirectly on Domain)
- **Frontend** is independent (client-side)

**Benefits**:
- Testability - Core logic can be tested without external dependencies
- Maintainability - Changes to external concerns don't affect core business logic
- Flexibility - Easy to swap implementations (e.g., different databases)

### 2. CQRS (Command Query Responsibility Segregation)

**Principle**: Separate read and write operations.

- **Commands** (writes): `CreateTaskCommand`, `UpdateTaskCommand`, `DeleteTaskCommand`
- **Queries** (reads): `GetTasksQuery`, `GetTaskByIdQuery`, `GetTagsQuery`

**Benefits**:
- Clear separation of concerns
- Optimized read and write paths
- Easier to scale read and write operations independently
- Better testability

**Implementation**: MediatR library handles command/query routing and processing.

### 3. Dependency Injection

**Principle**: Dependencies are injected rather than created directly.

**Usage**: Used throughout all layers via .NET's built-in DI container.

**Benefits**:
- Loose coupling between components
- Easy to swap implementations
- Better testability (can inject mocks)
- Centralized dependency management

### 4. Repository Pattern

**Principle**: Abstract data access behind an interface.

**Implementation**: Entity Framework Core DbContext acts as the repository.

**Benefits**:
- Decouples business logic from data access
- Easy to test (can mock DbContext)
- Centralized data access logic

## Layer Communication Rules

1. **Frontend** → **API**: HTTP/REST calls
2. **API** → **Application**: MediatR commands/queries
3. **Application** → **Domain**: Direct entity manipulation
4. **Application** → **Infrastructure**: Interface-based (dependency inversion)
5. **Infrastructure** → **Domain**: Entity Framework mappings
6. **Windows Service** → **Infrastructure**: Direct access to DbContext and RabbitMQ

## Single Responsibility Principle

Each layer has a **single, well-defined responsibility**:

- **Web**: User interface and client-side logic
- **API**: HTTP request handling and routing
- **Application**: Business logic orchestration
- **Domain**: Core business entities and rules
- **Infrastructure**: Data persistence and external services
- **Windows Service**: Background processing
- **Tests**: Automated verification

## Benefits of This Architecture

1. **Testability**: Each layer can be tested in isolation
2. **Maintainability**: Clear separation makes changes easier
3. **Scalability**: Can scale layers independently
4. **Flexibility**: Easy to swap implementations
5. **Clarity**: Clear structure makes onboarding easier
6. **Reusability**: Domain logic can be reused across different interfaces

## Technology Stack Summary

| Layer | Technologies |
|-------|-------------|
| Frontend | React, TypeScript, Redux Toolkit, React Hook Form, Yup, Axios |
| API | ASP.NET Core, Swagger, CORS |
| Application | MediatR, FluentValidation, manual mapping |
| Domain | Pure C# (no dependencies) |
| Infrastructure | Entity Framework Core, SQL Server, RabbitMQ.Client |
| Windows Service | .NET Background Service, RabbitMQ.Client |
| Tests | xUnit, Moq, FluentAssertions, ASP.NET Core Testing |

## Further Reading

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
