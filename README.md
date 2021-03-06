# OData-CLI
A friendly way to bootstrap an odata service for when you want to test/evaluate the OData protocol.

The cli creates a fully functional project that you can deploy on azure to test your service and hence allows you to be able to get started much faster.


## Prerequisites
1. [Dotnet CLI and Dotnet 3.1 sdk](https://docs.microsoft.com/en-us/dotnet/core/sdk)
2. [Azure CLI ](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
3. Azure ARM powershell module `Install-Module -Name AzureRM.profile -RequiredVersion 5.8.2`


## How to run.

Clone the project and run the following command.

```cmd
dotnet run --project ODataCLI\ODataCLI.csproj --csdl "C:\Users\UserName\source\repos\projectname.csdl" --subscriptionId "huher7y58459hudhuheie5494" --appServiceName "MyOdataService"
```

Upon successful deployment the application gives out a url for the service id.

## Consuming the service on .Net or your Xamarin app.

1. On visual studio install the[ OData Connected service extension](https://marketplace.visualstudio.com/items?itemName=laylaliu.ODataConnectedService).
2. [Generate the client](https://devblogs.microsoft.com/odata/odata-connected-service-0-4-0-release/) code using the OData Connected service. 
3. Enjoy. 

For other languages and platforms kindly consult [Odata.org](https://odata.org)