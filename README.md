[![Logo](static/TrueVote_Logo_Text_on_Black.png)](https://truevote.org)

[![Twitter](https://img.shields.io/twitter/follow/TrueVoteOrg?style=social)](https://twitter.com/TrueVoteOrg)
[![Keybase Chat](https://img.shields.io/badge/chat-on%20keybase-7793d8)](https://keybase.io/team/truevote)

[![TrueVote.Api](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-github.yml/badge.svg)](https://github.com/TrueVote/TrueVote.Api/actions/workflows/truevote-api-github.yml)
[![Coverage Status](https://coveralls.io/repos/github/TrueVote/TrueVote.Api/badge.svg?branch=master)](https://coveralls.io/github/TrueVote/TrueVote.Api?branch=master)

# TrueVote.Api

## 🌈 Overview

TrueVote.Api is the core backend for [TrueVote](https://truevote.org).

The main technology stack platform is [.NET Core](https://dotnet.microsoft.com/) 6.0 (preview).

## 🛠 Prerequisites

Install Visual Studio 2022 (preview) or later, or Visual Studio Code. Ensure that `$ dotnet --version` is at least 6.0.

## ⌨️ Install, Build, and Serve the Site

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

## ❤️ Contributing

We welcome all contributions. Please read our [contributing guidelines](CONTRIBUTING.md) before submitting a pull request.

## 📜 License

TrueVote.Api is licensed under the MIT license.

[![License](https://img.shields.io/github/license/TrueVote/TrueVote.Api)]((https://github.com/TrueVote/TrueVote.Api/master/LICENSE))

[truevote.org](https://truevote.org)