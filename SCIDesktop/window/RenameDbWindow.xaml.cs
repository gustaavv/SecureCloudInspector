using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using SCICore.dao;
using SCICore.entity;

namespace SCIDesktop.window;

public partial class RenameDbWindow : MetroWindow
{
    private DatabaseIndexDao IndexDao { get; set; }

    public RenameDbWindow(string oldDbName, DatabaseIndexDao indexDao)
    {
        InitializeComponent();

        IndexDao = indexDao;
        FromTextBox.Text = oldDbName;
    }

    private void SubmitButton_OnClick(object sender, RoutedEventArgs e)
    {
        var oldDbName = FromTextBox.Text;
        var newDbName = ToTextBox.Text;

        if (string.IsNullOrWhiteSpace(newDbName))
        {
            MessageBox.Show("Empty database name is not valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (oldDbName == newDbName)
        {
            MessageBox.Show("No need to rename.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (IndexDao.GetIndex().ContainsKey(newDbName))
        {
            MessageBox.Show("This name already existed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (DatabaseIndexDao.DbIndexFileName == $"{newDbName}.json")
        {
            MessageBox.Show("This name is not allowed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var oldDbFile = IndexDao.GetIndex()[oldDbName].Filepath;
        var newDbFile = Path.Join(Path.GetDirectoryName(oldDbFile), $"{newDbName}.json");

        File.Move(oldDbFile, newDbFile);
        _ = Task.Run(() => new DbDao(newDbFile)); // validate db file move

        var oldRecord = IndexDao.GetIndex()[oldDbName];
        IndexDao.GetIndex().Remove(oldDbName);

        var newRecord = new DatabaseRecord();
        IndexDao.GetIndex()[newDbName] = newRecord;
        newRecord.Filepath = newDbFile;
        newRecord.CreatedAt = oldRecord.CreatedAt;
        newRecord.UpdatedAt = DateTime.Today;


        _ = Task.Run(IndexDao.WriteDbIndex);

        MessageBox.Show("succeed");
        DialogResult = true;
        Close();
    }
}