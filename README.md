# MemoryStress
.NET application designed to simulate a memory stress test by incrementally reserving memory until the container's memory is exhausted

## Usage

Create a docker image and upload to registry. executing the following command in the repository root:

```powershell
docker build -f .\MemoryStress\Dockerfile -t <registry>/<imagename>:latest .
dockedr push <registry>/<imagename>:latest
```

Then we can create a container job in Azure Container Apps to run the memory stress test. 
The following script creates a new resource group, a new log analytics workspace, a new container application environment, and a new container job. 
The job will run the memory stress test with a delay of 5 seconds between memory allocations and will request 100MB of memory to the container.

```powershell
az login
az extension add --name containerapp --upgrade
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights

$resourceGroup = 'rg-container-jobs'
$location = 'westeurope'
$logAnalytics = 'la-jobs-memory'
$caeName = 'cae-jobs-memory'
$jobName = "memory-job"
$delay = 5
$mb= 100

az monitor log-analytics workspace create -g $resourceGroup -n $logAnalytics

$customerId = az resource show --name $logAnalytics --resource-type Microsoft.OperationalInsights/workspaces --query "properties.customerId" -g $resourceGroup --output tsv

$primarySharedKey = az monitor log-analytics workspace get-shared-keys  --resource-group $resourceGroup --workspace-name $logAnalytics --query primarySharedKey -o tsv


az containerapp env create -n $caeName -g $resourceGroup `
  --logs-workspace-id $customerId `
--logs-workspace-key $primarySharedKey `
--location $location

az containerapp job create `
    --name $jobName --resource-group $resourceGroup  --environment $caeName `
    --trigger-type "Manual" `
    --replica-timeout 600 `
    --replica-retry-limit 1 `
    --replica-completion-count 1 `
    --parallelism 1 `
    --image '<registry>/<imagename>:latest' `
    --cpu '0.5' --memory '1Gi' 

az containerapp job update -n $jobName -g $resourceGroup --set-env-vars SECONDS_DELAY=$delay MB_TO_REQUEST=$mb
```
Once the job is created we can launch it from Azure Portal. The container will crash after few seconds due to memory exhaustion.