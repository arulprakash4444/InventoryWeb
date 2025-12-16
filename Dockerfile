# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution
COPY Inventory.sln .

# Copy project files
COPY InventoryWeb/InventoryWeb.csproj InventoryWeb/
COPY Inventory.DataAccess/Inventory.DataAccess.csproj Inventory.DataAccess/
COPY Inventory.Models/Inventory.Models.csproj Inventory.Models/
COPY Inventory.Utility/Inventory.Utility.csproj Inventory.Utility/

# Restore dependencies
RUN dotnet restore Inventory.sln

# Copy everything else
COPY . .

# Publish ONLY the web project
RUN dotnet publish InventoryWeb/InventoryWeb.csproj \
    -c Release \
    -o /app/publish \
    /p:TreatWarningsAsErrors=false

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InventoryWeb.dll"]
