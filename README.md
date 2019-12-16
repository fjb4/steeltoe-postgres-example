# Demonstration of Steeltoe's Service Connectors feature

This is demonstrates using the Steeltoe Service Connectors feature to connect to a PostgreSQL database.

This application was generated using the [start.steeltoe.io](https://start.steeltoe.io) web site, with only Postgres and Cloud Foundry dependencies selected. It uses [.NET Core 2.2](https://dotnet.microsoft.com/download/dotnet-core), [Steeltoe 2.4](https://steeltoe.io/), and [Dapper](https://www.nuget.org/packages/Dapper/).

### Deployment Steps for Pivotal Web Services
- Create service instance: `cf create-service elephantsql turtle my-postgres` (or replace elephantsql with the name of a Postgres service in your environment)
- Deploy application: `cf push postgres-example`
- Bind service to application: `cf bind-service postgres-example my-postgres`

### Test Database Connectivity
- Open `http://<your-application-url>/api/values` in your browser; if it works you should see a list of the tables in your Postgres database
