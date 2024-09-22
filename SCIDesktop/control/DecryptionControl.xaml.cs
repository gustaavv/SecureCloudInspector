using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using SCICore.dao;
using SCICore.entity;
using System.Windows.Controls;
using Microsoft.Win32;
using SCICore.util;
using SCIDesktop.window;

namespace SCIDesktop.control;

public partial class DecryptionControl : UserControl
{
    private ConfigDao ConfigDao { get; set; }
    private DatabaseDao DatabaseDao { get; set; }

    public DecryptionControl(ConfigDao configDao)
    {
        InitializeComponent();

        ConfigDao = configDao;
        DatabaseDao = new DatabaseDao(configDao.Config.DecDbPath);
        RefreshDbList();
    }

    private void RefreshDbList()
    {
        var databases = DatabaseDao.SelectAll();
        DbList.ItemsSource = databases;
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

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dbName = ((Database)DbList.SelectedItem).Name;

        var result = MessageBox.Show($"Confirm delete {dbName}?", "Delete a database", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        DatabaseDao.Delete(dbName);

        MessageBox.Show($"{dbName} Deleted", "Delete a database");
        RefreshDbList();
    }

    private void DecryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void ImportButton_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "ZIP Archive (*.zip)|*.zip",
            Title = "Import File Location"
        };
        if (openFileDialog.ShowDialog() != true) return;
        var path = openFileDialog.FileName;

        var list = ArchiveUtils.ExtractZipInMem(path);
        var json = list.Find(t => t.path == "db.json").fileContent;
        if (json == null!)
        {
            MessageBox.Show("No db.json is found in this zip archive.", "Import",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var databases = new List<Database>();
        try
        {
            databases = JsonUtils.ToObj<List<Database>>(json)!;
        }
        catch (Exception ex)
        {
            MessageBox.Show("invalid db.json", "Import", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }


        if (ConfigDao.Config.VerifyDigestWhenImport)
        {
            // if digest is not created when exporting, it won't be verified
            var digest = list.Find(t => t.path == "digest.json").fileContent;
            if (digest != null!)
            {
                var digestHashResult = new HashResult();
                try
                {
                    digestHashResult = JsonUtils.ToObj<HashResult>(digest);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("invalid digest.json", "Import", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var calculatedHashResult = Task.Run(() => HashUtils.ComputeStringHash(json)).Result;
                if (digestHashResult != calculatedHashResult)
                {
                    MessageBox.Show("Digest hash calculation failed. The db is broken.", "Import",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        foreach (var db in databases)
        {
            if (DatabaseDao.CheckNameExists(db.Name))
            {
                var oldDb = DatabaseDao.SelectByName(db.Name);
                if (oldDb.UpdatedAt > db.UpdatedAt)
                {
                    var text = $"Warning: for database '{db.Name}':\n" +
                               $"The original db is updated at {oldDb.UpdatedAt}.\n" +
                               $"The imported db is updated at {db.UpdatedAt}.\n" +
                               $"It means that the imported '{db.Name}' might be an older version.\n" +
                               $"Confirm import this database '{db.Name}'?";
                    var messageBoxResult = MessageBox.Show(text,
                        "Confirm", MessageBoxButton.YesNo);
                    if (messageBoxResult != MessageBoxResult.Yes) continue;
                }

                DatabaseDao.Update(db);
            }
            else
            {
                DatabaseDao.Insert(db);
            }
        }

        MessageBox.Show("Import succeed", "Import");
        RefreshDbList();
    }
}