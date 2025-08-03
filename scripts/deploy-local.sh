#!/bin/bash

# Local deployment test script
# Use this to test deployments locally before pushing to GitHub

set -e

echo "=== Superelf Local Deployment Test ==="

# Check if docker-compose file is specified
if [ -z "$1" ]; then
    echo "Usage: $0 [prod|test]"
    echo "Example: $0 prod"
    exit 1
fi

ENVIRONMENT=$1

if [ "$ENVIRONMENT" = "prod" ]; then
    COMPOSE_FILE="docker-compose.prod.yml"
    ENV_FILE=".env.prod"
elif [ "$ENVIRONMENT" = "test" ]; then
    COMPOSE_FILE="docker-compose.test.yml"
    ENV_FILE=".env.test"
else
    echo "Invalid environment. Use 'prod' or 'test'"
    exit 1
fi

echo "Deploying $ENVIRONMENT environment using $COMPOSE_FILE"

# Check if environment file exists
if [ ! -f "$ENV_FILE" ]; then
    echo "Environment file $ENV_FILE not found!"
    echo "Create it with the following content:"
    if [ "$ENVIRONMENT" = "prod" ]; then
        echo "PROD_DB_PASSWORD=your_prod_password"
        echo "PROD_JWT_SECRET=your_prod_jwt_secret"
    else
        echo "TEST_DB_PASSWORD=your_test_password"
        echo "TEST_JWT_SECRET=your_test_jwt_secret"
    fi
    exit 1
fi

# Load environment variables
export $(cat $ENV_FILE | xargs)

echo "Stopping existing containers..."
docker-compose -f $COMPOSE_FILE down --remove-orphans || true

echo "Removing old images..."
docker image prune -f

echo "Building and starting new containers..."
docker-compose -f $COMPOSE_FILE up -d --build

echo "Waiting for services to start..."
sleep 30

echo "Container status:"
docker-compose -f $COMPOSE_FILE ps

if [ "$ENVIRONMENT" = "prod" ]; then
    TEST_PORT=5000
else
    TEST_PORT=5001
fi

echo "Testing API health..."
for i in {1..10}; do
    if curl -f http://localhost:$TEST_PORT/api/diagnostics/ping; then
        echo "API is healthy!"
        break
    fi
    echo "Attempt $i failed, retrying in 5 seconds..."
    sleep 5
done

echo "=== Deployment complete ==="
echo "Access the application at:"
if [ "$ENVIRONMENT" = "prod" ]; then
    echo "Frontend: http://localhost:3000"
    echo "API: http://localhost:5000"
else
    echo "Frontend: http://localhost:3001"
    echo "API: http://localhost:5001"
fi

echo ""
echo "View logs with:"
echo "docker-compose -f $COMPOSE_FILE logs -f"
