using System.Windows;
using SCICore.dao;
using SCICore.entity;
using System.Windows.Controls;
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
        throw new System.NotImplementedException();
    }
}