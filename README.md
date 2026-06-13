# FAST Societies Management System

A role-based Windows desktop application for managing university societies, memberships, events, announcements, attendance, tasks, feedback, and operational reporting.

## Features

- Secure login and student self-registration
- Administrator, society-head, and student workspaces
- Society creation, approval, activation, and assignment
- Membership applications and approval workflows
- Event creation, approval, registration, and attendance
- Society task assignment and progress tracking
- Announcements and student feedback
- Dashboard summaries and operational reports
- Password hashing with per-user salts
- Soft deletion and audit fields across core records
- Automatic initial administrator provisioning

## Role Workflows

| Role | Capabilities |
| --- | --- |
| Administrator | Manage societies and accounts, approve events, publish announcements, monitor attendance, and review reports |
| Society Head | Manage events, membership requests, member lists, tasks, announcements, attendance, and reports |
| Student | Browse societies, request membership, register for events, view announcements, submit feedback, and manage profile tickets |

## Technology Stack

| Layer | Technology |
| --- | --- |
| Desktop UI | Windows Forms, Guna UI2 |
| Language | C# |
| Runtime | .NET 10 |
| Architecture | Core, Data Access, Business Logic, UI |
| Database | Microsoft SQL Server |
| Data Access | Microsoft.Data.SqlClient |
| Testing | MSTest, Moq |

## Project Structure

```text
.
|-- Database/
|   `-- schema.sql
|-- src/
|   |-- FAST.SocietiesManagement.Core/
|   |-- FAST.SocietiesManagement.DAL/
|   |-- FAST.SocietiesManagement.BLL/
|   |-- FAST.SocietiesManagement.UI/
|   |-- FAST.SocietiesManagement.Tests/
|   `-- FAST.SocietiesManagement.slnx
|-- .gitignore
`-- README.md
```

## Prerequisites

- Windows 10 or later
- .NET 10 SDK
- SQL Server Express or SQL Server Developer
- SQL Server Management Studio or Azure Data Studio

## Database Setup

1. Open `Database/schema.sql` in SQL Server Management Studio or Azure Data Studio.
2. Connect to the local SQL Server instance.
3. Execute the complete script.
4. Confirm that the `FASTSocietiesDB` database was created.

The default connection uses:

```text
Server=localhost\SQLEXPRESS;Database=FASTSocietiesDB;Trusted_Connection=True;TrustServerCertificate=True;
```

To use another SQL Server instance, set:

```powershell
$env:FAST_SOCIETIES_CONNECTION_STRING="your_connection_string"
```

## Run Locally

Restore dependencies and build the solution:

```powershell
dotnet restore src/FAST.SocietiesManagement.slnx
dotnet build src/FAST.SocietiesManagement.slnx
```

Start the Windows application:

```powershell
dotnet run --project src/FAST.SocietiesManagement.UI/FAST.SocietiesManagement.UI.csproj
```

## Default Administrator

The application creates an administrator automatically when no `admin` account exists:

```text
Username: admin
Password: admin123
```

Change the default password after the first login.

## Tests

Run the complete automated test suite:

```powershell
dotnet test src/FAST.SocietiesManagement.Tests/FAST.SocietiesManagement.Tests.csproj
```

The suite covers business services, validators, repositories, authentication, password hashing, UI controls, and application composition.

## Architecture

| Project | Responsibility |
| --- | --- |
| `Core` | DTOs, enums, shared utilities, password hashing, and logging |
| `DAL` | SQL Server connections, repositories, and persistence |
| `BLL` | Authentication, validation, society operations, memberships, events, and tasks |
| `UI` | Role-aware Windows Forms screens and shared visual styling |
| `Tests` | Unit tests for application layers and user workflows |
