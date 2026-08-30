FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ECommerceApi.sln ./
COPY src/ECommerceApi.Domain/ECommerceApi.Domain.csproj src/ECommerceApi.Domain/
COPY src/ECommerceApi.Application/ECommerceApi.Application.csproj src/ECommerceApi.Application/
COPY src/ECommerceApi.Infrastructure/ECommerceApi.Infrastructure.csproj src/ECommerceApi.Infrastructure/
COPY src/ECommerceApi.Api/ECommerceApi.Api.csproj src/ECommerceApi.Api/
COPY tests/ECommerceApi.Tests/ECommerceApi.Tests.csproj tests/ECommerceApi.Tests/
RUN dotnet restore ECommerceApi.sln

COPY . .
RUN dotnet publish src/ECommerceApi.Api/ECommerceApi.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerceApi.Api.dll"]
