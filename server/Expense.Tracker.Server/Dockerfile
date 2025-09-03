# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Repository
COPY . .

# Restore Dependencies
RUN dotnet restore "Expense.Tracker.Server.sln"

# Build Application
RUN dotnet build "Expense.Tracker.Server.sln" -c Release --no-restore

# Run Unit Tests
RUN dotnet test "Expense.Tracker.Tests/Expense.Tracker.Tests.csproj" -c Release --no-build --verbosity normal

# Publish Application
RUN dotnet publish "Expense.Tracker.Application/Expense.Tracker.Application.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment
ENV ASPNETCORE_URLS=http://+:8080

# Start application
ENTRYPOINT ["dotnet", "Expense.Tracker.Application.dll"]
