# Team 3 Backend

Backend API for the BinX Final Project.

## Tech Stack

* ASP.NET Core Web API
* .NET 10
* C#
* Entity Framework Core
* PostgreSQL
* Swagger

## Requirements

Install the following tools:

* .NET SDK 10
* PostgreSQL
* Git
* Visual Studio Code or Visual Studio

## Setup

Clone the repository:

```bash
git clone https://github.com/Mithgal-JH/Team3-Backend.git
cd Team3-Backend/Team3.Backend
```

Restore the project packages:

```bash
dotnet restore
```

## Database

Create a PostgreSQL database named:

```text
team3_backend_db
```

Configure your local PostgreSQL connection using User Secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=team3_backend_db;Username=postgres;Password=YOUR_PASSWORD"
```

Replace `YOUR_PASSWORD` with your PostgreSQL password.

## Run the Project

```bash
dotnet build
dotnet run
```

## Swagger

After running the project, open:

```text
http://localhost:5116/swagger
```

The port may be different on your machine.

## Branches

* `main`: Main branch
* `development`: Development branch
* Personal branches: Used for individual work

Create your own branch before starting work:

```bash
git checkout -b your-branch-name
```

Push your branch:

```bash
git push origin your-branch-name
```

Pull Requests should be created toward:

```text
development
```
