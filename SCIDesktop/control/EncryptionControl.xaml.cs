using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SCICore.api;
using SCICore.dao;
using SCICore.util;
using SCIDesktop.window;

namespace SCIDesktop.control;

public partial class EncryptionControl : UserControl
{
    public ConfigDao ConfigDao { get; set; }
    public DatabaseDao DatabaseDao { get; set; }

    public EncryptionControl(ConfigDao configDao)
    {
        InitializeComponent();
        ConfigDao = configDao;
        DatabaseDao = new DatabaseDao(configDao.Config.EncDbPath);
        RefreshDbList();
    }

    public class DbInfoRow
    {
        public string Name { get; set; }
        public string SourceFolder { get; set; }
        public string EncryptedFolder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public DbInfoRow(string name, string sourceFolder, string encryptedFolder,
            DateTime createdAt, DateTime updatedAt)
        {
            Name = name;
            SourceFolder = sourceFolder;
            EncryptedFolder = encryptedFolder;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }

    private void RefreshDbList()
    {
        var databases = DatabaseDao.SelectAll();
        var list = databases.Select(db =>
                new DbInfoRow(db.Name, db.SourceFolder, db.EncryptedFolder, db.CreatedAt, db.UpdatedAt))
            .ToList();

        DbList.ItemsSource = list;
    }


    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshDbList();
        MessageBox.Show("Refreshed.", "Refresh DB List");
    }

    private void NewButton_OnClick(object sender, RoutedEventArgs e)
    {
        var createDbWindow = new CreateDbWindow(DatabaseDao);
        if (createDbWindow.ShowDialog() == true)
        {
            RefreshDbList();
        }
    }

    private void ExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("export DB");
    }


    private void InfoButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var dbName = (string)button!.Tag;
        MessageBox.Show(dbName, "info");
    }

    private void SearchButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var dbName = (string)button!.Tag;
        var searchDbWindow = new SearchDbWindow(DatabaseDao, dbName);
        searchDbWindow.ShowDialog();
    }

    private void RenameButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var dbName = (string)button!.Tag;

        var renameDbWindow = new RenameDbWindow(dbName, DatabaseDao);
        if (renameDbWindow.ShowDialog() == true)
        {
            RefreshDbList();
        }
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var dbName = (string)button!.Tag;

        var result = MessageBox.Show($"Confirm delete {dbName}?", "Delete a database", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        DatabaseDao.Delete(dbName);

        MessageBox.Show($"{dbName} Deleted", "Delete a database");
        RefreshDbList();
    }

    private void EncryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        ArchiveUtils.RarPath = ConfigDao.Config.RarPath;

        var button = sender as Button;
        var dbName = (string)button!.Tag;

        var db = DatabaseDao.SelectByName(dbName);

        var updatedAt = db.UpdatedAt;

        var span = DateTime.Now - updatedAt;
        if (span.TotalDays < 1)
        {
            var messageBoxResult = MessageBox.Show("Less in 1 day after the last encryption, confirm do it again?",
                "Confirm", MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes) return;
        }

        var sourceFolder = db.SourceFolder;
        if (!Directory.Exists(sourceFolder))
        {
            MessageBox.Show($"Source folder does not exist: {sourceFolder}. This database may be broken", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var encryptedFolder = db.EncryptedFolder;
        if (!Directory.Exists(encryptedFolder))
        {
            MessageBox.Show(
                $"Encrypted folder does not exist: {encryptedFolder}. Modify this path in the detailed info page",
                "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var progressDialog = new ProgressDialog
        {
            Owner = Window.GetWindow(this)
        };

        progressDialog.Loaded += async (_, _) =>
        {
            await Task.Run(async () =>
            {
                await SciApi.EncryptData(db);
                db.UpdatedAt = DateTime.Today;
                DatabaseDao.Update(db);
            });

            progressDialog.Close();
            MessageBox.Show("Encryption Completed!", "Result");

            RefreshDbList();
        };

        progressDialog.ShowDialog();
    }
}