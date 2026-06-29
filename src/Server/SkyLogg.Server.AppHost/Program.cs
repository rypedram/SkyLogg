using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Check out appsettings.Development.json for credentials/passwords settings.


var sqlite = builder.AddSqlite();


// https://aspire.dev/integrations/security/keycloak/
var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithDataVolume()
    .WithRealmImport("./Infrastructure/Realms");

var serverWebProject = builder.AddProject("serverweb", "../SkyLogg.Server.Web/SkyLogg.Server.Web.csproj")
    .WithExternalHttpEndpoints();

// Adding health checks endpoints to applications in non-development environments has security implications.
// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
if (builder.Environment.IsDevelopment())
{
    serverWebProject.WithHttpHealthCheck("/alive");
}


serverWebProject.WithReference(sqlite).WaitFor(sqlite);
serverWebProject.WithReference(keycloak);

if (builder.ExecutionContext.IsRunMode) // The following project is only added for testing purposes.
{
    // Blazor WebAssembly Standalone project.
    builder.AddProject("clientwebwasm", "../../Client/SkyLogg.Client.Web/SkyLogg.Client.Web.csproj")
        .WithExplicitStart();

    var mailpit = builder.AddMailPit("smtp") // For testing purposes only, in production, you would use a real SMTP server.
        .WithDataVolume("mailpit");

    serverWebProject.WithReference(mailpit);


    var tunnel = builder.AddDevTunnel("web-dev-tunnel")
        .WithAnonymousAccess()
        .WithReference(serverWebProject.WithHttpEndpoint(name: "devTunnel", port: 5000).GetEndpoint("devTunnel"));

    if (OperatingSystem.IsWindows())
    {
        // Blazor Hybrid Windows project.
        builder.AddProject("clientwindows", "../../Client/SkyLogg.Client.Windows/SkyLogg.Client.Windows.csproj")
            .WithExplicitStart();
    }

    builder.AddMaui(serverWebProject, tunnel);
}

await builder
    .Build()
    .RunAsync();
