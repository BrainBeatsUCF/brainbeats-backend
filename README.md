# BrainBeats Backend
### Summary
This repository contains two projects:
1. The C# ASP.NET Core Web API Backend containing code for all basic CRUD functionality that powers BrainBeats.
2. The Python Azure Functions containing code for Song Recommendations and Storage Cleaning.

### Database & File Storage
This repository is dependant on a deployed Azure Cosmos DB account and Azure Storage account and all associated connection strings. If running this repository for the first time, please double check to see if your keys are configured correctly if you run into connection problems.

### Security & User Auth
This project uses a self-service registration flow for Active Directory, the documentation can be found here: `https://docs.microsoft.com/en-us/azure/active-directory/external-identities/self-service-sign-up-user-flow`.

To get to the self-service configuration page, navigate to the "User Flows" page under the Azure AD B2C portal. If deploying and hosting this application in a new envnrionment, you will need to click "+ New User Flow" to generate a new sign up / sign in user flow and replace the Auth.MetadataAddress and Auth.TokenEndpoint variables in the appsettings.json file. See more below.

### Configuration Keys
In order to run this application locally, please rename the included APP_SETTINGS_TEMPLATE.json to appsettings.json, and fill in *only* the missing placeholder variables. If you are deploying and hosting this application in a new environment, you will need to fill in *all* variables with new names, endpoints, and ports.

In order to run this application in a deployed environment, please navigate to your App Service portal's Configuration page and click "+ New application setting" to create new variables. The naming convention of the App Service application settings is as follows:

```
{
    "Logging": {
        "LogLevel": {
            ...
        }
    },
    "AllowedHosts": "*",
    "Database": {
        "EndpointUrl": "brain-beats-database.gremlin.cosmos.azure.com",
        ...
    },
    ...
}
```

The corresponding application setting name for Database.EndpointUrl is `Database__EndpointUrl` with double underscores representing .json nesting. In this example, the full application setting will be name `Database__EndpointUrl`, value `brain-beats-database.gremlin.cosmos.azure.com`.

#### Helpful Hints:
- Database.PrimaryKey can be found in your Azure Cosmos DB account's Keys page.
- Storage.ConnectionString can be found in your Azure Storage account's Access Keys page.
- Auth.TenantId is your Directory (tenant) ID unique to your Azure Active Directory domain.
- Auth.AppId is your Application (client) ID you get in the Azure App Registrations page (you will need to register a new application if deploying from a fresh slate).
- Auth.ClientSecret can be found in your Azure App Registration's Certificates & Secrets page corresponding to this registered application.
- Auth.B2cExtensionAppClientId is your Application (client) ID you get from the *auto-generated* b2c-extensions-app in the Azure App Registrations page. If you can't see this application, please make sure you have "All Applications" selected and not "Owned Applications" selected in the registrations list.

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
BrainBeats Azure Functions works completely independantly from the C# backend and only extends functionality, rather than replace. The Azure Functions' goal is to provide a platform for *automated* tasks. The following two tasks are run at 9:30 AM daily.
- Automated Song Recommenders
- Automated Storage Cleaners

### Song Recommendations
The BrainBeats Song Recommender is a custom K Nearest Neighbors ML algorithm. The math for calculating distances for KNN can be found in the BrainBeats design documentation. For debugging generating recommendations for single users, a manual API endpoint is exposed. For production purposes, only the automated task should be run.

### Storage Cleaner
The BrainBeats Storage Cleaner analyzes Beat attributes in the database and gets a list of referenced Sample Ids. Sample Ids not being used by any Beat or Sample has the corresponding audio file deleted from the Azure Storage account to prevent clutter. For debugging deleting Sample files, a manual API endpoint is exposed. For production purposes, only the automated task should be run.

### How to Run
The BrainBeats Azure Functions is built with Microsoft Visual Studio Code. Using the Visual Studio Text Editor (not IDE) is strongly suggested for all testing and development.

### Configuration Keys
In order to debug functions locally, please rename `local.settings_TEMPLATE.json` to `local.settings.json` in the brainbeats-functions folder, and replace the placeholder database YOUR_PRIMARY_KEY and storage YOUR_CONN_STRING with the required values. See `Helpful Hints` to learn more about formatting.

To run functions in a deployed environment, please see the `Configuration Keys` section above to populate the Function App Configuration page.

To debug functions locally, follow the documentation `https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash` to install the Azure Functions Core Tools.

To deploy functions to Azure, follow the deployment documentation / quickstart `https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp`.
