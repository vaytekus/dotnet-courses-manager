# dotnet-courses-manager

Desktop application for managing courses, groups, students and teachers built with Avalonia UI and Entity Framework Core.

## Features

- Browse courses and groups in a tree view with expandable student lists
- Create, edit and delete groups with inline editing
- Assign teachers to groups
- Import / export student lists via CSV
- Export group student list to PDF or DOCX

## Stack

- .NET 9 / C#
- Avalonia UI 11
- Entity Framework Core + SQL Server
- CommunityToolkit.Mvvm
- QuestPDF
- DocumentFormat.OpenXml

## Database

Start SQL Server via Docker:

```bash
docker-compose up -d
```

## Configuration

Copy the example config and fill in your values:

```bash
cp src/Courses.App/appsettings.example.json src/Courses.App/appsettings.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CoursesDb;User Id=sa;Password=Password12345;TrustServerCertificate=True"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Avalonia": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

`appsettings.json` is excluded from git. Logs are written to `src/Logs/log-YYYYMMDD.txt`, a new file is created each day.

## Run

```bash
dotnet run --project src/Courses.App
```
