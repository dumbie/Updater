using ArnoldVinkCode;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using Updater.Classes;
using static ArnoldVinkCode.ApiGitHub;
using static ArnoldVinkCode.AVFiles;

namespace Updater
{
    public partial class WindowMain : Window
    {
        //Window Initialize
        public WindowMain() { InitializeComponent(); }

        //Window Initialized
        protected override async void OnSourceInitialized(EventArgs e)
        {
            try
            {
                //Check resources directory
                Directory_Create("Resources", false);

                //Load updater settings
                string updaterSettingsPath = @"Resources\Updater.json";
                if (!File.Exists(updaterSettingsPath))
                {
                    Debug.WriteLine("Updater settings file missing.");
                    await Application_Exit("Updater files missing, closing in a bit.");
                    return;
                }

                string updaterSettingsText = File.ReadAllText(updaterSettingsPath);
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                UpdaterJson updaterSettings = jsonSerializer.Deserialize<UpdaterJson>(updaterSettingsText);

                //Delete previous update files
                TextBlockUpdate("Deleting update files.");
                File_Delete("Resources/UpdaterReplace.exe");
                File_Delete("Resources/AppUpdate.zip");
                foreach (string fileName in updaterSettings.FilesDelete)
                {
                    File_Delete(fileName);
                }

                //Close running application
                TextBlockUpdate("Closing running application.");
                bool appRunning = false;
                foreach (string processName in updaterSettings.ProcessClose)
                {
                    if (Processes.ProcessClose(processName))
                    {
                        appRunning = true;
                    }
                }

                //Wait for applications to have closed
                await Task.Delay(1000);

                //Set network security protocol
                try
                {
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to set security protocol: " + ex.Message);
                }

                //Download update from GitHub
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers[HttpRequestHeader.UserAgent] = "Application Updater";
                        webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

                        Uri downloadUri = new Uri(ApiGitHub_GetDownloadPath(updaterSettings.RepoName, updaterSettings.AppName, updaterSettings.FileName), UriKind.RelativeOrAbsolute);
                        await webClient.DownloadFileTaskAsync(downloadUri, "Resources/AppUpdate.zip");
                        Debug.WriteLine("Update file has been downloaded.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to download update: " + ex.Message + "/" + ex.InnerException);
                    await Application_Exit("Failed to download update, closing in a bit.");
                    return;
                }

                //Extract the downloaded update archive
                try
                {
                    TextBlockUpdate("Updating the application to the latest version.");
                    using (ZipArchive zipArchive = ZipFile.OpenRead("Resources/AppUpdate.zip"))
                    {
                        foreach (ZipArchiveEntry zipFile in zipArchive.Entries)
                        {
                            string extractPath = AVFunctions.StringReplaceFirst(zipFile.FullName, updaterSettings.ExtractName + "/", string.Empty, false);
                            if (!string.IsNullOrWhiteSpace(extractPath))
                            {
                                if (string.IsNullOrWhiteSpace(zipFile.Name))
                                {
                                    Directory_Create(extractPath, false);
                                }
                                else
                                {
                                    //Ignore update files
                                    bool skipFileExtraction = false;
                                    foreach (string fileName in updaterSettings.FilesIgnore)
                                    {
                                        if (File.Exists(extractPath) && extractPath.ToLower().EndsWith(fileName.ToLower()))
                                        {
                                            Debug.WriteLine("Skipping: " + fileName);
                                            skipFileExtraction = true;
                                            break;
                                        }
                                    }
                                    if (skipFileExtraction) { continue; }

                                    //Rename and move the updater
                                    if (File.Exists(extractPath) && extractPath.ToLower().EndsWith("Updater.exe".ToLower()))
                                    {
                                        Debug.WriteLine("Renaming Updater.exe to Resources/UpdaterReplace.exe");
                                        extractPath = extractPath.Replace("Updater.exe", "Resources/UpdaterReplace.exe");
                                    }

                                    //Extract the file
                                    zipFile.ExtractToFile(extractPath, true);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    await Application_Exit("Failed to extract update, closing in a bit.");
                    return;
                }

                //Start application after the update has completed.
                if (appRunning)
                {
                    TextBlockUpdate("Starting updated version of the application.");
                    foreach (string executableName in updaterSettings.ProcessLaunch)
                    {
                        Processes.ProcessStartAdmin(executableName);
                    }
                }

                //Close the application
                await Application_Exit("Application has been updated, closing in a bit.");
            }
            catch
            {
                //Close the application
                await Application_Exit("Application update failed, closing in a bit.");
            }
        }

        //Update download progress
        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            try
            {
                ProgressBarUpdate(args.ProgressPercentage, false);
                TextBlockUpdate("Downloading update file: " + args.ProgressPercentage + "%");
            }
            catch { }
        }

        //Update the textblock
        public void TextBlockUpdate(string textString)
        {
            try
            {
                AVActions.ActionDispatcherInvoke(delegate
                {
                    textblock_Status.Text = textString;
                });
            }
            catch { }
        }

        //Update the progressbar
        public void ProgressBarUpdate(double currentProgress, bool isIndeterminate)
        {
            try
            {
                AVActions.ActionDispatcherInvoke(delegate
                {
                    progressbar_Status.IsIndeterminate = isIndeterminate;
                    progressbar_Status.Value = currentProgress;
                });
            }
            catch { }
        }

        //Application Close Handler
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
            }
            catch { }
        }

        //Close the application
        async Task Application_Exit(string exitMessage)
        {
            try
            {
                Debug.WriteLine("Exiting updater.");

                //Disable the interface
                AVActions.ActionDispatcherInvoke(delegate
                {
                    this.Opacity = 0.80;
                    this.IsEnabled = false;
                });

                //Delete the update zip file
                File_Delete("Resources/AppUpdate.zip");

                //Set the exit reason text message
                TextBlockUpdate(exitMessage);
                ProgressBarUpdate(100, false);

                //Close the updater after x seconds
                await Task.Delay(2000);
                Environment.Exit(0);
            }
            catch { }
        }
    }
}