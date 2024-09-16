using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SCICore.dao;

namespace SCIDesktop.control;

public partial class SettingControl : UserControl
{
    private bool SettingChanged { get; set; } = false;

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

    private void ChooseFileButton_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "All files (*.*)|*.*",
            Title = "Select the filepath"
        };
        if (openFileDialog.ShowDialog() != true) return;

        var button = sender as Button;
        var property = (string)button!.Tag;
        var propertyInfo = typeof(SettingControl).GetProperty(property)!;

        var newFilePath = openFileDialog.FileName;
        var oldFilePath = propertyInfo.GetValue(this);

        if (oldFilePath != newFilePath)
        {
            propertyInfo.SetValue(this, newFilePath);
            var textBox = (TextBox)FindName($"{property}TextBox")!;
            textBox.Text = newFilePath;
            SettingChanged = true;
        }
    }
}