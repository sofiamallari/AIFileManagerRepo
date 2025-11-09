# ===============================
#  STAGE 1: Build the .NET API
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first
COPY AIFIleManager.API.sln ./
COPY AIFileManager.API/AIFileManager.API.csproj ./AIFileManager.API/
RUN dotnet restore "AIFileManager.API/AIFileManager.API.csproj"

# Copy everything and build
COPY . .
WORKDIR /src/AIFileManager.API
RUN dotnet publish -c Release -o /app/publish

# ================================
#  STAGE 2: Run the API
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "AIFileManager.API.dll"]
