# ECommerce API

[![CI](https://github.com/ericklazerotti/ECommerceApi/actions/workflows/ci.yml/badge.svg)](https://github.com/ericklazerotti/ECommerceApi/actions/workflows/ci.yml)

API REST em ASP.NET Core 8 para um sistema simplificado de e-commerce (produtos, categorias e pedidos), construída em arquitetura em camadas (Domain / Application / Infrastructure / Api) com autenticação JWT baseada em papéis.

## Stack

- ASP.NET Core 8 (Web API)
- Entity Framework Core 8 + PostgreSQL (Npgsql)
- ASP.NET Core Identity (usuários e papéis)
- JWT Bearer para autenticação
- FluentValidation para validação de entrada
- xUnit + FluentAssertions para testes (regras de domínio e de serviço)
- Swagger / OpenAPI

## Arquitetura

```
src/
  ECommerceApi.Domain          # Entidades e regras de negócio puras (sem dependências externas)
  ECommerceApi.Application     # Casos de uso, DTOs, validadores, interfaces de repositório
  ECommerceApi.Infrastructure  # EF Core, Identity, JWT, implementação de repositórios
  ECommerceApi.Api             # Controllers, middleware, composição (Program.cs)
tests/
  ECommerceApi.Tests           # Testes de domínio e de serviço (EF Core InMemory)
```

Regras de negócio centrais ficam no domínio (ex.: `Product.DecreaseStock` nunca permite vender além do estoque disponível, `Order` só transiciona de status em sequências válidas). Os serviços de aplicação orquestram essas regras; controllers apenas validam entrada e delegam.

## Como rodar localmente

### Pré-requisitos
- .NET 8 SDK
- PostgreSQL rodando localmente (ou acessível via connection string)

### 1. Configurar segredos locais

A connection string e a chave JWT **não ficam no `appsettings.json`** — são lidas via [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
cd src/ECommerceApi.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=ecommerce_portfolio;Username=ecommerce_api;Password=SUA_SENHA"
dotnet user-secrets set "Jwt:Key" "uma-chave-secreta-aleatoria-de-pelo-menos-32-caracteres"
```

Opcionalmente, para ter um usuário Admin já criado em desenvolvimento:

```bash
dotnet user-secrets set "DevSeedAdmin:Email" "admin@ecommerce.local"
dotnet user-secrets set "DevSeedAdmin:Password" "Admin@12345"
```

### 2. Aplicar as migrations

```bash
dotnet tool install --global dotnet-ef   # se ainda não tiver
dotnet ef database update --project src/ECommerceApi.Infrastructure --startup-project src/ECommerceApi.Api
```

### 3. Rodar a API

```bash
dotnet run --project src/ECommerceApi.Api
```

Swagger disponível em `https://localhost:7163/swagger` (ou a porta configurada em `launchSettings.json`).

### 4. Rodar os testes

```bash
dotnet test
```

## Rodando com Docker

Sobe a API e o PostgreSQL juntos, sem precisar instalar nada além de Docker. As migrations são aplicadas automaticamente no start do container.

```bash
cp .env.example .env
# edite o .env e defina POSTGRES_PASSWORD e JWT_KEY

docker compose up --build
```

A API fica disponível em `http://localhost:8080` (Swagger em `http://localhost:8080/swagger`). Por padrão sobe com um usuário Admin já criado (`DEV_ADMIN_EMAIL`/`DEV_ADMIN_PASSWORD` no `.env`).

## CI

Todo push/PR para `master` roda build + testes via GitHub Actions ([`.github/workflows/ci.yml`](.github/workflows/ci.yml)).

## Segurança

- Senhas exigem mínimo de 8 caracteres, maiúscula, minúscula, número e caractere especial (validado tanto no FluentValidation quanto nas regras do Identity).
- `POST /api/Auth/login` e `/register` têm rate limiting (10 requisições/minuto por IP) contra brute-force e credential stuffing; a conta também é bloqueada temporariamente após tentativas de login inválidas seguidas (lockout padrão do Identity).
- Autorização por papel é sempre verificada no servidor via `[Authorize(Roles = ...)]`, nunca só escondida no cliente.
- Nenhum segredo (connection string, chave JWT, senha de admin de desenvolvimento) fica hardcoded no repositório — tudo vem de `user-secrets` local ou variáveis de ambiente/`.env` no Docker, e sem fallback fraco caso não sejam definidos.
- Mensagens de erro de login não revelam se o e-mail existe na base.

Gaps conhecidos, deixados fora de escopo por ser um projeto de portfólio: sem refresh token/revogação (o JWT expira em 60min e não pode ser invalidado antes disso) e sem HSTS/security headers explícitos.

## Fluxo básico da API

1. `POST /api/Auth/register` — cria um usuário com papel `Customer`.
2. `POST /api/Auth/login` — retorna um JWT.
3. Endpoints de escrita em `/api/Categories` e `/api/Products` exigem papel `Admin`.
4. `POST /api/Orders` — cliente autenticado cria um pedido; a API valida estoque disponível e decrementa automaticamente.
5. `POST /api/Orders/{id}/pay`, `/ship` — transições de status restritas a `Admin`.
6. `POST /api/Orders/{id}/cancel` — cliente dono do pedido ou Admin; devolve o estoque reservado.
