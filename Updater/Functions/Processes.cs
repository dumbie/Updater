using System;
using System.Diagnostics;

namespace Updater
{
    public class Processes
    {
        //Launch process as administrator
        public static void ProcessStartAdmin(string fileName)
        {
            try
            {
                using (Process startProcess = new Process())
                {
                    startProcess.StartInfo.FileName = fileName;
                    startProcess.StartInfo.Verb = "RunAs";
                    startProcess.Start();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to start process: " + ex.Message);
            }
        }

        //Close processes
        public static bool ProcessClose(string processName)
        {
            try
            {
                bool processClosed = false;
                foreach (Process closeProcess in Process.GetProcessesByName(processName))
                {
                    processClosed = true;
                    closeProcess.Kill();
                }
                return processClosed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to close processes: " + ex.Message);
                return false;
            }
        }
    }
}