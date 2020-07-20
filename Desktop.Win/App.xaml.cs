using Microsoft.Extensions.DependencyInjection;
using Remotely.ScreenCast.Core;
using Remotely.ScreenCast.Core.Services;
using Remotely.Shared.Utilities;
using Remotely.Shared.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Windows;

namespace Remotely.Desktop.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Write(e.Exception);
            MessageBox.Show("There was an unhandled exception.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) && !WindowsIdentity.GetCurrent().IsSystem)
            {
                try
                {
                    var psi = new ProcessStartInfo("cmd.exe")
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    };

                    var commandLine = Win32Interop.GetCommandLine().Replace(" -elevate", "");
                    var sections = commandLine.Split('"', StringSplitOptions.RemoveEmptyEntries);
                    var filePath = sections.First();
                    var arguments = string.Join('"', sections.Skip(1));

                    Logger.Write($"Creating temporary service with file path {filePath} and arguments {arguments}.");
                    psi.Arguments = $"/c sc create Remotely_Temp binPath=\"{filePath} {arguments} -elevate\"";
                    Process.Start(psi).WaitForExit();
                    psi.Arguments = "/c sc start Remotely_Temp";
                    Process.Start(psi).WaitForExit();
                    psi.Arguments = "/c sc delete Remotely_Temp";
                    Process.Start(psi).WaitForExit();
                    App.Current.Shutdown();
                }
                catch { }
            }

            if (Environment.GetCommandLineArgs().Contains("-elevate"))
            {
                var commandLine = Win32Interop.GetCommandLine().Replace(" -elevate", "");
                
                Logger.Write($"Elevating process {commandLine}.");
                var result = Win32Interop.OpenInteractiveProcess(
                    commandLine,
                    -1,
                    false,
                    "default",
                    true,
                    out var procInfo);

                Logger.Write($"Elevate result: {result}. Process ID: {procInfo.dwProcessId}.");
                Environment.Exit(0);
            }
        }
    }
}
