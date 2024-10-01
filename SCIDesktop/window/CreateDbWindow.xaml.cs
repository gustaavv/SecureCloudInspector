using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Ookii.Dialogs.Wpf;
using SCICore.api;
using SCICore.dao;
using SCICore.entity;

namespace SCIDesktop.window;

public partial class CreateDbWindow : MetroWindow
{
    public string DbName { get; set; } = "";

    public string SourceFolder { get; set; } = "";

    public string EncryptedFolder { get; set; } = "";

    public PasswordLevel SelectedPasswordLevel { get; set; } = PasswordLevel.Db;

    public DbType SelectedDbType { get; set; } = DbType.Normal;

    public uint EncryptedFilenameLength { get; set; } = 15;

    public uint EncryptedArchivePwdLength { get; set; } = 60;

    private DatabaseDao DatabaseDao { get; set; }

    private ConfigDao ConfigDao { get; set; }

    public CreateDbWindow(DatabaseDao databaseDao, ConfigDao configDao)
    {
        InitializeComponent();
        DatabaseDao = databaseDao;
        ConfigDao = configDao;

        DataContext = this;

        PwdLevelComboBox.ItemsSource = Enum.GetValues(typeof(PasswordLevel));
        DbTypeComboBox.ItemsSource = Enum.GetValues(typeof(DbType));

        DbNameValidationRule.ExistingDbNames = databaseDao.SelectNames().ToHashSet();
        SrcFolderValidationRule.Window = this;
        EncFolderValidationRule.Window = this;
    }

    private void ChooseFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var textBoxName = (string)button!.Tag + "TextBox";

        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Select a Folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != true) return;
        ((TextBox)FindName(textBoxName)!).Text = dialog.SelectedPath;
    }

    private bool CanSubmit()
    {
        DbNameTextBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
        SourceFolderTextBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
        EncryptedFolderTextBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();

        return !Validation.GetHasError(DbNameTextBox) &&
               !Validation.GetHasError(SourceFolderTextBox) &&
               !Validation.GetHasError(EncryptedFolderTextBox);
    }

    private void SubmitButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!CanSubmit())
        {
            MessageBox.Show("Some fields are not valid.", "Create Database",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // There seems no built-in ways to validate PasswordBox
        var pwd = PasswordBox.Password;
        if (string.IsNullOrWhiteSpace(pwd))
        {
            MessageBox.Show("Empty password is not valid.", "Create Database",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var db = new Database(DbName, SourceFolder, EncryptedFolder, null!, new EncryptScheme(), pwd, SelectedDbType,
            DateTime.Today, DateTime.Today);

        db.EncryptScheme.PwdLevel = SelectedPasswordLevel;
        db.EncryptScheme.FileNamePattern = EncryptApi.MakePattern((int)EncryptedFilenameLength);
        db.EncryptScheme.PwdPattern = EncryptApi.MakePattern((int)EncryptedArchivePwdLength);

        DatabaseDao.Insert(db);

        MessageBox.Show("succeed");
        DialogResult = true;
        Close();
    }
}

public class DbNameValidationRule : ValidationRule
{
    public HashSet<string> ExistingDbNames { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var dbName = (string)value;

        if (string.IsNullOrWhiteSpace(dbName))
            return new ValidationResult(false, "Empty database name is not valid.");

        if (dbName.StartsWith(' ') || dbName.EndsWith(' '))
            return new ValidationResult(false, "Database name can not start or end with space.");

        if (ExistingDbNames.Contains(dbName))
            return new ValidationResult(false, "This name has already existed.");

        return ValidationResult.ValidResult;
    }
}

public class SrcEncFolderValidationRule : ValidationRule
{
    public CreateDbWindow Window { get; set; }

    public string OtherFolderProperty { get; set; }
    public bool IsSourceFolder { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var folderPath = (string)value;
        var otherFolderPath = (string)typeof(CreateDbWindow).GetProperty(OtherFolderProperty)!.GetValue(Window)!;

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return new ValidationResult(false, "Path cannot be empty.");
        }

        if (!Directory.Exists(folderPath))
        {
            return new ValidationResult(false, "Folder doesn't exist.");
        }

        if (!string.IsNullOrWhiteSpace(otherFolderPath) && folderPath.Contains(otherFolderPath))
        {
            return new ValidationResult(false,
                IsSourceFolder
                    ? "Source folder should not be a child folder of the encrypted folder."
                    : "Encrypted folder should not be a child folder of the source folder.");
        }

        return ValidationResult.ValidResult;
    }
}