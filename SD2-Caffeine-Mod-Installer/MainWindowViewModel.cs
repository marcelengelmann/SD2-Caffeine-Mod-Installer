using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using TvSeriesCalendar.UtilityClasses;

namespace SD2_Caffeine_Mod_Installer
{
    public class MainWindowViewModel : ObservableObject
    {
        // Variables
        private string _installationStatus;
        private string _installationPath;
        private string _installButtonText;
        private string _backupPrefix = "_BACKUP_";

        public MainWindowViewModel()
        {
            InstallationPath = GetGameInstallationPath();
            InstallationStatus = GetModInstallationStatus();
            CloseApplicationCommand = new RelayCommand(CloseApplication);
            MinimizeApplicationCommand = new RelayCommand(MinimizeApplication);
            ChangeInstallationPathCommand = new RelayCommand(ChangeInstallationPath);
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

        public string InstallationPath
        {
            get => _installationPath;
            set
            {
                OnPropertyChanged(ref _installationPath, value);
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

        private void ChangeInstallationPath()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Select Soda Dungeon 2 Game Folder";
            dialog.IsFolderPicker = true;
            dialog.AddToMostRecentlyUsedList = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.EnsurePathExists = true;
            dialog.Multiselect = false;
            dialog.ShowPlacesList = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                InstallationPath = dialog.FileName;
            }
        }

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
                    }
                    return steamInstallationPath;
                }
            } 
            catch (Exception ex)
            {
                return "";
            }

        }

        private void InstallUninstallMod()
        {
            string dllFilePath = $@"{InstallationPath}\SodaDungeon2_Data\Managed\Assembly-CSharp.dll";
            string backupFilePath = $@"{InstallationPath}\SodaDungeon2_Data\Managed\{_backupPrefix}Assembly-CSharp.dll";

            if (InstallationStatus == "Installed")
            {
                if(File.Exists(backupFilePath) == false)
                    MessageBox.Show("Could not find the backup file!", "Installation failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                if(File.Exists(dllFilePath))
                    File.Delete(dllFilePath);
                File.Move(backupFilePath, dllFilePath);
                InstallationStatus = "Not installed";
            }
            else
            {
                if (File.Exists(dllFilePath) == false)
                {
                    MessageBox.Show("Could not find the Game location.\nPlease check the installation path.", "Installation failed!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if(File.Exists(backupFilePath))
                    File.Delete(backupFilePath);
                File.Move(dllFilePath, backupFilePath);
                File.WriteAllBytes(dllFilePath, Properties.Resources.Assembly_CSharp);
                InstallationStatus = "Installed";
            }
        }

        private string GetModInstallationStatus()
        {
            string dllPath = $@"{InstallationPath}\SodaDungeon2_Data\Managed\Assembly-CSharp.dll";
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