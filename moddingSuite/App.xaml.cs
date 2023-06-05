using moddingSuite.BL;
using moddingSuite.View;
using moddingSuite.View.DialogProvider;
using moddingSuite.View.Edata;
using moddingSuite.ViewModel.Edata;
using moddingSuite.ViewModel.UnhandledException;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace moddingSuite;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        if (path != null)
        {
            string file = Path.Combine(path, $"logging_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ff")}.dat");

            Trace.Listeners.Add(new TextWriterTraceListener(file));
            Trace.AutoFlush = true;
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        bool startApplication = false;

        Model.Settings.Settings settings = SettingsManager.Load();
        EdataManagerView mgr = new();


        if (settings.InitialSettings)
        {
            SettingsView settingsView = new()
            {
                DataContext = settings
            };

            bool? result = settingsView.ShowDialog();
            if (result.GetValueOrDefault(false))
            {
                if (Directory.Exists(settings.SavePath) && Directory.Exists(settings.WargamePath))
                    settings.InitialSettings = false;

                SettingsManager.Save(settings);
                startApplication = true;
            }
        }
        else
        {
            startApplication = true;
        }

        if (startApplication)
        {
            EdataManagerViewModel mainVm = new();
            mgr.DataContext = mainVm;
            mgr.Show();
        }
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        UnhandledExceptionViewModel vm = new(e.Exception);
        DialogProvider.ProvideView(vm);

        Exception excep = e.Exception;

        while (excep != null)
        {
            Trace.TraceError("Unhandeled exception occoured: {0}", e.Exception);
            excep = excep.InnerException;
        }
    }
}