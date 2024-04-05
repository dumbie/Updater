using System;
using System.Windows;

namespace Updater
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                //Resolve missing assembly dll files
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveEmbedded;

                //Set application startup arguments
                if (e.Args != null)
                {
                    AppVariables.StartupArguments = e.Args;
                }

                //Run application startup code
                AppStartup();
            }
            catch { }
        }
    }
}