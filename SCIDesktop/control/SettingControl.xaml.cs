using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using SCICore.dao;
using SCICore.util;

namespace SCIDesktop.control;

public partial class SettingControl : UserControl
{
    private bool SettingChanged { get; set; } = false;

    public ConfigDao ConfigDao { get; set; }

    public string DbFolderPath => ConfigDao.GetDbFolder();

    public string? RarPath => ConfigDao.Config.RarPath;

    public SettingControl(ConfigDao configDao)
    {
        InitializeComponent();

        ConfigDao = configDao;
        DataContext = this;
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (SettingChanged)
        {
            _ = Task.Run(ConfigDao.WriteConfig);
            MessageBox.Show("Settings saved.", "Save Settings", MessageBoxButton.OK);
        }
        else
        {
            MessageBox.Show("Nothing changed.", "Save Settings", MessageBoxButton.OK);
        }

        SettingChanged = false;
    }

    private void ChooseRarButton_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Executable files (*.exe)|*.exe",
            Title = "Select the Path to rar.exe"
        };
        if (openFileDialog.ShowDialog() != true) return;
        var newRarPath = openFileDialog.FileName;

        var process = ProcessUtils.CreateProcess(newRarPath, "");
        var (success, _, _, _) = Task.Run(() => ProcessUtils.RunProcess(process)).Result;
        if (!success)
        {
            MessageBox.Show("Not a valid exe", "Selection Result", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var oldRarPath = ConfigDao.Config.RarPath;

        if (oldRarPath != newRarPath)
        {
            ConfigDao.Config.RarPath = newRarPath;
            RarPathTextBox.Text = newRarPath;
            SettingChanged = true;
        }
    }

    private void ChooseDbFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Select a DB Folder",
            UseDescriptionForTitle = true, // Shows the description as the title
            ShowNewFolderButton = true // Allows the user to create a new folder
        };

        if (dialog.ShowDialog() != true) return;
        var newDbFolderPath = dialog.SelectedPath;

        if (newDbFolderPath != DbFolderPath)
        {
            ConfigDao.Config.DbFolder = newDbFolderPath;
            DbFolderTextBox.Text = newDbFolderPath;
            SettingChanged = true;
        }
    }
}