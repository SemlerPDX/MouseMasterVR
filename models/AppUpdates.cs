using MouseMasterVR.Properties;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using VGLabsFoundation;

namespace MouseMasterVR
{
    public static class AppUpdates
    {

        public static void FirstTimeUseNotice()
        {
            try
            {
                MessageBoxResult result = MessageBox.Show(
                    "Thank you for checking out my little app!\n" +
                    "\n" +
                    "This pop-up will never appear again, just a brief reminder " +
                    "that this is a public Beta test. Constructive criticism is very welcome!\n" +
                    "\n" +
                    "I have tested extensively, but only on my own computers. \n" +
                    "Various VR headsets, games, or PC's may operate differently " +
                    "with regards to display targeting and mouse cursor centering.\n" +
                    "\n" +
                    "\n" +
                    "If something doesn't work, please let me know and I'll " +
                    "do my best to fix it and release an update. \n" +
                    "    -SemlerPDX    (Jan2023)\n" +
                    "\n" +
                    "\n" +
                    "==HOTKEYS (when app is in focus)==\n" +
                    "CTRL+Arrow Key [any]  -  centers app on primary monitor \n" +
                    "ALT+F4  -  gracefully closes app",
                    Settings.Default.AppTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                Settings.Default.FirstTimeUse = false;
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error during AppUpdates First Time Use Notice");
            }
        }

        private static void UpdateFailureNotice()
        {
            try
            {
                MessageBoxResult result = MessageBox.Show(
                    "The automatic updates system has failed!\n" +
                    "\n" +
                    "There could be a number of reasons why, please " +
                    "try again later. If this problem persists, contact " +
                    "SemlerPDX. You may consider a manual update, as well.\n" +
                    "\n" +
                    "Check for an errors log in the application folder.",
                    Settings.Default.AppTitle + "   -   Automatic Updates",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error during AppUpdates Failure Notice");
            }
        }

        private static void CombineUserSettings()
        {
            try
            {
                string vgLabsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VG_Labs");
                string assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                if (Directory.Exists(vgLabsPath))
                {
                    // Sort through Settings Files in the VG_Labs settings folder matching this app name
                    var subfolders = Directory.GetDirectories(vgLabsPath).Where(f => f.StartsWith(vgLabsPath + "\\" + assemblyName));
                    if (subfolders.Count() > 1)
                    {
                        // Find most recent settings folder for this app and store its name
                        var mostRecentFolder = subfolders.OrderByDescending(d => Directory.GetCreationTime(d)).First();
                        foreach (var folder in subfolders.Where(f => f != mostRecentFolder))
                        {
                            // Sort through each matching settings folder of previous version grabbing most recent each time
                            var versionFolder = Directory.GetDirectories(folder).OrderByDescending(d => Directory.GetCreationTime(d)).First();
                            var destConfigPath = System.IO.Path.Combine(mostRecentFolder, System.IO.Path.GetFileName(versionFolder));

                            // Create a versions folder in most recent settings
                            if (!Directory.Exists(destConfigPath))
                            {
                                Directory.CreateDirectory(destConfigPath);
                            }

                            // Create source and destination paths, move old settings version folder to new settings main folder
                            var srcConfigPath = System.IO.Path.Combine(versionFolder, "user.config");
                            var finalDestConfigPath = System.IO.Path.Combine(destConfigPath, "user.config");
                            File.Copy(srcConfigPath, System.IO.Path.Combine(destConfigPath, "user.config"), true);
                            Directory.Delete(folder, true);
                        }
                    }
                }
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error combining settings file(s) from previous version(s)");
            }
        }

        public static bool CleanupPreviousUserSettings()
        {
            try
            {
                var level = ConfigurationUserLevel.PerUserRoamingAndLocal;
                var configuration = ConfigurationManager.OpenExeConfiguration(level);
                var configurationFilePath = configuration.FilePath;

                string thisAppSettingsFolders = System.IO.Path.GetDirectoryName(configurationFilePath);
                string thisAppSettingsFolder = System.IO.Path.GetDirectoryName(thisAppSettingsFolders);

                var subfolders = Directory.GetDirectories(thisAppSettingsFolder);
                if (subfolders != null && subfolders.Any())
                {
                    if (subfolders.Length > 1)
                    {
                        var oldVersionfolders = subfolders.Where(f => f != thisAppSettingsFolders);
                        foreach (var folder in oldVersionfolders)
                        {
                            try
                            {
                                Directory.Delete(folder, true);
                            }
                            catch (Exceptions ex)
                            {
                                ex.LogError("Error in delete folder of old version in combined settings folder");
                            }
                        }
                        return true;
                    }
                }
                return false;

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error cleaning up settings file(s) from previous version(s)");
                return false;
            }
        }


        public static void UpgradeSettings()
        {
            try
            {
                // Save the current user settings before upgrade attempt
                string appName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Settings.Default.AppTitle = appName + "  v" + appVersion;
                Settings.Default.Save();

                // Combine any existing previous version settings
                CombineUserSettings();

                // Ensure settings always upgraded to the latest version
                Settings.Default.Upgrade();

                // Reload any upgraded settings
                Settings.Default.Reload();

                // Remove any existing old version folders from combined settings folder
                CleanupPreviousUserSettings();

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error upgrading settings file from previous version");
            }
        }

        public static bool RunAutomaticUpdate(string appName, string[] updateData, bool openChangelog)
        {
            try
            {
                // Perform Update through VGLabsFoundation
                string root = AppDomain.CurrentDomain.BaseDirectory;
                string downloadPath = System.IO.Path.Combine(root, "update");
                string downloadPackagePath = downloadPath + @"\" + appName + "_v" + updateData[0].Replace(".", "_") + ".zip";
                Settings.Default.UpdatePackagePath = downloadPackagePath;

                // Acquire, Authenticate, and Extract the updated application package at the above specified path
                if (Acquisition.AcquireUpdatePackage(downloadPath, appName, updateData, openChangelog))
                {
                    Thread.Sleep(2000); // Short Wait for app to exist
                    string updatedApp = System.IO.Path.Combine(downloadPath, @"temp\" + appName + ".exe");

                    Settings.Default.AppUpdated = false;
                    Settings.Default.AppUpdating = Applications.LaunchApp(updatedApp);
                    Settings.Default.Save();

                    // Returning 'true' will close this app instance
                    return Settings.Default.AppUpdating;
                }
                else
                {
                    UpdateFailureNotice();
                }

                return false;
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error during AppUpdates Run Automatic Update");
                return false;
            }
        }

        public static bool UpdateCheck()
        {
            try
            {
                string appNameToGet = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                bool openChangelog = false;

                string[] appData = Acquisition.CheckForUpdates(appNameToGet, appVersion);
                if (appData != null)
                {
                    Settings.Default.AppChangelog = appData[7];
                    if (Settings.Default.UpdateModeDial == 2)
                    {
                        // Present Automatic Update Message Choice Box on Update Found
                        // Yes/No/Cancel == "Open Changelog & Update / Update / Don't Update"
                        MessageBoxResult result = MessageBox.Show(
                                "An app update has been found!\n" +
                                "\n" +
                                "Version: " + appData[0] + "\n" +
                                "Featured Change:\n" + appData[1] + "\n" +
                                "\n" +
                                "\n" +
                                "This update will now be applied, it should only take seconds, \n" +
                                "and all saved user settings will be automatically restored.\n" +
                                "\n" +
                                "\n" +
                                "Would you like to view the latest changelog?\n" +
                                "\n" +
                                "YES: Apply Update and Open Changelog\n" +
                                "NO: Apply Update\n" +
                                "CANCEL: Postpone Update",
                                Settings.Default.AppTitle + "   -   Automatic Updates",
                                MessageBoxButton.YesNoCancel,
                                MessageBoxImage.Question
                        );

                        // Perform Automatic Update operations
                        if ((int)result > 5)
                        {
                            if ((int)result == 6)
                            {
                                // Opens Changelog in default web browser
                                openChangelog = true;
                            }

                            return RunAutomaticUpdate(appNameToGet, appData, openChangelog);

                        }
                    }
                    else
                    {
                        // Present Manual Update Message Choice Box on Update Found
                        // OK/Cancel == "Open GitHub Repo / Do Nothing"
                        MessageBoxResult result = MessageBox.Show(
                            "An app update has been found!\n" +
                            "\n" +
                            "Version: " + appData[0] + "\n" +
                            "Featured Change:\n" + appData[1] + " \n" +
                            "\n" +
                            "This update is available at the following link:\n" +
                            appData[8] + " \n" +
                            "\n" +
                            "\n" +
                            "Press OK to open the repository webpage listed above, \n" +
                            "where you can manually download or compile this app.\n" +
                            "\n" +
                            "Simply replace the old " + appNameToGet + ".exe with the new one, " +
                            "all saved user settings will be restored on first launch.",
                            Settings.Default.AppTitle + "   -   Manual Updates",
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Information
                        );

                        // Open GitHub Repo for Manual Download/Update
                        if ((int)result == 1)
                        {
                            _ = Applications.LaunchApp(appData[8]);
                        }
                    }
                }

                return false;

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error during AppUpdate Update Check");
                return false;
            }
        }

        public static bool CanUpdateCheck()
        {
            try
            {
                if ((Settings.Default.UpdateModeDial > 0) || Settings.Default.AppUpdated || Settings.Default.AppUpdating)
                {
                    // On standard operations, ensure location of this assembly is saved for updates later
                    if (!Settings.Default.AppUpdated && !Settings.Default.AppUpdating)
                        Settings.Default.UpdateFileTarget = System.Reflection.Assembly.GetEntryAssembly().Location;

                    // When mode set to Automatic Updates, returning true can signal Close() of this app to begin using updated App
                    bool updatedAppIsRunning = UpdateOperations(Settings.Default.AppUpdated, Settings.Default.AppUpdating);
                    return updatedAppIsRunning;

                }

                return false;
            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in AppUpdates Can Update Check method");
                return false;
            }
        }

        public static bool UpdateOperations(bool isUpdated, bool isUpdating)
        {
            try
            {
                if (!isUpdated && !isUpdating)
                {
                    //Standard Operation, user updates mode is not set to "none"
                    Settings.Default.AppUpdating = UpdateCheck();
                    Settings.Default.Save();

                    // If true, will signal to stop this instance and that (temp) update process is now running
                    return Settings.Default.AppUpdating;
                }
                else if (!isUpdated && isUpdating)
                {
                    // Launch is (Temp) during update process, should extract authenticated package to UpdateFileTarget
                    Thread.Sleep(2000); //Just a short pause to help ensure prior instance is closed before applying update

                    string updatePackagePath = Settings.Default.UpdatePackagePath;
                    string updateFileTarget = Settings.Default.UpdateFileTarget;
                    string fullName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

                    // Set the updated flag bool (if false/unsuccessful, can start over with UpdateCheck next launch)
                    Settings.Default.AppUpdated = Applications.ApplyUpdate(updatePackagePath, updateFileTarget, fullName);
                    Settings.Default.AppUpdating = false;
                    Settings.Default.Save();

                    if (Settings.Default.AppUpdated)
                    {
                        MessageBoxResult updatedResult = MessageBox.Show(
                            "UPDATE COMPLETE!  The application will now close...\n" +
                            "\n" +
                            "Please restart the " + fullName + " app!\n" +
                            "\n" +
                            "\n" +
                            "You can take this opportunity to validate the updated app " +
                            "against the published checksum before using, if you wish.\n" +
                            "\n" +
                            "\n" +
                            "Press OK to Open latest Changelog & Checksum",
                            Settings.Default.AppTitle + "    (UPDATED!)",
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Information
                        );

                        // Open the latest changelog and checksum
                        if ((int)updatedResult == 1)
                        {
                            _ = Applications.LaunchApp(Settings.Default.AppChangelog);
                        }
                    }
                    else
                    {
                        // If update failure, it was error - stop and informing user
                        Exceptions.LogMessage("Error at UpdateOperations in temp apply update phase");
                        UpdateFailureNotice();
                        // Will stop instance even though update is not ready
                        return true;
                    }

                    // If true, will signal to stop this instance and that final updated version is now ready
                    return Settings.Default.AppUpdated;
                }
                else if (isUpdated && !isUpdating)
                {
                    //Launch is new Updated App, cleanup updater files by deleting update folder recursive
                    string root = AppDomain.CurrentDomain.BaseDirectory;
                    string downloadPath = System.IO.Path.Combine(root, "update");
                    _ = Applications.UpdateCleanup(downloadPath); // (failure here is acceptable)
                    Settings.Default.AppUpdated = false; // Reset Updated flag to normal

                    //Returning False here (or on 'else') is indicating not to stop this application instance
                }

                return false;

            }
            catch (Exceptions ex)
            {
                ex.LogError("Error in AppUpdates main Update Operations method");
                return false;
            }
        }

    }
}
