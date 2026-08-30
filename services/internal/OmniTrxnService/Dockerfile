# ---- Stage 1: Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file and restore dependencies
COPY ["src/OmniTrxnService.Api/OmniTrxnService.Api.csproj", "src/OmniTrxnService.Api/"]
RUN dotnet restore "src/OmniTrxnService.Api/OmniTrxnService.Api.csproj"

# Copy the rest of the source code
COPY . .

# Publish the application
RUN dotnet publish "src/OmniTrxnService.Api/OmniTrxnService.Api.csproj" -c Release -o /app/publish

# ---- Stage 2: Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy the published output
COPY --from=build /app/publish .

# Expose the default port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "OmniTrxnService.Api.dll"]