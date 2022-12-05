# Plaid quickstart for .NET

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](code_of_conduct.md) 
[![Plaid API](https://img.shields.io/badge/Plaid%20API-v1.205.3-blue
)](https://github.com/plaid/plaid-openapi)
[![Build+Test](https://github.com/jcoliz/PlaidQuickstartBlazor/actions/workflows/buildtest.yml/badge.svg)](https://github.com/jcoliz/PlaidQuickstartBlazor/actions/workflows/buildtest.yml)

This repository is a port of the official [Plaid quickstart](https://github.com/plaid/quickstart) project, using the [Going.Plaid](https://github.com/viceroypenguin/Going.Plaid) client libraries for C# .NET. Here, the quickstart is implemented as a Hosted Blazor WebAssembly.

![Plaid quickstart for .NET](/docs/images/QuickstartDotNet.png)

## 1. Clone the repository

```Powershell
PS> git clone https://github.com/jcoliz/PlaidQuickstartBlazor
PS> cd PlaidQuickstartBlazor
```

## 2. Set up your secrets

```Powershell
PS> cd .\Server\
PS Server> cp .\secrets.example.yaml .\secrets.yaml
```

Copy `secrets.example.yaml` to a new file called `secrets.yaml`, then fill out the configuration variables inside. At
minimum `ClientID` and `Secret` must be filled out. Get your Client ID and secrets from
the [Plaid dashboard](https://dashboard.plaid.com/account/keys)

> NOTE: The `secrets.yaml` files is included as a convenient local development tool. In fact, you can use any of the many methods of getting configuration settings into ASP.NET. Please see [Safe storage of app secrets in development in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=linux).

## 3. Install .NET 6.0 SDK

If you don't already have the .NET 6.0 SDK installed, be sure to get a copy first from the [Download .NET](https://dotnet.microsoft.com/en-us/download) page.

## 4. Run it!

Build and run the app from the `.\Server` directory, which contains all necessary components.

```Powershell
PS> cd .\Server\
PS Server> dotnet run
```

The quickstart will now be listening on `https://localhost:7267`

## Test credentials

In Sandbox, you can log in to any supported institution (except Capital One) using `user_good` as the username and `pass_good` as the password. If prompted to enter a 2-factor authentication code, enter `1234`.

In Development or Production, use real-life credentials.

## Porting guide

For details on how to keep the port up-to-date, please see the [Porting Guide](/docs/PORTING.md).

## Code of conduct

We as members, contributors, and leaders pledge to make participation in our
community a harassment-free experience for everyone. We pledge to act and
interact in ways that contribute to an open, welcoming, diverse, inclusive, 
and healthy community.

Please review the [Code of conduct](/code_of_conduct.md) for more details.
