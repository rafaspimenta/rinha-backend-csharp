IMAGE_NAME=rinha-backend-csharp
TAG=$(shell git rev-parse --short HEAD 2>/dev/null || echo "latest")
FULL_IMAGE_NAME=rafaspimenta/$(IMAGE_NAME):$(TAG)

# Build the AOT app and Docker image
build:
	docker build --platform linux/amd64 -t $(FULL_IMAGE_NAME) .

# Remove image
clean:
	docker container prune -f
	docker image prune -f
	docker volume prune -f
	docker network prune -f

# Push to Docker Hub (optional)
push:
	docker login
	docker build --platform linux/amd64 -t $(FULL_IMAGE_NAME) .
	docker push $(FULL_IMAGE_NAME)

compose:
	docker-compose -f docker-compose.yml up
	
down:
	docker-compose -f docker-compose.yml down --remove-orphans