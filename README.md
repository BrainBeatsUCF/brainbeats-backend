# BrainBeats Backend
### Summary
This repository contains two projects:
1. The C# ASP.NET Core Web API Backend containing code for all basic CRUD functionality that powers BrainBeats.
2. The Python Azure Functions containing code for Song Recommendations and Storage Cleaning.

## BrainBeats ASP.NET Core Web API Backend
The BrainBeats backend is built with Microsoft Visual Studio. Using the Visual Studio IDE is strongly suggested for all testing and development.

### How to Run
1. Open the Visual Studio Solution file `brainbeats-backend.sln` in the `brainbeats-backend` folder.
2. In the `Build` tab in the top navigational bar, click `Build Solution`.
3. In the `Debug` tab in the top navigational bar, click `Start Debugging`.
4. Access the APIs through the root URL `https://localhost:5001`.

## How to Deploy
Deployment is handled by creating a Docker image containing the Web API Backend application and deploying the image to Azure as an Azure Container. The deployed image is registered in the Azure Container Registry. The registered image is served as an Azure (Web) App Service.

## Docker
1. To build the Docker image, open up a terminal in the brainbeats-backend directory (the folder containing the Dockerfile) and run the following:
`docker build -t brainbeats-backend .`.
2. To run the image:
`docker run -d -p 5001:80 --name my-brainbeats-backend brainbeats-backend`.

## Azure Container Registry
To push the Docker image to the Azure Container Registry, run the following after building the Docker Image:
1. Ensure you are logged in to the ACR account by running `docker login brainbeatscontainersregistry.azurecr.io`. The login username and password can be found in the `brainbeatscontainerregistry` ACR portal in the "Access Keys" page.
2. `docker tag [IMAGE ID] brainbeatscontainersregistry.azurecr.io/brainbeats-backend`.
3. `docker push brainbeatscontainersregistry.azurecr.io/brainbeats-backend`.

## Web App Deployment
Deploy the registered Azure Container as an Azure (Web) App Service. The Microsoft documentation for this can be found in the link: `https://docs.microsoft.com/en-us/learn/modules/deploy-run-container-app-service/`.

If you are redeploying an updated image to an existing App Service, please hit the "restart" button on the App Service portal. Sometimes Azure App Services can be buggy and it won't detect an updated image during redeployment even if continuous deployment is toggled.

## BrainBeats Azure Functions
### Song Recommendations
### Storage Cleaner
### How to Run