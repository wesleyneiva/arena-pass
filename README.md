# ArenaPass

Sistema de gestão de quadras e docência terceirizada para clubes. Professores terceirizados agendam horários, pagam a taxa ao clube e emitem convites (com QR Code) para alunos não-sócios.

Esta é a primeira fatia entregue: autenticação (JWT), cadastro de quadras/professores e agendamento com bloqueio de conflito. Financeiro completo, convites/QR Code e dashboards ricos vêm nas próximas rodadas.

## Stack

- **Backend**: .NET 10, Clean Architecture (Domain/Application/Infrastructure/Api), CQRS com MediatR, EF Core + Npgsql (Postgres via Supabase)
- **Frontend**: Angular (standalone) + Tailwind CSS

## Estrutura

```
backend/   API .NET (Clean Architecture)
frontend/  App Angular
```

## Rodando localmente

### Backend

1. Configure a connection string do Postgres (Supabase) via user-secrets:
   ```
   cd backend/src/ArenaPass.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<sua connection string>"
   ```
2. Rode a API (em `Development`, ela aplica as migrations e faz o seed inicial — modalidade "Beach Tennis", "Quadra 4" e um usuário admin):
   ```
   dotnet run --project backend/src/ArenaPass.Api
   ```
3. Swagger em `https://localhost:<porta>/swagger`.
4. Login inicial do clube: `admin@arenapass.local` / `Admin@123` (senha de dev — trocar antes de produção).

### Frontend

```
cd frontend
npm install
npm start
```

App em `http://localhost:4200`.

## Deploy (planejado)

- API no **Render**
- Frontend no **Netlify**
