# DevTask Manager

MVP full-stack para gerenciamento de tasks: API REST em .NET 8 com autenticação JWT e frontend em React + Vite, com CI/CD via Azure Pipelines.

## Visão geral

O DevTask Manager permite que um usuário autenticado:

- Faça login (usuário `admin`, senha `123456`)
- Liste, crie, edite e exclua tasks
- Marque tasks como concluídas

Todas as rotas de tasks exigem um token JWT válido no header `Authorization: Bearer <token>`.

## Stack

| Camada     | Tecnologia                          |
| ---------- | ----------------------------------- |
| Backend    | .NET 8, ASP.NET Core Web API        |
| Banco      | SQLite + Entity Framework Core      |
| Auth       | JWT (HMAC-SHA256)                   |
| Frontend   | React 18, Vite 5, React Router 6, Axios |
| Testes     | xUnit                              |
| CI/CD      | Azure DevOps Pipelines              |

## Estrutura de pastas

```
.
├── backend/                # API .NET 8
│   ├── Controllers/        # AuthController, TasksController
│   ├── Data/               # AppDbContext (EF Core + SQLite)
│   ├── Dtos/               # Records de request/response
│   ├── Migrations/         # Migrations do EF Core
│   ├── Models/             # TaskItem
│   └── Services/           # TokenService (geração de JWT)
├── backend.Tests/          # Testes xUnit (integração via WebApplicationFactory)
├── frontend/               # React + Vite
│   └── src/
│       ├── pages/          # Login.jsx, Dashboard.jsx
│       └── services/       # api.js (Axios + interceptors)
├── azure-pipelines.yml     # Pipeline CI/CD
└── README.md
```

## Como rodar localmente

### Pré-requisitos

- .NET 8 SDK
- Node.js 20+
- npm

### Backend

```bash
cd backend
dotnet restore
dotnet run
```

A API sobe em `http://localhost:5246` (Swagger em `http://localhost:5246/swagger`).

O banco SQLite (`devtaskmanager.db`) é criado e migrado automaticamente na inicialização.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

O frontend sobe em `http://localhost:5173`.

> O Vite tem um proxy configurado para `/api` → `http://localhost:5246`, e o backend aceita CORS de `http://localhost:5173`.

### Testes

```bash
dotnet test DevTaskManager.sln
```

## Arquitetura

```
[React + Vite (5173)] ──HTTP──▶ [API .NET 8 (5246)]
                                      │
                              [EF Core + SQLite]
```

- O frontend usa Axios com um interceptor que injeta o token JWT salvo no `localStorage` em toda requisição.
- O backend valida o token via `JwtBearer` middleware; rotas de tasks têm `[Authorize]`.
- Swagger (apenas em desenvolvimento) está configurado com suporte a Bearer token.

## Fluxo de autenticação JWT (passo a passo)

1. O usuário preenche o formulário em `Login.jsx`.
2. O frontend faz `POST /api/auth/login` com `{ username, password }`.
3. O `AuthController` valida as credenciais (configuradas em `appsettings.json`: `admin` / `123456`).
4. Em caso de sucesso, o `TokenService` gera um JWT assinado com HMAC-SHA256 (claims: `sub`, `jti`, `name`), com validade de 2 horas.
5. O token é retornado no corpo da resposta e salvo no `localStorage`.
6. O usuário é redirecionado para `/dashboard` (rota protegida pelo `ProtectedRoute` do React Router).
7. O interceptor do Axios anexa `Authorization: Bearer <token>` em todas as chamadas.
8. O backend valida assinatura, emissor, audiência e expiração do token em cada requisição a `/api/tasks`.
9. Em caso de `401`, o interceptor limpa o token e redireciona para `/login`.
10. O logout remove o token do `localStorage` e volta para a tela de login.

## Fluxo de deploy (CI/CD) com Azure Pipelines

O arquivo `azure-pipelines.yml` define o pipeline:

1. **Trigger**: push na `main` e em Pull Requests.
2. **Pool**: agente Linux `ubuntu-latest`.
3. **Passos**:
   - Instala o .NET 8 SDK e o Node.js 20.
   - `dotnet restore` → restaura pacotes NuGet.
   - `dotnet build` → compila a solução em Release.
   - `dotnet test` → roda os testes xUnit e publica resultados.
   - `npm ci` + `npm run build` → builda o frontend.
   - `dotnet publish` → publica a API (zip).
   - Publica os artefatos (`api` + `frontend/dist`) no Azure DevOps.

Para usar:

1. Crie um repositório no Azure Repos (ou GitHub) e faça push deste código.
2. No Azure DevOps, crie um novo pipeline e aponte para `azure-pipelines.yml`.
3. O pipeline roda automaticamente a cada push/PR.

## Futuras melhorias

- Persistência de usuários reais (registro, hash de senha com BCrypt, refresh tokens)
- Banco de dados em produção (PostgreSQL/SQL Server)
- Paginação, filtros e ordenação no GET `/api/tasks`
- Testes do frontend (Vitest + React Testing Library)
- Deploy automático (Azure App Service, Docker/Kubernetes)
- Logs estruturados e monitoramento (Application Insights)
- HTTPS e variáveis de ambiente para segredos (Key Vault)
