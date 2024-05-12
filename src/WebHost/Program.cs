﻿namespace HoU.GuildBot.WebHost;

public static class Program
{
    private static readonly Runner _runner;

    static Program()
    {
        _runner = new Runner();
    }

    public static async Task Main()
    {
        try
        {
            Startup.EnvironmentConfigured += Startup_EnvironmentConfigured;
            await Console.Out.WriteLineAsync("Building web host ...");
            var host = BuildWebHost();
            await Console.Out.WriteLineAsync("Running web host ...");
            await host.RunAsync(ApplicationLifecycle.CancellationToken);
            await Console.Out.WriteLineAsync("Exited web host.");
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.ToString());
            _runner.NotifyShutdown(e.ToString());
        }

        _runner.NotifyShutdown("no reason specified");
        Environment.FailFast("Shutting down process due to lacking connection.");
    }

    private static IWebHost BuildWebHost() =>
        Microsoft.AspNetCore.WebHost.CreateDefaultBuilder()
                 .ConfigureAppConfiguration(configurationBuilder =>
                  {
                      configurationBuilder.AddJsonFile("appsettings.json", true, true)
                                          .AddUserSecrets(typeof(Program).Assembly, true, true)
                                          .AddEnvironmentVariables("SETTINGS_OVERRIDE_");
                  })
                 .UseStartup<Startup>()
                 .Build();

    private static void Startup_EnvironmentConfigured(object? sender,
                                                      EnvironmentEventArgs e)
    {
        Startup.EnvironmentConfigured -= Startup_EnvironmentConfigured;
        _runner.Run(e.Environment,
                    e.RootSettings,
                    ApplicationLifecycle.CancellationToken);
    }
}