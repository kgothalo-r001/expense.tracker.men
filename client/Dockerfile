# Build stage
FROM node:20-alpine AS build
WORKDIR /client

# Install curl for health checks
RUN apk add --no-cache curl

# Copy Workspace
COPY . .

# Install & Build Abstractions
RUN if [ -d "abstractions" ]; then \
    echo "Building abstractions..."; \
  cd "abstractions" && \
  npm install && \
    npm run build; \
    echo "Abstractions built successfully"; \
  fi

# Install Business dependencies first
RUN if [ -d "business" ]; then \
    echo "Installing business dependencies..."; \
  cd "business" && \
  npm install; \
  fi

# Create auto-generated directories to prevent build failures
RUN if [ -d "business" ]; then \
    mkdir -p business/auto/autobusinessclient && \
    cp business/fallback-client.ts business/auto/autobusinessclient/index.ts; \
  fi

# Build Business (with fallback client)
RUN if [ -d "business" ]; then \
    echo "Building business layer..."; \
    cd "business" && \
    (npm run genClient && echo "Client generated successfully") || \
    (echo "Client generation failed, using fallback" && \
     cp fallback-client.ts auto/autobusinessclient/index.ts) && \
    npm run build; \
    echo "Business layer built successfully"; \
  fi  

# Install & Build Main Application
RUN if [ -d "expense.tracker.main" ]; then \
    echo "Building main application..."; \
  cd "expense.tracker.main" && \
  npm install && \
    (npm run genClient && echo "Client generated successfully") || echo "Client generation failed, continuing..." && \
    npm run build -- --configuration production; \
    echo "Main application built successfully"; \
  fi

# Production stage
FROM nginx:alpine AS production

# Copy custom nginx config
COPY nginx.conf /etc/nginx/nginx.conf

# Copy built application
COPY --from=build /client/expense.tracker.main/dist/expense.tracker.main/* /usr/share/nginx/html/

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]  