[![License](https://img.shields.io/github/license/denis-peshkov/Cross-Messaging)](LICENSE)
[![GitHub Release Date](https://img.shields.io/github/release-date/denis-peshkov/Cross-Messaging?label=released)](https://github.com/denis-peshkov/Cross-Messaging/releases)
[![NuGetVersion](https://img.shields.io/nuget/v/Cross.Messaging.svg)](https://nuget.org/packages/Cross.Messaging/)
[![NugetDownloads](https://img.shields.io/nuget/dt/Cross.Messaging.svg)](https://nuget.org/packages/Cross.Messaging/)
[![issues](https://img.shields.io/github/issues/denis-peshkov/Cross-Messaging)](https://github.com/denis-peshkov/Cross-Messaging/issues)
[![.NET PR](https://github.com/denis-peshkov/Cross-Messaging/actions/workflows/dotnet.yml/badge.svg?event=pull_request)](https://github.com/denis-peshkov/Cross-Messaging/actions/workflows/dotnet.yml)

![Size](https://img.shields.io/github/repo-size/denis-peshkov/Cross-Messaging)
[![GitHub contributors](https://img.shields.io/github/contributors/denis-peshkov/Cross-Messaging)](https://github.com/denis-peshkov/Cross-Messaging/contributors)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/denis-peshkov/Cross-Messaging/latest?label=new+commits)](https://github.com/denis-peshkov/Cross-Messaging/commits/master)
![Activity](https://img.shields.io/github/commit-activity/w/denis-peshkov/Cross-Messaging)
![Activity](https://img.shields.io/github/commit-activity/m/denis-peshkov/Cross-Messaging)
![Activity](https://img.shields.io/github/commit-activity/y/denis-peshkov/Cross-Messaging)

# Cross.Messaging

A set of libraries for sending notifications across multiple channels.

Current focus:
- Email (`Cross.Messaging.Email.*`)
- SMS (`Cross.Messaging.Sms.*`)
- Telegram (`Cross.Messaging.Telegram.*`)

**Supported targets:** .NET Standard 2.1, .NET 6, .NET 7, .NET 8, .NET 9, .NET 10

## Install NuGet package

Install the _Cross.Messaging_ [NuGet package](https://www.nuget.org/packages/Cross.Messaging/) into your .NET project:

```powershell
Install-Package Cross.Messaging
```

or

```bash
dotnet add package Cross.Messaging
```

## Quick start

### Register the email sender in DI

```csharp
services.AddEmailSender(configuration);
```

`AddEmailSender`:
- binds the `NotificationEmail` configuration section to `MessagingEmailOptions`
- registers `IEmailSenderService` -> `EmailSenderService` (scoped)

### Sample email configuration

```json
{
  "NotificationEmail": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "SecureSocket": "StartTls",
    "SmtpLogin": "login",
    "SmtpPassword": "password",
    "FromUserName": "App Notifications",
    "FromUserAddress": "no-reply@example.com",
    "RecipientOverride": ""
  }
}
```

## Unit tests

Tests use the **Given_When_Then** naming style:

- **Given** — context and preconditions.
- **When** — the action under test.
- **Then** — the expected outcome.

Example: `Given_ValidSmtpOptions_When_SendAsync_Then_UsesConfiguredHost`.

Tests live in `Cross.Messaging.Tests/` (for example `Email/`, `Sms/`).

## Code coverage

Run tests with code coverage (`opencover`):

```bash
dotnet test Cross.Messaging.Tests/Cross.Messaging.Tests.csproj \
 --collect:"XPlat Code Coverage" \
 --results-directory ./TestResults \
 -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```
