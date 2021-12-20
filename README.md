# Azure Redis Cache Maintenance Listener

This is a sample application that listens for maintenance events emitted by the Azure Redis Cache service. It is written in .NET 6 and hosted in a Docker container for ease of deployment. Redis maintenance events are written as Warning messages in the applicaiton log.

## Run and Debug Locally

> To run and debug this application locally, it is assumed you have the .NET 6 SDK installed and are using Visual Studio Code. It is also assumed you have a working instance of Azure Redis Cache to test against.

At the root of the project, create a new file called `appsettings.Development.json` and update it as follows:

```json
{
    "RedisConnection": "YOUR_REDIS_INSTANCE_CONNECTION_STRING"
}
```

Next, click on the Run and Debug option in VS Code and luanch the **.NET Core Launch (console)** task. To test receipt of a maintenance event, use the Azure Portal to reboot either a primary or replica node (Administration --> Reboot). Shortly after this, you see warning messages appear in the debug console.

## Build and Release with Docker

```sh
docker build -t [docker_id]/rediseventlistener:0.0.1 .
docker run -d -e 'RedisConnection=[YOUR_REDIS_INSTANCE_CONNECTION_STRING]' [docker_id]/redismaintenancelistener:0.0.1
```

You can confirm the code is working within the contianer by examining the logs by using `docker logs -f [container_id]`. When confimred, use `docker push` to publish the container image to the registry of your choice.

## Deploy to Kubernetes

Switch to the `deployment` folder and create a new file called `secrets.yaml`. Update it to match your environment.

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: redis-eventlistener-secrets
stringData:
  RedisConnection: "[YOUR_REDIS_INSTANCE_CONNECTION_STRING]"
```

Customize the `service.yaml` file as necessary to match your environment. If you're deploying your own version of the service, you will need to update line 20. Run the following to deploy the sample to the default namespace.

```sh
kubectl apply -f secrets.yaml
kubectl apply -f service.yaml
```

## Alerting

If your AKS enviroment uses Container Insights, you can create an Alert Rule from the connected Log Analytics workspace to trigger an alert.

```Kusto
ContainerLog |
where LogEntry contains "NodeMaintenanceStarting"
```

Be aware that the shortest time slice an Alert Rule can evaluate logs in Log Analytics is every 5 minutes. If an immediate alert is required, the application will likely need to be modified to push an alert via some other mechanism.
