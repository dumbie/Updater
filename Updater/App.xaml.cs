﻿using System.IO;
using System.Reflection;
using System.Windows;

namespace Updater
{
    public partial class App : Application
    {
        //Application Startup
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                //Set application startup arguments
                if (e.Args != null)
                {
                    AppVariables.StartupArguments = e.Args;
                }

                //Set the working directory to executable directory
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

                //Open the application window
                AppVariables.WindowMain.Show();
            }
            catch { }
        }
    }
}