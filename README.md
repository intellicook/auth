# Auth

The authentication service for IntelliCook.

## Design

The Auth service contains authentication and authorization information of the users.

### User

- **Name**: The name of the user for display.

- **Username**: The username of the user for unique identification.

- **Email**: The email of the user for communication.

- **Password**: The password of the user for authentication.

- **Role**: The role of the user for authorization, which is one of the following:

    1. **None**: A usual user with normal access to the service.
  
    2. **Admin**: An admin user with access to all the features of the service including administrative controls.
  
    3. **Dev**: A developer user with access to all the features of the service and the ability to manage all the users.

### Authentication

- The authentication is done with JWT tokens.

- The login endpoint is at `/Auth/Login`.

- The access token will not expire.

## Development Setup

The service uses [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet) on C# 12 and .NET 8.0, and [Entity Framework Core 8](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore). Since I am using JetBrains Rider, I will be able to provide more instructions for setting up the project in Rider.

1. Clone the repository.

2. To open the projects, open the [Auth.sln](Auth.sln) solution file found in the root of the repository.

3. Run the following commands to install the [Husky.Net](https://alirezanet.github.io/Husky.Net/) pre-commit hooks, which can help check if your formatting will pass the GitHub workflows before you commit.

   ```bash
   # Restore the tools defined in .config/dotnet-tools.json
   dotnet tool restore
   
   # Install the Husky.Net git hooks
   dotnet husky install
   ```

4. You have 2 options for running the database:

    - Use [Microsoft SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb), which will be closer to the production environment. You will need to run the migrations to create the database schema.

    - Use In-Memory Database, which is easier to set up and use for development. To enable it, the `Database__UseInMemory` environment variable or `Database:UseInMemory` variable in [IntelliCook.Auth.Host/appsettings.json](IntelliCook.Auth.Host/appsettings.json) have to be set to `true`, which should already be done for you in the "Http/Https In Memory Database" configurations in the [IntelliCook.Auth.Host/Properties/launchSettings.json](IntelliCook.Auth.Host/Properties/launchSettings.json) file.

## Making Code Changes

Important things to note when making code changes:

- All code changes made to the main branch must be done from a pull request, the branch name should use `kebab-case`.

- The GitHub workflows on the pull request must be passed before being able to merge into main.

- The formatting are defined in [.editorconfig](./.editorconfig).

- Always use the `dotnet-ef` tool to create migrations and update the database schema, so that the migration files are consistent.

## Tests

The service uses [xUnit](https://xunit.net/) for unit tests and [Moq](https://github.com/devlooped/moq) for mocking. The followings are the test projects:

- [IntelliCook.Auth.Host.UnitTests](IntelliCook.Auth.Host.UnitTests): Unit tests for the API.

- [IntelliCook.Auth.Host.E2ETests](IntelliCook.Auth.Host.E2ETests): End-to-end tests for the API, which runs the in-memory database to test the API from an HTTP client.
