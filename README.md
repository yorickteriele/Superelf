# Superelf

A fantasy football manager application built with .NET Core and React. Users can create their own fantasy teams, participate in pools, and earn points based on their selected players' performances.

## Requirements

- .NET 7.0 SDK
- Node.js 16.x
- SQL Server

## Installation

1. Clone and setup backend:
```bash
git clone https://github.com/yorickteriele/Superelf.git
cd Superelf
dotnet restore
cd Superelf.Infrastructure
dotnet ef database update
```

2. Setup frontend:
```bash
cd ../Superelf.Web
npm install
```

## Environments

### Development
```bash
# Start API
cd Superelf.API
dotnet run

# Start Frontend (new terminal)
cd Superelf.Web
npm run dev
```
Access at:
- API: https://localhost:7241
- Frontend: http://localhost:5173

### Test
```bash
docker-compose -f docker-compose.test.yml up -d
```
Access at http://test.superelf.nl

### Production
```bash
docker-compose -f docker-compose.prod.yml up -d
```
Access at http://superelf.nl

## Architecture

The application uses Clean Architecture with Domain-Driven Design:

![Backend Architecture](image/README/1760013244059.png)

### Core Layers
The domain and application layers form the core of the application, containing business rules and use cases.

![Infrastructure and API](image/README/1760013378458.png)

### Infrastructure & API
The outer layers handle data persistence, external integrations, and provide REST API endpoints. The React frontend communicates with these endpoints to deliver the user interface.
