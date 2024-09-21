using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SCICore.api;
using SCICore.dao;
using SCICore.entity;
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

    private void RefreshDbList()
    {
        var databases = DatabaseDao.SelectAll();
        DbList.ItemsSource = databases;
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
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "All files (*.*)|*.*",
            Title = "Export File Location",
            FileName = "export.zip"
        };
        if (saveFileDialog.ShowDialog() != true) return;
        var path = saveFileDialog.FileName;

        var names = DbList.SelectedItems.Cast<Database>().Select(r => r.Name).ToList();
        var databases = DatabaseDao.SelectByNames(names);
        var json = JsonUtils.ToStr(databases);

        var sources = new List<(string fileContent, string path)> { (json, "db.json") };

        if (ConfigDao.Config.CreateDigestWhenExport)
        {
            var hashResult = Task.Run(() => HashUtils.ComputeStringHash(json)).Result;
            var digest = JsonUtils.ToStr(hashResult);
            sources.Add((digest, "digest.json"));
        }

        _ = Task.Run(() => ArchiveUtils.CompressZipInMem(path, sources));

        MessageBox.Show("Export succeed", "Export");
    }

    private void InfoButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dbName = ((Database)DbList.SelectedItem).Name;
        MessageBox.Show(dbName, "info");
    }

    private void SearchButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dbName = ((Database)DbList.SelectedItem).Name;
        var searchDbWindow = new SearchDbWindow(DatabaseDao, dbName);
        searchDbWindow.ShowDialog();
    }

    private void RenameButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dbName = ((Database)DbList.SelectedItem).Name;

        var renameDbWindow = new RenameDbWindow(dbName, DatabaseDao);
        if (renameDbWindow.ShowDialog() == true)
        {
            RefreshDbList();
        }
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dbName = ((Database)DbList.SelectedItem).Name;

        var result = MessageBox.Show($"Confirm delete {dbName}?", "Delete a database", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        DatabaseDao.Delete(dbName);

        MessageBox.Show($"{dbName} Deleted", "Delete a database");
        RefreshDbList();
    }

    private void EncryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        ArchiveUtils.RarPath = ConfigDao.Config.RarPath;

        var dbName = ((Database)DbList.SelectedItem).Name;

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