using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Ookii.Dialogs.Wpf;
using SCICore.dao;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SCIDesktop.window;

public partial class DecryptWindow : MetroWindow
{
    private DatabaseDao DatabaseDao { get; set; }

    public DecryptWindow(DatabaseDao databaseDao, string dbName, ConfigDao configDao)
    {
        InitializeComponent();
        DatabaseDao = databaseDao;

        var databases = DatabaseDao.SelectAll();
        ChooseDbComboBox.ItemsSource = databases.Select(v => v.Name).ToList();
        ChooseDbComboBox.SelectedItem = dbName;

        DecryptPathTextBox.Text = configDao.Config.PreferredDecryptedPath;
    }

    private void ChooseFileButton_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "RAR Archive (*.rar)|*.rar",
            Title = "Select the filepath"
        };
        if (openFileDialog.ShowDialog() != true) return;
        EncFilePathTextBox.Text = openFileDialog.FileName;
    }

    private void ChooseFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        var textBoxName = (sender as Button)!.Tag as string;

        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Select a Folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != true) return;
        (FindName(textBoxName!) as TextBox)!.Text = dialog.SelectedPath;
    }

    private void DecryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dbName = (string)ChooseDbComboBox.SelectedItem;
        var db = DatabaseDao.SelectByName(dbName);
        // TODO
        MessageBox.Show("decrypt " + dbName);
    }
}