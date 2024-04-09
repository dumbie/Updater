using ArnoldVinkCode;

namespace Updater
{
    public partial class App
    {
        //Application startup code
        private void AppStartup(string[] launchArgs)
        {
            try
            {
                //Set application startup arguments
                if (launchArgs != null)
                {
                    AppVariables.StartupArguments = launchArgs;
                }

                //Set the working directory to executable directory
                AVFunctions.ApplicationUpdateWorkingPath();

                //Open application window
                AppVariables.WindowMain.Show();
            }
            catch { }
        }
    }
}