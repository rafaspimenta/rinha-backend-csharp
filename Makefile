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
	docker rmi -f $(IMAGE_NAME):latest || true

# Push to Docker Hub (optional)
push:
	docker tag $(IMAGE_NAME):latest your-dockerhub-user/$(IMAGE_NAME):latest
	docker push your-dockerhub-user/$(IMAGE_NAME):latest
