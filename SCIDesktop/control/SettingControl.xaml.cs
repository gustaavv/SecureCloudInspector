using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using SCICore.dao;

namespace SCIDesktop.control;

public partial class SettingControl : UserControl
{
    private ConfigDao ConfigDao { get; set; }

    public string EncDbPath
    {
        get => ConfigDao.Config.EncDbPath;
        set => ConfigDao.Config.EncDbPath = value;
    }

    public string DecDbPath
    {
        get => ConfigDao.Config.DecDbPath;
        set => ConfigDao.Config.DecDbPath = value;
    }

    public string? RarPath
    {
        get => ConfigDao.Config.RarPath;
        set => ConfigDao.Config.RarPath = value;
    }

    public bool CreateDigestWhenExport
    {
        get => ConfigDao.Config.CreateDigestWhenExport;
        set => ConfigDao.Config.CreateDigestWhenExport = value;
    }

    public bool VerifyDigestWhenImport
    {
        get => ConfigDao.Config.VerifyDigestWhenImport;
        set => ConfigDao.Config.VerifyDigestWhenImport = value;
    }

    public string PreferredDecryptedPath
    {
        get => ConfigDao.Config.PreferredDecryptedPath;
        set => ConfigDao.Config.PreferredDecryptedPath = value;
    }

    public SettingControl(ConfigDao configDao)
    {
        InitializeComponent();

        ConfigDao = configDao;
        DataContext = this;
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        _ = Task.Run(ConfigDao.WriteConfig);
        MessageBox.Show("Settings saved.", "Save Settings", MessageBoxButton.OK);
    }

    private void ChooseFileButton_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "All files (*.*)|*.*",
            Title = "Select the filepath"
        };
        if (openFileDialog.ShowDialog() != true) return;

        var textBoxName = (string)(sender as Button)!.Tag;
        ((TextBox)FindName(textBoxName)!).Text = openFileDialog.FileName;
    }

    private void ChooseFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Select a Folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };
        if (dialog.ShowDialog() != true) return;

        var textBoxName = (string)(sender as Button)!.Tag;
        ((TextBox)FindName(textBoxName)!).Text = dialog.SelectedPath;
    }
}