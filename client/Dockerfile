# Build stage
FROM node:20-alpine AS build
WORKDIR /client

# Copy Workspace
COPY . .

# Install & Build Abstractions
RUN if [ -d "abstractions" ]; then \
    cd "abstractions" && \
    npm install && \
    npm run build; \
  fi

# Install & Build Business
RUN if [ -d "business" ]; then \
    cd "business" && \
    npm install && \
    npm run build; \
  fi  

# Install & Build Main Application
RUN if [ -d "expense.tracker.main" ]; then \
    cd "expense.tracker.main" && \
    npm install && \
    npm run build -- --configuration production; \
  fi

# Production stage
FROM nginx:alpine AS production

# Copy custom nginx config
COPY nginx.conf /etc/nginx/nginx.conf

# Copy built application
COPY --from=build /client/expense.tracker.main/dist/expense.tracker.main/ /usr/share/nginx/html/

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]  