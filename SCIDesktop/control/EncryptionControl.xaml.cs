using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SCICore.dao;
using SCICore.entity;
using SCIDesktop.window;

namespace SCIDesktop.control;

public partial class EncryptionControl : UserControl
{
    public ConfigDao ConfigDao { get; set; }
    public DatabaseIndexDao IndexDao { get; set; }

    public EncryptionControl(ConfigDao configDao)
    {
        InitializeComponent();
        ConfigDao = configDao;
        IndexDao = Task.Run(() => new DatabaseIndexDao(ConfigDao.GetDbFolder())).Result;
        RefreshDbList();
    }

    public class DbInfoRow
    {
        public string Name { get; set; }
        public string SourceFolder { get; set; }
        public string EncryptedFolder { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public DbInfoRow(string name, string sourceFolder, string encryptedFolder,
            DateTime? createdAt, DateTime? updatedAt)
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
        var valueTuples = Task.Run(IndexDao.ListDatabases).Result;
        var list = new List<DbInfoRow>();

        foreach (var t in valueTuples)
        {
            var dbInfoRow = new DbInfoRow(t.name, t.db.SourceFolder, t.db.EncryptedFolder,
                t.record.CreatedAt, t.record.UpdatedAt);
            list.Add(dbInfoRow);
        }

        DbList.ItemsSource = list;
    }


    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshDbList();
        MessageBox.Show("Refreshed.", "Refresh DB List");
    }

    private void NewButton_OnClick(object sender, RoutedEventArgs e)
    {
        var createDbWindow = new CreateDbWindow(IndexDao);
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
        var dbInfo = (string)button!.Tag;
        MessageBox.Show(dbInfo, "info");
    }

    private void SearchButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var dbInfo = (string)button!.Tag;
        MessageBox.Show(dbInfo, "search");
    }

    private void RenameButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var dbInfo = (string)button!.Tag;
        MessageBox.Show(dbInfo, "rename");
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var dbInfo = (string)button!.Tag;
        MessageBox.Show(dbInfo, "delete");
    }
}