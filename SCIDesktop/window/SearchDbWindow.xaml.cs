using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MahApps.Metro.Controls;
using SCICore.dao;
using SCICore.entity;
using SCICore.util;

namespace SCIDesktop.window;

public partial class SearchDbWindow : MetroWindow
{
    public string SelectedDb { get; set; }

    public string UserInput { get; set; } = "";


    public bool AndCondition { get; set; } = true;

    public bool FilesizeChecked { get; set; } = false;

    public double FileSizeLowerBound { get; set; } = 0;

    public ByteUnit FileSizeLowerBoundUnit { get; set; } = ByteUnit.MB;

    public double FileSizeUpperBound { get; set; } = 10;

    public ByteUnit FileSizeUpperBoundUnit { get; set; } = ByteUnit.MB;

    private DatabaseDao DatabaseDao { get; set; }

    private const string EmptyDbMsg = "This is an empty database, run encryption first.";

    public SearchDbWindow(DatabaseDao databaseDao, string dbName)
    {
        InitializeComponent();
        DataContext = this;

        DatabaseDao = databaseDao;

        var units = Enum.GetValues(typeof(ByteUnit));
        var comboBoxes = new[] { FileSizeLowerBoundUnitComboBox, FileSizeUpperBoundUnitComboBox };
        foreach (var cb in comboBoxes)
        {
            cb.ItemsSource = units;
        }

        FileLowerBoundValidationRule.Window = this;
        FileUpperBoundValidationRule.Window = this;

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
        FileSizeLowerBoundTextBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
        FileSizeUpperBoundTextBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();

        return !Validation.GetHasError(KeyWordsTextBox) &&
               !Validation.GetHasError(FileSizeLowerBoundTextBox) &&
               !Validation.GetHasError(FileSizeUpperBoundTextBox);
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
                And = AndCondition,
                Keywords = keywords,
                UseFileSize = FilesizeChecked,
                FileSizeLowerBound = FsUtils.ToBytes(FileSizeLowerBound, FileSizeLowerBoundUnit),
                FileSizeUpperBound = FsUtils.ToBytes(FileSizeUpperBound, FileSizeUpperBoundUnit)
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

    private void SearchResultList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SearchResultList.SelectedItem == null) return;
        var result = (SearchResult)SearchResultList.SelectedItem;

        var infoRows = new List<InfoWindow.InfoRow>
        {
            new("Filename", result.Filename),
            new("Source Path", result.SourcePath),
            new("Encrypted Path", result.EncryptedPath),
            new("File Size", FsUtils.PrettyPrintBytes(result.Node.FileSize)),
            new("Archive Size", FsUtils.PrettyPrintBytes(result.Node.ArchiveSize))
        };

        var infoWindow = new InfoWindow(infoRows, "Detailed File Info");
        infoWindow.ShowDialog();
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

public class LowerBoundValidationRule : ValidationRule
{
    public string LowerBoundUnitProperty { get; set; }

    public string UpperBoundProperty { get; set; }

    public string UpperBoundUnitProperty { get; set; }

    public string CheckBoxProperty { get; set; }

    public SearchDbWindow Window { get; set; }

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if ((bool)typeof(SearchDbWindow).GetProperty(CheckBoxProperty)!.GetValue(Window)! != true)
        {
            return ValidationResult.ValidResult;
        }

        double lowerBound;

        if (double.TryParse((string)value!, NumberStyles.Float, CultureInfo.InvariantCulture, out lowerBound))
        {
            if (lowerBound < 0)
                return new ValidationResult(false, "lowerbound must be a number greater than or equal to 0");
        }
        else
        {
            return new ValidationResult(false, "lowerbound must be a number.");
        }

        var upperBound = (double)typeof(SearchDbWindow).GetProperty(UpperBoundProperty)!.GetValue(Window)!;

        var lowerBoundUnit =
            (ByteUnit)typeof(SearchDbWindow).GetProperty(LowerBoundUnitProperty)!.GetValue(Window)!;
        var upperBoundUnit =
            (ByteUnit)typeof(SearchDbWindow).GetProperty(UpperBoundUnitProperty)!.GetValue(Window)!;

        if (FsUtils.ToBytes(lowerBound, lowerBoundUnit) > FsUtils.ToBytes(upperBound, upperBoundUnit))
        {
            return new ValidationResult(false, "lowerbound must be less than or equal to upperbound");
        }

        return ValidationResult.ValidResult;
    }
}

public class UpperBoundValidationRule : ValidationRule
{
    public string LowerBoundProperty { get; set; }

    public string LowerBoundUnitProperty { get; set; }

    public string UpperBoundUnitProperty { get; set; }

    public string CheckBoxProperty { get; set; }

    public SearchDbWindow Window { get; set; }

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if ((bool)typeof(SearchDbWindow).GetProperty(CheckBoxProperty)!.GetValue(Window)! != true)
        {
            return ValidationResult.ValidResult;
        }

        double upperBound;

        if (double.TryParse((string)value!, NumberStyles.Float, CultureInfo.InvariantCulture, out upperBound))
        {
            if (upperBound < 0)
                return new ValidationResult(false, "upperBound must be a number greater than or equal to 0");
        }
        else
        {
            return new ValidationResult(false, "upperBound must be a number.");
        }

        var lowerBound = (double)typeof(SearchDbWindow).GetProperty(LowerBoundProperty)!.GetValue(Window)!;

        var lowerBoundUnit =
            (ByteUnit)typeof(SearchDbWindow).GetProperty(LowerBoundUnitProperty)!.GetValue(Window)!;
        var upperBoundUnit =
            (ByteUnit)typeof(SearchDbWindow).GetProperty(UpperBoundUnitProperty)!.GetValue(Window)!;

        if (FsUtils.ToBytes(lowerBound, lowerBoundUnit) > FsUtils.ToBytes(upperBound, upperBoundUnit))
        {
            return new ValidationResult(false, "upperBound must be greter than or equal to lowerbound");
        }

        return ValidationResult.ValidResult;
    }
}