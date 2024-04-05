using ArnoldVinkCode;
using ArnoldVinkCode.Styles;
using System.IO;

namespace Updater
{
    public partial class App
    {
        //Application startup code
        private void AppStartup()
        {
            try
            {
                //Set the working directory to executable directory
                Directory.SetCurrentDirectory(AVFunctions.ApplicationPathRoot());

                //Load application styles
                AVResourceDictionary.LoadStyles();

                //Open application window
                AppVariables.WindowMain = new WindowMain();
                AppVariables.WindowMain.Show();
            }
            catch { }
        }
    }
}