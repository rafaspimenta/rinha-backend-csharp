# Makefile for Rinha Backend C# - Mac ARM Development + AMD64 Production

# Project configuration
PROJECT_NAME ?= rinha-backend-csharp
DOCKER_HUB_USER ?= rafaspimenta
IMAGE_TAG ?= $(DOCKER_HUB_USER)/$(PROJECT_NAME):prod_v1

.PHONY: help build-local build-prod push-prod clean dev

# Default target
help:
	@echo "Available commands:"
	@echo "  build-local     - Build for local Mac ARM development"
	@echo "  build-prod      - Build for production AMD64 deployment"
	@echo "  push-prod       - Build and push production AMD64 image"
	@echo "  dev             - Build local ARM64 and start development environment"
	@echo "  prod            - Start production environment (Docker Hub images)"
	@echo "  clean           - Clean up containers and images"
	@echo ""
	@echo "Project: $(PROJECT_NAME)"

# Development (ARM only - for local development)
dev:
	DOCKER_DEFAULT_PLATFORM=linux/arm64 docker compose build
	docker compose up -d

# Production deployment (builds and runs with AMD64 platform)
prod:
	DOCKER_DEFAULT_PLATFORM=linux/amd64 docker compose build
	docker compose up -d

# Push production AMD64 image
prod-push: docker build --platform linux/amd64 --tag $(IMAGE_TAG) .
	docker push $(IMAGE_TAG)

# Clean up - Stop and remove all containers and volumes
clean:
	docker compose down --volumes --remove-orphans