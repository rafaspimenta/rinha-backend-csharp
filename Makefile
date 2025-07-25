# Makefile for Rinha Backend C# - Mac ARM Development + AMD64 Production

# Project configuration
PROJECT_NAME ?= rinha-backend-csharp

.PHONY: help build-local build-prod push-prod clean dev

# Default target
help:
	@echo "Available commands:"
	@echo "  build-local     - Build for local Mac ARM development"
	@echo "  build-prod      - Build for production AMD64 deployment"
	@echo "  push-prod       - Build and push production AMD64 image"
	@echo "  dev             - Build local ARM64 and start development environment"
	@echo "  clean           - Clean up containers and images"
	@echo ""
	@echo "Project: $(PROJECT_NAME)"

# Production build (AMD64 only - for deployment)
build-prod:
	docker build --platform linux/amd64 --tag $(PROJECT_NAME):prod .

# Push production AMD64 image
push-prod: build-prod
	docker push $(PROJECT_NAME):prod

# Development with local ARM64 build
dev:
	DOCKER_DEFAULT_PLATFORM=linux/arm64 docker compose build
	docker compose up -d

# Clean up - Stop and remove all containers and volumes
clean:
	docker compose down --volumes --remove-orphans