[![Logo](static/TrueVote_Logo_Text_on_Black.png)](https://truevote.org)

[![Twitter](https://img.shields.io/twitter/follow/TrueVoteOrg?style=social)](https://twitter.com/TrueVoteOrg)
[![Keybase Chat](https://img.shields.io/badge/chat-on%20keybase-7793d8)](https://keybase.io/team/truevote)

[![TrueVote.Api](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-github.yml/badge.svg)](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-github.yml)
[![TrueVote.Api.Preprod.Integration](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-preprod-integration.yml/badge.svg)](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-preprod-integration.yml)
[![Coverage Status](https://coveralls.io/repos/github/TrueVote/TrueVote.Api/badge.svg)](https://coveralls.io/github/TrueVote/TrueVote.Api)

# TrueVote.Api

## 🌈 Overview

TrueVote.Api is the core backend for [TrueVote](https://truevote.org).

The main technology stack platform is [.NET Core](https://dotnet.microsoft.com/) 8.0.

## 🛠 Prerequisites

* Install Visual Studio 2022 (preview) or later, or Visual Studio Code. Ensure that `$ dotnet --version` is at least 8.0.
* Install Azure [CosmosDB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator-release-notes)

## ⌨️ Install, Build, and Serve the Site

Create a new file at the root of the TrueVote.Api project named `local.settings.json` and add the following, replacing the account key with the actual account key from the [CosmosDB Emulator start page](https://localhost:8081/_explorer/index.html).

Get the `ServiceBusConnectionString` from Azure portal. Currently Service Bus is not available to run locally.

Create a JWTSecret: `$ openssl rand -base64 32`

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDbConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=<AccountKeyFromCosmosDBEmulator>",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "ServiceBusConnectionString": "<ServiceBusConnectionString>",
    "ServiceBusApiEventQueueName": "apieventqueue-dev",
    "JWTSecret": <JWTBase64Key>
  }
}
```

### Install the packages

```bash
$ dotnet restore
$ dotnet tool restore
```
Open TrueVote.Api.sln solution in Visual Studio, and build the solution.

You'll see output in the console showing the various local URL access points.

![](static/console-output.png)

REST Api root [`https://localhost:7071/api/swagger/ui`](https://localhost:7071/api/swagger/ui)

GraphQL root [`https://localhost:7071/api/graphql`](https://localhost:7071/api/graphql)

## 🧪 Unit Testing

Unit testing and code coverage are setup and **must** be maintained. To run the tests and generate a coverage report, run the Powershell script from the command line.

```bash
$ powershell ./scripts/RunTests.ps1
```

This generates a coverage report in `TrueVote.Api.Tests/coverage-html`. Open `index.html` to view the report.

<a name="proxying-truevoteapi-locally"></a>
## 🎛️ Proxying TrueVote.Api Locally

In order to use TrueVote.Api locally with the [React Frontend](https://github.com/TrueVote/TrueVote.App), you must proxy it to simulate production and bypass [CORS](https://en.wikipedia.org/wiki/Cross-origin_resource_sharing) issues.

[Stunnel](https://www.stunnel.org/) works well. Simply install and open the `stunnel.conf` file and add this section to the bottom.

```
[TrueVote.Api]
client = yes
accept = localhost:8080
connect = localhost:7071
```

This will enable traffic to port :8080 as a proxy from the default port of TrueVote.Api (typically :7071). The React frontend expects TrueVote.Api to be listening on :8080.

## 📮 Making requests via Postman

[Postman](https://www.postman.com/) is a useful tool for testing Apis. TrueVote has a [hosted workspace](https://www.postman.com/truevote/workspace/truevote-api) containing a collection of useful example endpoints and their usage.

## 🎁 Versioning

TrueVote.Api uses [sementic versioning](https://semver.org/), starting with 1.0.0.

The patch (last segment of the 3 segments) is auto-incremented via a GitHub action when a pull request is merged to master. The GitHub action is configured in [.github/workflows/truevote-api-version.yml](.github/workflows/truevote-api-version.yml). To update the major or minor version, follow the instructions specified in the [bumping section of the action](https://github.com/anothrNick/github-tag-action#bumping) - use #major or #minor in the commit message to auto-increment the version.

## ❤️ Contributing

We welcome useful contributions. Please read our [contributing guidelines](CONTRIBUTING.md) before submitting a pull request.

## 📜 License

TrueVote.Api is licensed under the MIT license.

[![License](https://img.shields.io/github/license/TrueVote/TrueVote.Api)]((https://github.com/TrueVote/TrueVote.Api/master/LICENSE))

[truevote.org](https://truevote.org)
<!---
Icons used from: https://emojipedia.org/
--->