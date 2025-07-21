# Name of the final Docker image
IMAGE_NAME=rinha-backend-csharp
PORT=8080

# Build the AOT app and Docker image
build:
	docker build -t $(IMAGE_NAME):latest .

# Run the app in a local container
run:
	docker run --rm -it -p $(PORT):8080 --name $(IMAGE_NAME)-dev $(IMAGE_NAME):latest

# Rebuild and restart the container
rebuild: clean build run

# Remove image
clean:
	docker container prune -f
	docker image prune -f
	docker volume prune -f
	docker network prune -f

# Push to Docker Hub (optional)
push:
	docker tag $(IMAGE_NAME):latest rafaspimenta/$(IMAGE_NAME):latest
	docker push rafaspimenta/$(IMAGE_NAME):latest

compose:
	docker-compose -f docker-compose.yml up
	
down:
	docker-compose -f docker-compose.yml down --remove-orphans