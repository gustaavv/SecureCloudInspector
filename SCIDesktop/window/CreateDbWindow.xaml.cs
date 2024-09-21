using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Ookii.Dialogs.Wpf;
using SCICore.api;
using SCICore.dao;
using SCICore.entity;

namespace SCIDesktop.window;

public partial class CreateDbWindow : MetroWindow
{
    private DatabaseDao DatabaseDao { get; set; }

    private ConfigDao ConfigDao { get; set; }

    public CreateDbWindow(DatabaseDao databaseDao, ConfigDao configDao)
    {
        InitializeComponent();

        PwdLevelComboBox.ItemsSource = Enum.GetValues(typeof(PasswordLevel));
        PwdLevelComboBox.SelectedItem = PasswordLevel.Db;

        DatabaseDao = databaseDao;
        ConfigDao = configDao;
    }

    private void ChooseFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var textBoxName = (string)button!.Tag + "TextBox";

        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Select a Folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != true) return;
        ((TextBox)FindName(textBoxName)!).Text = dialog.SelectedPath;
    }


    // see DatabaseCreate
    private void SubmitButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dbName = DbNameTextBox.Text;
        var sourceFolder = SourceFolderTextBox.Text;
        var encryptedFolder = EncryptedFolderTextBox.Text;
        var pwd = PasswordBox.Password;
        var pwdLevel = (PasswordLevel)PwdLevelComboBox.SelectedItem;

        if (string.IsNullOrWhiteSpace(dbName))
        {
            MessageBox.Show("Empty database name is not valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (DatabaseDao.CheckNameExists(dbName))
        {
            MessageBox.Show("A database with the same name has already existed.", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (!Directory.Exists(sourceFolder))
        {
            MessageBox.Show("Source folder doesn't exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!Directory.Exists(encryptedFolder))
        {
            MessageBox.Show("Encrypted folder doesn't exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (sourceFolder.Contains(encryptedFolder))
        {
            MessageBox.Show("Source folder should not be a child folder of encrypted folder.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (encryptedFolder.Contains(sourceFolder))
        {
            MessageBox.Show("Encrypted folder should not be a child folder of source folder.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(pwd))
        {
            MessageBox.Show("Empty password is not valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var db = new Database(dbName, sourceFolder, encryptedFolder, null!, new EncryptScheme(), pwd, DbType.Normal,
            DateTime.Today, DateTime.Today);

        db.EncryptScheme.PwdLevel = pwdLevel;
        db.EncryptScheme.FileNamePattern = EncryptApi.MakePattern((int)ConfigDao.Config.EncryptedFilenameLength);
        db.EncryptScheme.PwdPattern = EncryptApi.MakePattern((int)ConfigDao.Config.EncryptedArchivePwdLength);

        DatabaseDao.Insert(db);

        MessageBox.Show("succeed");
        DialogResult = true;
        Close();
    }
}