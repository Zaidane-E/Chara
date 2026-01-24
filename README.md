# Task API

Web app built with **C# (.NET 8)** backend and **Angular** frontend.  
Currently supports user tasks, task priorities, due dates, and completion tracking. Includes EF Core migrations, Swagger API, and unit tests.

---

![main-page](img/page.png)

## Features

- Task fields:
  - Description
  - Priority (Low, Medium, High)
  - Due date
  - Completion status
  - Timestamps
- Habit tracking
  - Add daily habits
  - Track completion rate
  - Track completion streak
- Task filtering
- User authentication with JWT
- Optional guest mode (use the app without registering)
- 

## To be added

- Graph user stats
- More customization options for penalties and rewards
- Add streak of successful/failed days in a row
- Journal

## Known Issues
- Dashboard
	- Shows percentage for the wrong day in weekly view
	- Shows tasks as a fraction instead of just showing the number of tasks completed in that day

---

## Technologies

- **Backend:** C#, ASP.NET Core 8, Entity Framework Core 8, SQLite
- **Frontend:** Angular (latest stable), TypeScript
- **Testing:** xUnit
- **Other:** Swagger/OpenAPI, Git

![swagger-page](img/swagger.png)

---

## Setup Instructions

### Backend

1. Navigate to backend folder:

```bash
cd backend/TaskAPI
```
2. Restore packages:

```bash
dotnet restore
```

3. Apply migrations and create database:

```bash
dotnet ef database update
```
    
4. Run the API:

```bash
dotnet run
```

Swagger UI available at https://localhost:<port>/swagger

### Frontend

1. Navigate to frontend folder:

```bash
cd frontend
```

2. Install dependencies:

```bash
npm install
```

3. Run Angular dev server:

```bash
ng serve
```

App available at http://localhost:4200

### Running Tests

```bash
cd backend/TaskAPI.Tests
dotnet test
```
