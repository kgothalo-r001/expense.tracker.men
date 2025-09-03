# Setup .NET for genClient
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet
WORKDIR /app

# Install NSwag CLI
RUN dotnet tool install --global NSwag.ConsoleCore
ENV PATH="$PATH:/root/.dotnet/tools"

# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

# Copy Workspace
COPY . .

# Install & Build Abstractions
RUN if [ -d "abstractions" ]; then \
    echo "Building abstractions..."; \
    cd abstractions && \
    npm install && \
    npm run build && \
    echo "Abstractions built successfully"; \
  fi

# Install Business # Build Business
RUN if [ -d "business" ]; then \
    echo "Building business..."; \
    cd business && \
    npm install && \
    npm run build && \
    echo "Business built successfully"; \
  fi 

# Install & Build Main Application
RUN if [ -d "expense.tracker.main" ]; then \
    echo "Building main application..."; \
    cd expense.tracker.main && \
    npm install && \
    npm run genClient && \
    npm run build -- --configuration production && \
    echo "Main application built successfully"; \
  fi

# Production stage
FROM nginx:alpine AS production

# Copy custom nginx config
COPY nginx.conf /etc/nginx/nginx.conf

# Copy built application
  # Copy built application
  COPY --from=build /app/expense.tracker.main/dist/expense.tracker.main/ /usr/share/nginx/html/

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]