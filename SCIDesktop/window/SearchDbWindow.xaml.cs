using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MahApps.Metro.Controls;
using SCICore.dao;
using SCICore.entity;

namespace SCIDesktop.window;

public partial class SearchDbWindow : MetroWindow
{
    private DatabaseDao DatabaseDao { get; set; }

    public SearchDbWindow(DatabaseDao databaseDao, string dbName)
    {
        InitializeComponent();

        DatabaseDao = databaseDao;

        var databases = DatabaseDao.SelectAll();
        ChooseDbComboBox.ItemsSource = databases.Select(v => v.Name).ToList();
        ChooseDbComboBox.SelectedItem = dbName;

        var db = DatabaseDao.SelectByName(dbName);
        if (db.Node == null!)
        {
            EmptyDbTextBlock.Text = "This is an empty database, run encryption first.";
        }

        Loaded += (_, _) => KeyWordsTextBox.Focus();
    }

    private void KeyWordsTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            SearchButton_OnClick(SearchButton, new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }

    public class SearchResult
    {
        public string Filename { get; set; }
        public string SourcePath { get; set; }
        public string EncryptedPath { get; set; }

        public Node Node { get; set; }

        public SearchResult(string filename, string sourcePath, string encryptedPath, Node node)
        {
            Filename = filename;
            SourcePath = sourcePath;
            EncryptedPath = encryptedPath;
            Node = node;
        }
    }

    private void SearchButton_OnClick(object sender, RoutedEventArgs e)
    {
        var input = KeyWordsTextBox.Text;

        if (string.IsNullOrWhiteSpace(input))
        {
            MessageBox.Show("Input some keywords.", "Result", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dbName = (string)ChooseDbComboBox.SelectedItem;
        var db = DatabaseDao.SelectByName(dbName);

        var keywords = input.Split(' ')
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.ToLower())
            .ToArray();

        if (db.Node == null!) return;

        var results = db.Node.Search(keywords)
            .Select(t => new SearchResult(t.node.FileName, t.srcPath, t.encPath, t.node))
            .ToList();
        SearchResultList.ItemsSource = results;
    }

    private void InfoButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var node = (Node)button!.Tag;
    }
}