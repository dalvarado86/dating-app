# dating-app
Based on of Neils Cummings's course on Udemy [Build an app with ASPNET Core and Angular from scratch](https://www.udemy.com/course/build-an-app-with-aspnet-core-and-angular-from-scratch/).

## Features

## Prerequisites
* [.NET Core SDK](https://dotnet.microsoft.com/download) (7.0.2 or later)
* [Node.js](https://nodejs.org/en/download/current/) (18.15.0 or later)
* [Angular](https://angular.io/guide/setup-local) (14.2.0 or later)
* [Cloudinary](https://cloudinary.com/) (Open a free account)

## Technologies

### ASP .NET CORE 7
* Entity Framework Core
* Automapper
* ASP .NET Core Identity
* Cloudinary API
* SignalR

### Angular 14
* Signalr

## Getting Started

### Database Configuration

The solution is configured to use Sqlite by default. Feel free to change to another database service as SQL Server or In-Memory Database if you want.

### Database Migration

Add a new migration from the api folder:

 `dotnet ef migrations add "MyMigration"`

 Then, you need run the api project to populate the data from the `/API/Data/UserSeedData.json`
 
 `dotnet run`

 **Important!** It needs to remove all migrations when changing a database service.

## Overview

### API

This layer is a Web API based on ASP.NET Core 7. Contains fundamental examples to start codig with C# and .NET Core 7.

### Cloudinary
* Open a free account and save your credentials in `/API/appsettings.json`

`"CloudinarySettings": {
    "CloudName": "YourCloudinaryName in cloudinary.com",
    "ApiKey": "YourApiKey",
    "ApiSecret": "YourApiSecret"
  }`

### Clien-App

This is a client app based on Angular with typescript, it represent the front-end of this application. A basic hands-on with Angular.

Have fun!
