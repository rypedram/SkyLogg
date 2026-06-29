using Aspire.Hosting.Maui;
using Aspire.Hosting.DevTunnels;

namespace Aspire.Hosting;

public static class IDistributedApplicationBuilderExtensions
{




    /// <summary>
    /// Adds a SQLite database instance with a web-based management UI.
    /// </summary>
    public static IResourceBuilder<SqliteResource> AddSqlite(this IDistributedApplicationBuilder builder)
    {
        return builder.AddSqlite("sqlite", databaseFileName: "SkyLoggDb.db")
            .WithSqliteWeb();
    }


    /// <summary>
    /// Adds the .NET MAUI Blazor Hybrid project and configures it for all supported device targets
    /// (Windows, macOS Catalyst, iOS Device, iOS Simulator, Android Device, Android Emulator).
    /// Uses dev tunnels for OpenTelemetry data collection on mobile/remote targets.
    /// </summary>
    public static IResourceBuilder<MauiProjectResource> AddMaui(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> serverWebProject,
        IResourceBuilder<DevTunnelResource> tunnel)
    {
        var mauiapp = builder.AddMauiProject("mauiapp", @"../../Client/SkyLogg.Client.Maui/SkyLogg.Client.Maui.csproj");

        if (OperatingSystem.IsWindows())
        {
            mauiapp.AddWindowsDevice()
                .WithExplicitStart()
                .WithReference(serverWebProject);
        }

        if (OperatingSystem.IsMacOS())
        {
            mauiapp.AddMacCatalystDevice()
                .WithExplicitStart()
                .WithReference(serverWebProject);
        }

        if (OperatingSystem.IsMacOS())
        {
            // Windows supports iOS Simulator and Physical devices if there's a mac connected to network, but the following runners only work on macOS for now.

            mauiapp.AddiOSDevice()
                .WithExplicitStart()
                .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
                .WithReference(serverWebProject, tunnel);

            mauiapp.AddiOSSimulator()
                .WithExplicitStart()
                .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
                .WithReference(serverWebProject, tunnel);
        }

        mauiapp.AddAndroidDevice()
            .WithExplicitStart()
            .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
            .WithReference(serverWebProject, tunnel);

        mauiapp.AddAndroidEmulator()
            .WithExplicitStart()
            .WithOtlpDevTunnel() // Required for OpenTelemetry data collection
            .WithReference(serverWebProject, tunnel);

        return mauiapp;
    }
}
