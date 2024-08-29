using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;
using SCICore.api;
using SCICore.dao;
using SCICore.entity;
using SCICore.util;

namespace SCIDesktop.window;

public partial class CreateDbWindow : Window
{
    private DatabaseIndexDao IndexDao { get; set; }

    public CreateDbWindow(DatabaseIndexDao indexDao)
    {
        InitializeComponent();

        PwdLevelComboBox.ItemsSource = Enum.GetValues(typeof(PasswordLevel));
        PwdLevelComboBox.SelectedItem = PasswordLevel.Db;

        IndexDao = indexDao;
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

        if (IndexDao.GetIndex().ContainsKey(dbName))
        {
            MessageBox.Show("A database with the same name has already existed.", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (DatabaseIndexDao.DbIndexFileName == $"{dbName}.json")
        {
            MessageBox.Show("This database name is not allowed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        var db = new Database(sourceFolder, encryptedFolder, null!, new EncryptScheme(), pwd);

        db.EncryptScheme.PwdLevel = pwdLevel;
        db.EncryptScheme.FileNamePattern = EncryptApi.MakePattern(15);
        db.EncryptScheme.PwdPattern = EncryptApi.MakePattern(60);


        var dbPath = Path.Join(IndexDao.DbFolder, $"{dbName}.json");
        _ = Task.Run(() => JsonUtils.Write(dbPath, db));
        _ = Task.Run(() => new DbDao(dbPath));

        var dbRecord = new DatabaseRecord();
        IndexDao.GetIndex()[dbName] = dbRecord;

        dbRecord.Filepath = dbPath;
        dbRecord.CreatedAt = DateTime.Today;
        dbRecord.UpdatedAt = DateTime.Today;

        _ = Task.Run(IndexDao.WriteDbIndex);

        MessageBox.Show("succeed");
        DialogResult = true;
        Close();
    }
}