# ---- Stage 1: Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file and restore dependencies
COPY ["src/OmniTrxnGateway.Service/OmniTrxnGateway.Service.csproj", "src/OmniTrxnGateway.Service/"]
RUN dotnet restore "src/OmniTrxnGateway.Service/OmniTrxnGateway.Service.csproj"

# Copy the rest of the source code
COPY . .

# Publish the application
RUN dotnet publish "src/OmniTrxnGateway.Service/OmniTrxnGateway.Service.csproj" -c Release -o /app/publish

# ---- Stage 2: Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy the published output
COPY --from=build /app/publish .

# Expose the default port
EXPOSE 8080

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Run the application
ENTRYPOINT ["dotnet", "OmniTrxnGateway.Service.dll"]