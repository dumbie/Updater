using ArnoldVinkCode;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
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
                //Check launch arguments
                bool argumentProcessLaunch = AppVariables.StartupArguments.Any(x => x.ToLower() == "-processlaunch");

                //Close running updater
                TextBlockUpdate("Closing running updater.");
                if (AVProcess.Close_ProcessesByName("Updater", true))
                {
                    await Task.Delay(1000);
                }
                if (AVProcess.Close_ProcessesByName("UpdaterReplace", true))
                {
                    await Task.Delay(1000);
                }

                //Create resources directory
                Directory_Create("Resources", false);

                //Delete previous update files
                TextBlockUpdate("Deleting update files.");
                File_Delete("Resources/UpdaterReplace.exe");
                File_Delete("Resources/AppUpdate.zip");

                //Load current updater settings
                if (!LoadUpdaterSettings())
                {
                    await Application_Exit("Updater settings missing, closing in a bit.");
                    return;
                }

                //Set network security protocol
                TextBlockUpdate("Downloading update file.");
                AVDownloader.UpdateWebSecurityProtocol();

                //Download update from GitHub
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers[HttpRequestHeader.UserAgent] = "Application Updater";
                        webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

                        Uri downloadUri = GetPathLatestDownload(AppVariables.UpdaterSettings.UserName, AppVariables.UpdaterSettings.RepoName, AppVariables.UpdaterSettings.FileName);
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

                //Extract new updater settings
                try
                {
                    Debug.WriteLine("Extracting latest updater settings.");
                    TextBlockUpdate("Updating settings to the latest version.");
                    using (ZipArchive zipArchive = ZipFile.OpenRead("Resources/AppUpdate.zip"))
                    {
                        ZipArchiveEntry zipFile = zipArchive.Entries.Where(x => x.FullName.ToLower().EndsWith("Resources/Updater.json".ToLower())).FirstOrDefault();
                        string extractPath = AVFunctions.StringReplaceFirst(zipFile.FullName, AppVariables.UpdaterSettings.ExtractName + "/", string.Empty, false);
                        zipFile.ExtractToFile(extractPath, true);
                    }
                }
                catch
                {
                    Debug.WriteLine("Failed to extract latest settings file.");
                }

                //Load new updater settings
                if (!LoadUpdaterSettings())
                {
                    await Application_Exit("Updater settings missing, closing in a bit.");
                    return;
                }

                //Close running application
                TextBlockUpdate("Closing running application.");
                bool appRunning = false;
                if (AppVariables.UpdaterSettings.ProcessClose != null)
                {
                    foreach (string processName in AppVariables.UpdaterSettings.ProcessClose)
                    {
                        if (AVProcess.Close_ProcessesByName(processName, true))
                        {
                            appRunning = true;
                        }
                    }
                }

                //Wait for applications to have closed
                if (appRunning)
                {
                    await Task.Delay(1000);
                }

                //Delete unused files
                if (AppVariables.UpdaterSettings.FilesDelete != null)
                {
                    TextBlockUpdate("Deleting unused files.");
                    foreach (string fileName in AppVariables.UpdaterSettings.FilesDelete)
                    {
                        File_Delete(fileName);
                    }
                }

                //Delete unused folders
                if (AppVariables.UpdaterSettings.FoldersDelete != null)
                {
                    TextBlockUpdate("Deleting unused folders.");
                    foreach (string folderName in AppVariables.UpdaterSettings.FoldersDelete)
                    {
                        Directory_Delete(folderName);
                    }
                }

                //Move files
                if (AppVariables.UpdaterSettings.FilesMove != null)
                {
                    TextBlockUpdate("Moving files.");
                    foreach (string[] fileName in AppVariables.UpdaterSettings.FilesMove)
                    {
                        File_Move(fileName[0], fileName[1], true);
                    }
                }

                //Move folders
                if (AppVariables.UpdaterSettings.FoldersMove != null)
                {
                    TextBlockUpdate("Moving folders.");
                    foreach (string[] folderName in AppVariables.UpdaterSettings.FoldersMove)
                    {
                        Directory_Move(folderName[0], folderName[1], true);
                    }
                }

                //Copy files
                if (AppVariables.UpdaterSettings.FilesCopy != null)
                {
                    TextBlockUpdate("Copying files.");
                    foreach (string[] fileName in AppVariables.UpdaterSettings.FilesCopy)
                    {
                        File_Copy(fileName[0], fileName[1], true);
                    }
                }

                //Copy folders
                if (AppVariables.UpdaterSettings.FoldersCopy != null)
                {
                    TextBlockUpdate("Copying folders.");
                    foreach (string[] folderName in AppVariables.UpdaterSettings.FoldersCopy)
                    {
                        Directory_Copy(folderName[0], folderName[1], true);
                    }
                }

                //Extract the downloaded update archive
                try
                {
                    TextBlockUpdate("Updating application to the latest version.");
                    using (ZipArchive zipArchive = ZipFile.OpenRead("Resources/AppUpdate.zip"))
                    {
                        foreach (ZipArchiveEntry zipFile in zipArchive.Entries)
                        {
                            string extractPath = AVFunctions.StringReplaceFirst(zipFile.FullName, AppVariables.UpdaterSettings.ExtractName + "/", string.Empty, false);
                            if (!string.IsNullOrWhiteSpace(extractPath))
                            {
                                if (string.IsNullOrWhiteSpace(zipFile.Name))
                                {
                                    Directory_Create(extractPath, false);
                                }
                                else
                                {
                                    //Ignore update files and folders
                                    bool skipFileExtraction = false;
                                    if (AppVariables.UpdaterSettings.FilesIgnore != null)
                                    {
                                        foreach (string fileName in AppVariables.UpdaterSettings.FilesIgnore)
                                        {
                                            if (File.Exists(extractPath) && extractPath.ToLower().EndsWith(fileName.ToLower()))
                                            {
                                                Debug.WriteLine("Skipping file: " + extractPath);
                                                skipFileExtraction = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (AppVariables.UpdaterSettings.FoldersIgnore != null)
                                    {
                                        foreach (string folderName in AppVariables.UpdaterSettings.FoldersIgnore)
                                        {
                                            if (File.Exists(extractPath) && extractPath.ToLower().Contains(folderName.ToLower()))
                                            {
                                                Debug.WriteLine("Skipping folder: " + extractPath);
                                                skipFileExtraction = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (skipFileExtraction) { continue; }

                                    //Rename and move updater executable
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
                if (appRunning || argumentProcessLaunch)
                {
                    TextBlockUpdate("Starting updated version of the application.");
                    if (AppVariables.UpdaterSettings.ProcessLaunch != null)
                    {
                        foreach (string executableName in AppVariables.UpdaterSettings.ProcessLaunch)
                        {
                            AVProcess.Launch_ShellExecute(executableName, "", "", true);
                        }
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

        //Load updater settings
        private bool LoadUpdaterSettings()
        {
            try
            {
                string updaterSettingsPath = @"Resources\Updater.json";
                if (!File.Exists(updaterSettingsPath))
                {
                    return false;
                }

                string updaterSettingsText = File.ReadAllText(updaterSettingsPath);
                AppVariables.UpdaterSettings = JsonSerializer.Deserialize<UpdaterJson>(updaterSettingsText);

                Debug.WriteLine("Loaded updater settings.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load updater settings: " + ex.Message);
                return false;
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
                AVActions.DispatcherInvoke(delegate
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
                AVActions.DispatcherInvoke(delegate
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
                AVActions.DispatcherInvoke(delegate
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