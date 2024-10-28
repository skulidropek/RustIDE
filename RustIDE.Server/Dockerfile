# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y nodejs npm

COPY ["RustIDE.Server/RustIDE.Server.csproj", "RustIDE.Server/"]
RUN dotnet restore "./RustIDE.Server/RustIDE.Server.csproj"

COPY . .
WORKDIR "/src/RustIDE.Server"
RUN dotnet build "./RustIDE.Server.csproj" -c Release -o /app/build

# Stage 2: Publish stage
FROM build AS publish
RUN dotnet publish "./RustIDE.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RustIDE.Server.dll"]
