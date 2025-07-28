# Makefile for Rinha Backend C# - Mac ARM Development + AMD64 Production

# Project configuration
PROJECT_NAME ?= rinha-backend-csharp
DOCKER_HUB_USER ?= rafaspimenta
IMAGE_TAG ?= $(DOCKER_HUB_USER)/$(PROJECT_NAME):prod_v1

.PHONY: dev prod prod-push clean

# ARM (builds and runs with ARM platform)
dev:
	DOCKER_BUILDKIT=0 DOCKER_DEFAULT_PLATFORM=linux/arm64 docker compose build
	docker compose up -d

# AMD64 (builds and runs with AMD64 platform)
prod:
	DOCKER_BUILDKIT=0 DOCKER_DEFAULT_PLATFORM=linux/amd64 docker compose build
	docker compose up -d

# Push production AMD64 image
prod-push: 
	docker build --platform linux/amd64 --tag $(IMAGE_TAG) .
	docker push $(IMAGE_TAG)

# Clean up - Stop and remove all containers and volumes
clean:
	docker compose down --volumes --remove-orphans