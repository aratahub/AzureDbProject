
# AzureDbProject

A simple ASP.NET 8 Web API containerized with Docker that demonstrates secure JWT authentication, clean separation of services, and support for multiple data sources including In-Memory DB, Azure SQL, and Cosmos DB.

This version uses an **in-memory database**, making it portable and ideal for demos or GitHub publishing without revealing secrets.

## 🚀 Features

- ASP.NET Core 8 Web API
- JWT Authentication
- Health Check Endpoint
- Swagger API documentation
- Switchable data storage:
  - In-Memory (Default)
  - Azure SQL via Azure Key Vault
  - Azure Cosmos DB (optional)
- Docker-ready

## 🧰 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

## 📦 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-username/AzureDbProject.git
cd AzureDbProject
```

### 2. Build and run using Docker

```bash
docker-compose up --build
```

### 3. Access the API

Open Swagger UI to explore the endpoints:

```
http://localhost:5001/swagger
```

## 🔐 Authentication

This API uses JWT-based authentication. You must first authenticate to receive a token, then use the `Bearer <token>` format in `Authorization` headers for subsequent calls.

The sample app comes with a hardcoded `Jwt` key and issuer in `appsettings.json` for demo purposes. In production, store secrets in **Azure Key Vault**.

## 🏗 Project Structure

```
AzureDbProject/
├── Core/                  # Service layer
├── Data/                  # DbContext and entity configs
├── Entities/              # Models (e.g., DbOrder)
├── WebApi/                # Controllers and API setup
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## 📊 Health Check

You can verify if the app is running via:

```
http://localhost:5001/health
```

## 🤖 Sample Endpoints

- `POST /api/DbOrder/create` – Add an order
- `GET /api/DbOrder/List` – List all orders
- `GET /api/DbOrder/{id}/GetById` – Get order by ID
- `DELETE /api/DbOrder/{id}/DeleteById` – Delete order by ID

## 🧪 Testing Tips

Use Postman or Swagger UI for quick testing. Remember to include the `Authorization: Bearer <token>` header when accessing secure endpoints.

---

### 📘 License

This project is provided for demonstration and educational purposes. You are free to fork and build on it.
