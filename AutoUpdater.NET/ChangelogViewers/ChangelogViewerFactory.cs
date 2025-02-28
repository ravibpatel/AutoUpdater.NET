using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AutoUpdaterDotNET.ChangelogViewers;

internal static class ChangelogViewerFactory
{
    private static readonly List<IChangelogViewerProvider> Providers =
    [
        new RichTextBoxViewerProvider(),
        new WebBrowserViewerProvider()
    ];

    static ChangelogViewerFactory()
    {
        if (AutoUpdater.AutoLoadExtensions)
            LoadExtensions();
    }

    public static void RegisterProvider(IChangelogViewerProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        var providerType = provider.GetType();
        var existingProvider = Providers.FirstOrDefault(p => p.GetType() == providerType);
        if (existingProvider != null)
        {
            if (existingProvider.Priority == provider.Priority)
                return; // Same type and priority, nothing to do

            // Remove existing provider to allow priority override
            Providers.Remove(existingProvider);
        }

        Providers.Add(provider);
    }

    internal static IChangelogViewer Create(IChangelogViewerProvider provider = null)
    {
        provider ??= AutoUpdater.ChangelogViewerProvider;

        if (provider != null)
        {
            if (!provider.IsAvailable)
                throw new InvalidOperationException($"The specified viewer provider '{provider.GetType().Name}' is not available in this environment.");

            return provider.CreateViewer();
        }

        // Get all available providers ordered by priority (highest first)
        var availableProviders = Providers
            .Where(p => p.IsAvailable)
            .OrderByDescending(p => p.Priority);

        var changelogViewerProvider = availableProviders.FirstOrDefault();

        if (changelogViewerProvider == null)
            throw new InvalidOperationException("No changelog viewers are available in this environment.");

        return changelogViewerProvider.CreateViewer();
    }

    private static void LoadExtensions()
    {
        var extensions = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "AutoUpdater.NET.*.dll");
        if (extensions.Length == 0) return;

        var iProviderType = typeof(IChangelogViewerProvider);
        foreach (var extension in extensions)
        {
            try
            {
                var assembly = Assembly.LoadFrom(extension);
                var providers = assembly
                    .GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && iProviderType.IsAssignableFrom(t));

                foreach (var provider in providers)
                {
                    try
                    {
                        var instance = (IChangelogViewerProvider)Activator.CreateInstance(provider);
                        if (instance?.IsAvailable == true)
                            RegisterProvider(instance);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to create instance of provider {provider.FullName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load extension {extension}: {ex.Message}");
            }
        }
    }
}