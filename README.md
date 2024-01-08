
# das-forecasting-jobs

## Build Status

![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/Manage%20Funding/das-reservations-jobs?branchName=master)

## Requirements

DotNet 6.0 and any supported IDE for DEV running. Azure function SDK v4 is also required.

## About

The forecasting jobs repository provides Azure Functions to automate the triggering of the forecasting process.

## Local running

You are able to run in **LOCAL** mode, for this you need the following dependencies:

- Azure Storage - an entry should be created with a PartitionKey of **LOCAL** and a **RowKey** of `SFA.DAS.Forecasting.Jobs_1.0` and a **Data** property

Your configuration file for Data should look like the following:

```
{

	"AppName": "das-forecasting-jobs",
	"ForecastingJobs": {
		"LevyDeclarationPreLoadHttpFunctionXFunctionKey": "[LevyDeclarationPreLoadHttpFunctionXFunctionKey]",
		"PaymentPreLoadHttpFunctionBaseUrl": "[PaymentPreLoadHttpFunctionBaseUrl]",
		"PaymentPreLoadHttpFunctionXFunctionKey": "[PaymentPreLoadHttpFunctionXFunctionKey]",
		"LevyDeclarationPreLoadHttpFunctionBaseUrl": "[LevyDeclarationPreLoadHttpFunctionBaseUrl]"
	}
}
```
- it is also necessary to have the `SFA.DAS.Encoding` config for running the forecasting functions

You will also require a local settings file like the following:

```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
    "ASPNETCORE_ENVIRONMENT": "DEV",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AppInsights_InstrumentationKey": "",
    "ConfigNames": "SFA.DAS.Forecasting.Jobs,SFA.DAS.Encoding",
    "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true",
    "EnvironmentName": "LOCAL",
    "FUNCTIONS_EXTENSION_VERSION": "~4",
    "LoggingRedisConnectionString": "",
    "NServiceBusLicense": "[NServiceBusLicense]",
    "ServiceBusConnectionString": "[ServiceBusConnectionString]"
  }
}
```

## Functions

### SFA.DAS.Forecasting.Triggers
This function is responsible for consuming events from the Employer Apprenticeship Service. It handles the following nServiceBus events:

- AccountFundsExpiredEvent
- RefreshEmployerLevyDataCompletedEvent
- RefreshPaymentDataCompletedEvent

The function act as a simple receiver for the above NServicebus events and then calls the existing HTTP triggered Azure functions which exist in das-forecasting tools.

