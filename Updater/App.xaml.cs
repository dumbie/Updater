using System.Windows;

namespace Updater
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                //Run application startup code
                AppStartup(e.Args);
            }
            catch { }
        }
    }
}