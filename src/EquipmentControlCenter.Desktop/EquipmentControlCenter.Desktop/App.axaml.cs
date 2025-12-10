using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using EquipmentControlCenter.Desktop.ViewModels;
using EquipmentControlCenter.Desktop.Views;
using EquipmentControlCenter.Desktop.Services;
using EquipmentControlCenter.Desktop.Services.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MassTransit;
using System;

namespace EquipmentControlCenter.Desktop;

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Build host with dependency injection
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<EquipmentServiceRegistry>();
                services.AddSingleton<ControlCommandService>();

                // Configure MassTransit
                services.AddMassTransit(x =>
                {
                    // Add consumers
                    x.AddConsumer<ServiceRegisteredConsumer>();
                    x.AddConsumer<ServiceHeartbeatConsumer>();
                    x.AddConsumer<ServiceStateChangedConsumer>();

                    // Add request client for control commands
                    x.AddRequestClient<Shared.Messages.ControlCommand>();

                    x.UsingRabbitMq((ctx, cfg) =>
                    {
                        // Get RabbitMQ connection string from Aspire (if running via AppHost)
                        var rabbitMqConnection = context.Configuration.GetConnectionString("rabbitmq");

                        if (!string.IsNullOrEmpty(rabbitMqConnection))
                        {
                            // Use connection string from Aspire
                            cfg.Host(new Uri(rabbitMqConnection));
                        }
                        else
                        {
                            // Fallback for local development (running standalone)
                            cfg.Host("localhost", "/", h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });
                        }

                        // Configure endpoints
                        cfg.ConfigureEndpoints(ctx);

                        // Configure retry
                        cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
                    });
                });

                // Register ViewModels
                services.AddTransient<MainViewModel>();
            })
            .Build();

        // Start the host
        _host.Start();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            // Handle shutdown gracefully
            desktop.Exit += async (s, e) =>
            {
                if (_host != null)
                {
                    await _host.StopAsync();
                    _host.Dispose();
                }
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();

            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}