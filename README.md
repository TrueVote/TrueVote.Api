[![Logo](static/TrueVote_Logo_Text_on_Black.png)](https://truevote.org)

[![Twitter](https://img.shields.io/twitter/follow/TrueVoteOrg?style=social)](https://twitter.com/TrueVoteOrg)
[![Keybase Chat](https://img.shields.io/badge/chat-on%20keybase-7793d8)](https://keybase.io/team/truevote)

[![TrueVote.Api](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-github.yml/badge.svg)](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-github.yml)
[![TrueVote.Api.Preprod.Integration](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-preprod-integration.yml/badge.svg)](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-preprod-integration.yml)
[![Coverage Status](https://coveralls.io/repos/github/TrueVote/TrueVote.Api/badge.svg?branch=master)](https://coveralls.io/github/TrueVote/TrueVote.Api?branch=master)

# TrueVote.Api

## 🌈 Overview

TrueVote.Api is the core backend for [TrueVote](https://truevote.org).

The main technology stack platform is [.NET Core](https://dotnet.microsoft.com/) 6.0 (preview).

## 🛠 Prerequisites

* Install Visual Studio 2022 (preview) or later, or Visual Studio Code. Ensure that `$ dotnet --version` is at least 6.0.
* Install Azure [CosmosDB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/sql-api-emulator)

## ⌨️ Install, Build, and Serve the Site

Create a new file at the root of TrueVote.Api named `local.settings.json` and add the following, replacing the account key with the actual account key from the [CosmosDB Emulator start page](https://localhost:8081/_explorer/index.html).

```json
{
  "IsEncrypted": false,
  "Values": {
    "CosmosDbConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=<AccountKeyFromCosmosDBEmulator>"
  }
}
```

```bash
$ dotnet restore
$ dotnet tool restore
```
Open the TrueVote.Api.sln solution in Visual Studio, and build the solution.

You'll see output in the console showing the various local URL access points.

![](static/console-output.png)

You can then access the Api root [`http://localhost:7071/api/swagger/ui`](http://localhost:7071/api/swagger/ui).

## 🧪 Unit Testing

Unit testing and code coverage are setup and **must** be maintained. To run the tests and generate a coverage report, run the Powershell script from the command line.

```bash
$ powershell ./scripts/RunTests.ps1
```

This generages a coverage report in `TrueVote.Api.Tests/coverage-html`. Open `index.html` to view the report.

## 🎁 Versioning

TrueVote.Api uses [sementic versioning](https://semver.org/), starting with 1.0.0.

The patch (last segment of the 3 segments) is auto-incremented via a GitHub action when a pull request is merged to master. The GitHub action is configured in the [.github/workflows/developer-guide.yml](.github/workflows/truevote-api-version.yml) file. To update the major or minor version, follow the instructions specified in the [bumping section of the action](https://github.com/anothrNick/github-tag-action#bumping) - use #major or #minor in the commit message to auto-increment the version.

## ❤️ Contributing

We welcome all contributions. Please read our [contributing guidelines](CONTRIBUTING.md) before submitting a pull request.

## 📜 License

TrueVote.Api is licensed under the MIT license.

[![License](https://img.shields.io/github/license/TrueVote/TrueVote.Api)]((https://github.com/TrueVote/TrueVote.Api/master/LICENSE))

[truevote.org](https://truevote.org)
<!---
Icons used from: https://emojipedia.org/
--->