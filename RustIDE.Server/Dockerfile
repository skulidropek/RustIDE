# Stage 1: Base image with ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Устанавливаем переменную окружения для порта
ENV ASPNETCORE_URLS=http://*:8080

# Stage 2: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем файлы проекта и выполняем dotnet restore
# Обратите внимание на корректировку путей
COPY ["../rustide.client/nuget.config", "../rustide.client/"]
COPY ["RustIDE.Server.csproj", "."]
COPY ["../rustide.client/rustide.client.esproj", "../rustide.client/"]

RUN dotnet restore "./RustIDE.Server.csproj"

# Копируем все файлы проекта для сборки
COPY .. .
WORKDIR "/src/RustIDE.Server"
RUN dotnet build "./RustIDE.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RustIDE.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final image with runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RustIDE.Server.dll"]
