using System.Windows;
using MahApps.Metro.Controls;
using SCICore.dao;

namespace SCIDesktop.window;

public partial class RenameDbWindow : MetroWindow
{
    private DatabaseDao DatabaseDao { get; set; }

    public RenameDbWindow(string oldDbName, DatabaseDao databaseDao)
    {
        InitializeComponent();

        DatabaseDao = databaseDao;
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

        if (DatabaseDao.CheckNameExists(newDbName))
        {
            MessageBox.Show("This name already existed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var db = DatabaseDao.SelectByName(oldDbName);
        DatabaseDao.Delete(oldDbName);
        db.Name = newDbName;
        DatabaseDao.Insert(db);

        MessageBox.Show("succeed");
        DialogResult = true;
        Close();
    }
}