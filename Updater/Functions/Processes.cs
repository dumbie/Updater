using System;
using System.Diagnostics;

namespace Updater
{
    public class Processes
    {
        //Launch process as administrator
        public static bool ProcessStartAdmin(string fileName)
        {
            try
            {
                using (Process startProcess = new Process())
                {
                    startProcess.StartInfo.FileName = fileName;
                    startProcess.StartInfo.Verb = "RunAs";
                    return startProcess.Start();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to start process: " + ex.Message);
                return false;
            }
        }

        //Close processes
        public static bool ProcessClose(string processName)
        {
            try
            {
                bool processClosed = false;
                Process currentProcess = Process.GetCurrentProcess();
                foreach (Process closeProcess in Process.GetProcessesByName(processName))
                {
                    try
                    {
                        if (closeProcess.Id != currentProcess.Id)
                        {
                            closeProcess.Kill();
                            processClosed = true;
                        }
                    }
                    catch { }
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