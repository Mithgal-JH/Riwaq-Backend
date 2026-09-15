
# رِواق | Riwaq — Backend API

Backend API for **رِواق | Riwaq**, an educational knowledge and experience-sharing platform developed by BinX Tech Team 3.

The backend provides the API, database integration, authentication-related services, and core backend functionality for the Riwaq platform.

---

## Tech Stack

- ASP.NET Core Web API
- .NET 10
- C#
- Entity Framework Core
- PostgreSQL
- Swagger / OpenAPI
- Docker
- Railway

---

## Project Structure

```text
Riwaq-Backend/
│
├── Team3.Backend/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Entities/
│   ├── Migrations/
│   ├── Services/
│   ├── Program.cs
│   └── appsettings.json
│
├── docs/
├── Dockerfile
├── .gitignore
└── README.md
```

> The project structure may change as development continues.

---

## Requirements

Install the following tools before running the project:

- [.NET SDK 10](https://dotnet.microsoft.com/en-us/download)
- [Git](https://git-scm.com/downloads)
- [PostgreSQL](https://www.postgresql.org/download/) for local development
- Visual Studio or Visual Studio Code
- Entity Framework Core CLI

Install the EF Core CLI if it is not installed:

```bash
dotnet tool install --global dotnet-ef
```

If it is already installed, update it:

```bash
dotnet tool update --global dotnet-ef
```

---

## Clone the Repository

Clone the repository:

```bash
git clone https://github.com/Mithgal-JH/Riwaq-Backend.git
```

Move into the project directory:

```bash
cd Riwaq-Backend/Team3.Backend
```

---

## Restore and Build the Project

Restore the project packages:

```bash
dotnet restore
```

Build the project:

```bash
dotnet build
```

---

## Database Configuration

The project uses PostgreSQL with Entity Framework Core.

### Local Database

Create a PostgreSQL database named:

```text
team3_backend_db
```

Initialize User Secrets:

```bash
dotnet user-secrets init
```

Configure the local database connection:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=team3_backend_db;Username=postgres;Password=YOUR_PASSWORD"
```

Replace `YOUR_PASSWORD` with your local PostgreSQL password.

> Do not commit passwords, connection strings, or secret keys to GitHub.

---

## Entity Framework Core Migrations

### Create a New Migration

After modifying entities or database relationships, create a migration:

```bash
dotnet ef migrations add MigrationName
```

Example:

```bash
dotnet ef migrations add AddUserSkills
```

### Apply the Migration

For a local PostgreSQL database:

```bash
dotnet ef database update
```

---

## Railway PostgreSQL Database

The production PostgreSQL database is hosted on Railway.

To connect securely to the Railway database, use the Railway PostgreSQL tunnel.

### 1. Open the Railway Tunnel

Run:

```bash
railway connect Postgres --tunnel-only
```

The command will display a local host and port similar to:

```text
Host: 127.0.0.1
Port: 57594
```

Keep the tunnel terminal open while using the database.

### 2. Configure the Database URL

Open another terminal and set the `DATABASE_URL` variable:

```powershell
$env:DATABASE_URL="postgresql://postgres:PASSWORD@127.0.0.1:PORT/railway"
```

Replace:

- `PASSWORD` with the Railway PostgreSQL password.
- `PORT` with the port displayed by the tunnel command.

Example:

```powershell
$env:DATABASE_URL="postgresql://postgres:YOUR_PASSWORD@127.0.0.1:57594/railway"
```

### 3. Apply Migrations to Railway

```bash
dotnet ef database update
```

> The Railway tunnel must remain open while applying migrations.

### 4. Close the Tunnel

Press:

```text
Ctrl + C
```

---

## Run the Project

Run the application:

```bash
dotnet run
```

The API will run on the configured local port.

---

## Swagger

After running the project, open Swagger using the URL displayed in the terminal.

Example:

```text
http://localhost:5116/swagger
```

The port may be different on your machine.

Swagger can be used to:

- View available API endpoints
- Test API requests
- Check request and response models
- Explore the API documentation

---

## Git Workflow

The project uses the following branches:

- `main`: Main branch
- `development`: Main development branch
- Feature branches: Individual team member branches

### Update Your Local Development Branch

```bash
git fetch origin
git switch development
git pull origin development
```

### Create Your Own Feature Branch

Always create a new branch before starting a task:

```bash
git switch -c feature/your-task-name
```

Example:

```bash
git switch -c feature/user-profile
```

### Commit Your Changes

```bash
git add .
git commit -m "Implement user profile"
```

### Push Your Branch

```bash
git push -u origin feature/your-task-name
```

### Pull Request

Create a Pull Request using the following direction:

```text
Your Feature Branch → development
```

Do not push directly to the `development` or `main` branches unless you have permission.

---

## Development Guidelines

- Create a separate branch for every task.
- Use clear and meaningful commit messages.
- Keep controllers focused on handling HTTP requests.
- Put business logic inside services.
- Use DTOs for request and response models.
- Do not expose passwords or secret keys.
- Test API endpoints using Swagger or Postman.
- Keep database changes inside EF Core migrations.
- Create a Pull Request before merging your work into `development`.

---

## Deployment

The backend is prepared for deployment using:

- Docker
- Railway
- PostgreSQL

The production backend is hosted on Railway.

---

## Team

**BinX Tech — Team 3**

Project:

# رِواق | Riwaq

Educational knowledge and experience-sharing platform.