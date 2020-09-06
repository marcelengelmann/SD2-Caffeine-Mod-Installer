using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using TvSeriesCalendar.UtilityClasses;

namespace SD2_Caffeine_Mod_Installer
{
    public class MainWindowViewModel : ObservableObject
    {
        // Variables
        private string _installationStatus;
        private string _gameLocationPath;
        private string _installButtonText;
        private const string BackupPrefix = "_BACKUP_";

        public MainWindowViewModel()
        {
            GameLocationPath = GetGameInstallationPath();
            InstallationStatus = GetModInstallationStatus();
            CloseApplicationCommand = new RelayCommand(CloseApplication);
            MinimizeApplicationCommand = new RelayCommand(MinimizeApplication);
            ChangeInstallationPathCommand = new RelayCommand(ChangeGameLocationPath);
            InstallUninstallModCommand = new RelayCommand(InstallUninstallMod);
        }

        // Properties
        public ICommand CloseApplicationCommand { get; }
        public ICommand MinimizeApplicationCommand { get; }
        public ICommand ChangeInstallationPathCommand { get; }
        public ICommand InstallUninstallModCommand { get; }

        public string InstallationStatus
        {
            get => _installationStatus;
            set
            {
                OnPropertyChanged(ref _installationStatus, value);
                InstallButtonText = value == "Installed" ? "Uninstall" : "Install";
            }
        }

        public string GameLocationPath
        {
            get => _gameLocationPath;
            set
            {
                OnPropertyChanged(ref _gameLocationPath, value);
                InstallationStatus = GetModInstallationStatus();
            }
        }

        public string InstallButtonText
        {
            get => _installButtonText;
            set => OnPropertyChanged(ref _installButtonText, value);
        }

        // Methods

        private void CloseApplication()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Application.Current.Shutdown();
        }

        private void MinimizeApplication()
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// OpenFileDialog to pick the game location folder
        /// </summary>
        private void ChangeGameLocationPath()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Select Soda Dungeon 2 Game Folder";
            dialog.IsFolderPicker = true;
            dialog.AddToMostRecentlyUsedList = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.EnsurePathExists = true;
            dialog.Multiselect = false;
            dialog.ShowPlacesList = true;
            dialog.InitialDirectory = GameLocationPath;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                GameLocationPath = dialog.FileName;
            }
        }

        /// <summary>
        /// Try to find the installation path of the Soda Dungeon 2 Game
        /// </summary>
        /// <returns>The installation path to the game, to steam or an empty string if no location was found</returns>
        private string GetGameInstallationPath()
        {
            try
            {
                using (RegistryKey key =
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    string steamInstallationPath = key.GetValue("InstallPath").ToString();
                    string gamePath = $@"{steamInstallationPath}\steamapps\common\Soda Dungeon 2\SodaDungeon2.exe";

                    if (File.Exists(gamePath))
                    {
                        return $@"{steamInstallationPath}\steamapps\common\Soda Dungeon 2";
                    }
                    else{
                        // check the file libraryfolders.vdf inside the steamapps folder to get additional installation folders for steam games
                        // -> read further : https://stackoverflow.com/a/34091380
                        return steamInstallationPath;
                    }
                }
            } 
            catch (Exception)
            {
                return "";
            }

        }

        /// <summary>
        /// Installs or Uninstalls the mod depending on the current installation status
        /// </summary>
        private void InstallUninstallMod()
        {

            string dllFileName = "Assembly-CSharp.dll";

            string dllFolderPath = $@"{GameLocationPath}\SodaDungeon2_Data\Managed\";
            string dllFilePath = $"{dllFolderPath}{dllFileName}";

            string dllBackupFolderPath = $@"{GameLocationPath}\SodaDungeon2_Data\Managed\";
            string dllBackupFilePath = $"{dllBackupFolderPath}{BackupPrefix}{dllFileName}";

            if (InstallationStatus == "Installed")
            {
                if (File.Exists(dllBackupFilePath) == false)
                {
                    MessageBox.Show("Could not find the backup file!", "Uninstall failed!", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if(File.Exists(dllFilePath))
                    File.Delete(dllFilePath);
                File.Move(dllBackupFilePath, dllFilePath);
                InstallationStatus = "Not installed";
            }
            else
            {
                // check if the game folder exists
                if (Directory.Exists(dllFolderPath) == false)
                {
                    MessageBox.Show("Could not find the Game location.\nPlease check the installation path.", "Installation failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (File.Exists(dllFileName) == false)
                {
                    MessageBox.Show("Could not find the Mod file.\nRe-download the latest release", "Installation failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //only delete the backup if a new backup will be created
                if (File.Exists(dllBackupFilePath) == true && File.Exists(dllFilePath) == true)
                {
                    File.Delete(dllBackupFilePath);
                }

                //create a new backup
                if (File.Exists(dllFilePath) == true)
                {
                    File.Move(dllFilePath, dllBackupFilePath);
                }

                // install mod
                File.Copy(dllFileName, dllFilePath);

                InstallationStatus = "Installed";
            }
        }

        /// <summary>
        /// Check the installation status of the mod by checking the product name of the dll
        /// </summary>
        /// <returns> "Installed" or "Not installed"</returns>
        private string GetModInstallationStatus()
        {
            string dllPath = $@"{GameLocationPath}\SodaDungeon2_Data\Managed\Assembly-CSharp.dll";
            if (File.Exists(dllPath))
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(dllPath);
                if (fvi.ProductName == "Caffeine")
                {
                    return "Installed";
                }
            }

            return "Not installed";
        }

    }
}