using System.Globalization;
using System.Linq;
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
    public string SelectedDb { get; set; }

    public string? UserInput { get; set; }

    private DatabaseDao DatabaseDao { get; set; }

    private const string EmptyDbMsg = "This is an empty database, run encryption first.";

    public SearchDbWindow(DatabaseDao databaseDao, string dbName)
    {
        InitializeComponent();
        DataContext = this;

        DatabaseDao = databaseDao;

        ChooseDbComboBox.ItemsSource = DatabaseDao.SelectNames();
        SelectedDb = dbName;

        var db = DatabaseDao.SelectByName(dbName);
        if (db.Node == null!)
        {
            EmptyDbTextBlock.Text = EmptyDbMsg;
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

    private bool CanSearch()
    {
        KeyWordsTextBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
        return !Validation.GetHasError(KeyWordsTextBox);
    }

    private void SearchButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!CanSearch())
        {
            MessageBox.Show("Please enter a valid search condition.", "Search",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var db = DatabaseDao.SelectByName(SelectedDb);
        var keywords = UserInput.Split(' ')
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.ToLower())
            .ToArray();

        if (db.Node == null!)
        {
            EmptyDbTextBlock.Text = EmptyDbMsg;
            return;
        }

        var results = db.Node.Search(new Node.SearchCondition
            {
                Keywords = keywords
            })
            .Select(t => new SearchResult(t.node.FileName, t.srcPath, t.encPath, t.node))
            .ToList();

        SearchResultList.ItemsSource = results;
    }

    private void ChooseDbComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EmptyDbTextBlock.Text = "";
        SearchResultList.ItemsSource = null;
    }
}

public class UserInputValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (string.IsNullOrWhiteSpace((string)value))
            return new ValidationResult(false, "Keywords cannot be empty");

        return ValidationResult.ValidResult;
    }
}