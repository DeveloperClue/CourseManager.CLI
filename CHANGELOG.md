# CHANGELOG

## [2024-Nov-20] **Add Course commands (Add, Update, Delete, List, View)**
- Branch: `feature-course-commands`
- Implemented CRUD command handlers for `Course`
- Structured command files under `ConsoleApp/Commands/Course/`
- Introduced `CommandBase` and `ICommand` interface for command pattern
- Added `NotImplementedCommand` as a default fallback

## [2024-Nov-20] **Implement Course repository and service layer**
- Branch: `feature-course-repo-service`
- Added `CourseRepository` for JSON-based data access
- Created `ICourseService` interface and its implementation (`CourseService`)
- Introduced `CourseManagerEventArgs` and `CourseManagerException` for domain-specific events and errors
- Implemented generic `IRepository` support in `JsonFileRepository`
- Registered ILogger using Serilog for logging
- Configured dependency injection for `ICourseService`

## [2024-Nov-18] **Add Course model and repository interfaces**
- Branch: `feature-course-model-repo`
- Added `Course` domain model
- Introduced `IRepository` and `ICourseRepository` interface for abstraction
- Structured under `Core/Models` and `Core/Infrastructure`

## [2024-Nov-16] **Initial project structure and solution files**
- Branch: `init-project-structure`
- Added solution file (.sln) and project files (.csproj)
- Created initial folder structure for the application
- Added .gitignore for standard .NET exclusions
- Included global.json to lock SDK version
- Prepared the repository for initial development