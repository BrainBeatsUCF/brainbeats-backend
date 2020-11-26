# Docker
1. To build the Docker image, open up a terminal in this directory and run the following:
`docker build -t brainbeats-backend .`
2. To run the image:
`docker run -d -p 5001:80 --name my-brainbeats-backend brainbeats-backend`

# Azure Container Registry
1. `docker tag [IMAGE ID] brainbeatscontainersregistry.azurecr.io/brainbeats-backend`
2. `docker push brainbeatscontainersregistry.azurecr.io/brainbeats-backend`