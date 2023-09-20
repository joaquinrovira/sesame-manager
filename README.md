
<h1><p align="center">Sesame Manager</p></h1> 

<p align="center">
    <a href="https://www.sesametime.com/" target="blank"><img src="./.img/icon.png"  height="100" alt="Sesame Logo" /></a>
</p>
 
<p align="center">Manage your <a href="https://www.sesametime.com/" target="blank">Sesame</a> account, written in <a href="https://dotnet.microsoft.com" target="blank">C#</a>.</p>

## 🐋 Running with Docker

Read the sections below for configuration details. This repository automatically publishes the contents of the main branch to `joaquinrovira/sesame-manager:latest`and includes multi-architecture support for Linux hosts with `amd64`,`arm64` and `arm/v7` architectures. Once configured you can run the reservation agent as follows:

``` bash
docker run                                                \
  -e Email=YOUR_LOGIN_EMAIL                               \
  -e Password=YOUR_LOGIN_PASSWORD                         \
  -e TZ=YOUR_DESIRED_TIMEZONE                             \
  -v /path/to/local/appsettings.json:/appappsettings.json \
  joaquinrovira/sesame-manager
```

> *NOTE:* As this is time-sensitive software, **setting the appropriate timezone is vital**. [More info below](#🛠️-configuration).

## 🏗️ Running the application

Install `dotnet` following the [official instructions](https://learn.microsoft.com/dotnet/core/install/). Then, build the binary with the following commands:

```bash
# Clone the repo
git clone https://github.com/joaquinrovira/sesame-manager
cd sesame-manager

# Build the binary
dotnet build

# Run the application
dotnet run
``` 

## 🛠️ Configuration

The application can be configured like any other default .NET [console application](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration#alternative-hosting-approach).
Configuration options are read in the following order, from highest to lowest priority:

1. Command-line arguments using the [Command-line configuration provider](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#command-line-configuration-provider).
2. Environment variables using the [Environment Variables configuration provider](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#environment-variable-configuration-provider).
3. [App secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) when the app runs in the `Development` environment.
4. appsettings.`Environment`.json using the [JSON configuration provider](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#file-configuration-provider). For example, appsettings.Production.json and appsettings.Development.json.
5. appsettings.json using the [JSON configuration provider](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#file-configuration-provider).


### Basic configuration

| Variable             | Description                                                                                                                        |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `Email`              | (*required*) Sesame login email                                                                                                    |
| `Password`           | (*required*) Sesame login password                                                                                                 |
| `WeeklySchedule`     | (*required*) Set the weekly check-in/check-out schedule ([more info](#🗓️-weekly-schedule-configuration))                            |
| `TZ`                 | (*optional*) Set the application timezone ([more info](#🕒-timezone-configuration))                                                 |
| `AdditionalHolidays` | (*optional*) Configurable holidays to complement the default holiday providers ([more info](#🎄-additional-holidays-configuration)) |

#### 🕒 Timezone configuration

As this is time-sensitive software, **setting the appropriate timezone is vital**. This is done by configuring the `TZ` variable to a valid value — from the `TZ database name` column of [the following table](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones#List). The timezone is automatically set to the host's timezone. In linux, you can check out the local machine's timezone with `timedatectl show | grep "^Timezone=.*\$" | cut -d '=' -f2`. Alternatively, it can be set manually to a value like `Asia/Tokyo`.

Example: `TZ=Asia/Tokyo dotnet run`

#### 🗓️ Weekly schedule

The weekly schedule is used to tell SesameManager when to run check-in and check-out operations. It could be configured through environment variables but the default .NET parser makes it difficult. It is best to configure the weekly schedule through the `appsettings.json` file. The weekly schedule configuration must follow the schema below.

```json
{
  "WeeklySchedule": {
    "Monday": {
      "Start": { "Hour": 12, "Minute": 34 },
      "End":   { "Hour": 4 },
      "Site": "homeoffice"
    },
    "Tuesday": {
      "Start": { "Hour": 2 },
      "End":   { "Hour": 13, "Minute": 37}
    },
    "Wednesday": { },
    ...
  }
}
```

For each day of the week (`Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`, `Saturday`, and `Sunday`), it accepts an object with three properties: `Start`, `End`, and `Site`.
The first two are in and of themselves a time of day as describes by an `Hour` and a `Minute` value. The minute value is optional and default to zero. The site value optionally sets the location for the check-in and check-out operations. A `null` or undefined value will apply the default location for your account.  

#### 🎄 Additional holidays

Besides the holidays obtained from the holiday providers, the user can manually configure dates where check-in and check-out operation will not be triggered. It is best to configure  these through the `appsettings.json` file. Additional holidays configuration must respect the following schema.

```json
{
  "AdditionalHolidays": [
    { "Day": 1, "Month": 11 }
  ]
}
```

### Holiday provider configuration

There are two holiday providers currently implemented, [Nager](https://date.nager.at/) (Global) and [Ideal](https://calendarios.ideal.es/) (Spain only). They are publicly accessible. However, it is up to the user to configure them in order two provide data for their appropriate location.

#### Nager

| Variable                              | Description                                                                                                                        |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `HolidayProvider__Nager__CountryCode` | (*required*) (e.g., `ES`) The two letter country codes from Nager API - (a list can be found here)[https://date.nager.at/Country]  |
| `HolidayProvider__Nager__CountyCode`  | (*optional*) (e.g., `ES-AN`) The longer county code used to also retrieve the specific holidays for a given county                 |

#### Ideal

| Variable                              | Description                                                                                                                        |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `HolidayProvider__Ideal__CalendarPath` | (*required*) The path to the specific calendar regional calendar  |

> *NOTE: the `CalendarPath` is obtained from https://calendarios.ideal.es. Navigate to your desired yearly calendar and retrieve the path as follows. Given an example calendar url, https://calendarios.ideal.es/laboral/castilla-la-mancha/ciudad-real/tomelloso/2023, `CalendarPath` here is the url segments between `ideal.es/` and `/2023`, i.e., `laboral/castilla-la-mancha/ciudad-real/tomelloso`.*

## :information_source: Contributing

As there is no official public API, the application may break at any time. Pull requests are welcome.

## ⚠️ Disclaimer

The Sesame **does not authorize** the use of this program. This has been published for **educational purposes only**. 

