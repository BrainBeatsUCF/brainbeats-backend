# Docker
1. To build the Docker image, open up a terminal in this directory and run the following:
docker build -t brainbeats-backend .
2. To run the image:
docker run -d -p 5001:80 --name my-brainbeats-backend brainbeats-backend -> Local port : Docker port
3. To push the image to the Docker Hub repository:
docker image -> Get the image ID
docker tag [IMAGE ID] [jcbang/brainbeats-backend]:1.0 -> Replace 1.0 with the version number, replace jcbang/brainbeats-backend with the repo name
docker push jcbang/brainbeats-backend -> docker push [REPO NAME]