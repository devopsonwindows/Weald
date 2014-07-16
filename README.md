Weald
=====

## Contents

1. [Introduction](README.md#introduction)
2. [Screenshots](README.md#screenshots)
3. [Implementation](README.md#implementation)
4. [Installation](README.md#installation)
  1. [Advanced Settings](README.md#advanced-settings)
6. [TODO](README.md#todo)

## Introduction

Weald is a dashboard and REST API for SVN repository details for repos hosted on [VisualSVN Server](http://www.visualsvn.com/server/). The REST API is documented in the "API" link off the dashboard itself.

## Screenshots

### Dashboard

![Weald Dashboard](/docs/dashboard.png?raw=true "Weald Dashboard")

### API Help page

![Weald API Help Page](/docs/api1.png?raw=true "Weald API Help Page")

### API Help page, continued

![Weald API Help Page, continued](/docs/api2.png?raw=true "Weald API Help Page, continued")

## Implementation

Weald is implemented using ASP.NET MVC 4 and the Razor view engine. The dashboard is "dog fooding" the REST API by calling it to populate the grid and charts it displays.

See the [OTHER_LICENSES](/Weald/OTHER_LICENSES) file for a listing of other tools and libraries used to create Weald.

## Installation

Currently Weald expects to be installed on the same host as your VisualSVN Server. This means you need IIS enabled/installed on your SVN server.

Assuming you already have IIS up and running, [download our latest release](https://github.com/devopsonwindows/Weald/releases/) and unzip the Weald directory contained in the release zip file into your C:\inetpub directory creating the path C:\inetpub\Weald.


Next, create an Application Pool in IIS named Weald using at least .NET 4.0 as the Framework version, with "Integrated" for the "Managed pipline mode" setting. The other caveat is that the Weald Application Pool will need to run as the Local System account or some user with:
* read permissions to your VisualSVN Server configuration file
* read permissions to your repo storage directory
* execute permissions to svnlook.exe (ships with VisualSVN Server)

Then create a new website named Weald in IIS and assign the Weald Application Pool to it.

### Advanced settings

While not terribly ideal at the moment, there are a couple of advanced settings you can tweak if you want.

#### Server Alias

In the Web.config file there is a setting "SvnServerAlias". If your SVN server is behind a CNAME (DNS Alias) you can specify it here and your repo URLs displayed in Weald will show this hostname vs. the hostname specified in the VisualSVN Server httpd.conf.

#### Logging

Weald logs to C:\ProgramData\Weald\Weald.log by default (it mostly just logs warnings and errors it encounters at the moment). You can adjust this log path in the Web.config's <log4net> section's RollingFileAppender's <file> element.

#### Update frequency

The dashboard is basically just polling the REST API periodically. This is currently controlled by the setInterval call in Weald/Scripts/weald.js. Out of the box it's 3 seconds (i.e. 3000 milliseconds).

#### svnlook frequency

The repository details are obtained by Weald automatically running svnlook on each repo in the background periodically. How frequently it runs svnlook is controlled by the "RepoInfoRefreshInterval" setting in the Web.config file. This setting defaults to 5 minutes out of the box and is specified in [.NET TimeSpan format](http://msdn.microsoft.com/en-us/library/ee372286).

## TODO

* General unhandled exception "handler"
* Dynamically discover CNAMEs
