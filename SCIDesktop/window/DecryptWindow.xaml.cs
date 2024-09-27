using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Ookii.Dialogs.Wpf;
using SCICore.api;
using SCICore.dao;
using SCICore.entity;
using SCICore.util;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;

namespace SCIDesktop.window;

public partial class DecryptWindow : MetroWindow
{
    private DatabaseDao DatabaseDao { get; set; }

    private ConfigDao ConfigDao { get; set; }

    public DecryptWindow(DatabaseDao databaseDao, string dbName, ConfigDao configDao)
    {
        InitializeComponent();
        DatabaseDao = databaseDao;
        ConfigDao = configDao;

        var databases = DatabaseDao.SelectAll();
        ChooseDbComboBox.ItemsSource = databases.Select(v => v.Name).ToList();
        ChooseDbComboBox.SelectedItem = dbName;

        DecryptPathTextBox.Text = configDao.Config.PreferredDecryptedPath;


        DecResultList.ItemsSource =
            new List<DecryptResult>(new[]
            {
                new DecryptResult { Status = DecryptResult.SuccessStatus, EncFilePath = "abc", DecFilePath = "def" },
                new DecryptResult { Status = DecryptResult.FailedStatus, EncFilePath = "abc", Message = "not found" },
            });
    }

    private void ChooseFileButton_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "RAR Archive (*.rar)|*.rar",
            Title = "Select the filepath"
        };
        if (openFileDialog.ShowDialog() != true) return;
        EncFilePathTextBox.Text = openFileDialog.FileName;
    }

    private void ChooseFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        var textBoxName = (sender as Button)!.Tag as string;

        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Select a Folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != true) return;
        (FindName(textBoxName!) as TextBox)!.Text = dialog.SelectedPath;
    }

    public class DecryptResult
    {
        public const string SuccessStatus = "\u2714\ufe0f"; // ✔️

        public const string FailedStatus = "\u274c"; // ❌

        public string Status { get; set; }

        public string EncFilePath { get; set; }

        public string DecFilePath { get; set; }

        public const string NoSuchFileMessage = "File does not exist in this database.";

        public const string NoPwdMessage = "No matching password.";

        public string Message { get; set; }
    }

    private void DecryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        ArchiveUtils.RarPath = ConfigDao.Config.RarPath;

        if (!File.Exists(ArchiveUtils.RarPath))
        {
            MessageBox.Show("Set RAR path in the settings.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var encPath = EncFilePathTextBox.Text;

        var isEncFile = File.Exists(encPath);
        var isEncDir = Directory.Exists(encPath);

        if (!(isEncFile || isEncDir))
        {
            MessageBox.Show("Choose a Encrypted File/Folder", "Decrypt",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var decPath = DecryptPathTextBox.Text;
        if (!Directory.Exists(decPath))
        {
            MessageBox.Show("Choose a Decrypt Folder", "Decrypt",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var progressDialog = new ProgressDialog
        {
            Owner = this
        };

        // access the controls outside Task.Run below, otherwise the new thread is trying to access a WPF object
        // (UI element) from a thread other than the one on which the element was created (i.e. the UI thread).
        // told by GPT
        var dbName = (string)ChooseDbComboBox.SelectedItem;
        var decryptResults = new List<DecryptResult>();
        var overwriteMode = OverwriteModeCheckBox.IsChecked == true;

        progressDialog.Loaded += async (_, _) =>
        {
            await Task.Run(async () =>
            {
                var db = DatabaseDao.SelectByName(dbName);
                var archives = isEncFile ? new[] { encPath } : Directory.GetFiles(encPath);

                foreach (var a in archives)
                {
                    if (!a.EndsWith(".rar"))
                        continue;

                    var extractSuccess = false;
                    var decFilePath = "";
                    if (db.EncryptScheme.PwdLevel == PasswordLevel.Db) // if the enc level is per db, just decrypt it
                    {
                        var pwd = await EncryptApi.MakeArchivePwd(db, null);
                        var exitCode = await ArchiveUtils.ExtractRar(a, decPath, pwd, overwriteMode);
                        // 10 means file with the same name already exists
                        extractSuccess = exitCode == 0 || exitCode == 10;
                        if (extractSuccess)
                        {
                            decFilePath = (await ArchiveUtils.ListFilesInRar(a, pwd))[0];
                        }
                    }
                    else // search the file in db.node and then decrypt it
                    {
                        var hashResult = await HashUtils.ComputeFileHash(a);

                        var aLength = new FileInfo(a).Length;
                        const double sizeDelta = 0.01;

                        var searchResults = db.Node == null!
                            ? null
                            : db.Node.Search(new Node.SearchCondition
                            {
                                Keywords = new[] { FsUtils.GetLastEntry(a) },
                                ArchiveHashResult = hashResult,
                                UseArchiveSize = true,
                                ArchiveSizeLowerBound = (long)(aLength * (1 - sizeDelta)),
                                ArchiveSizeUpperBound = (long)(aLength * (1 + sizeDelta)),
                            });

                        if (searchResults == null! || searchResults.Count == 0)
                        {
                            decryptResults.Add(new DecryptResult
                            {
                                Status = DecryptResult.FailedStatus,
                                EncFilePath = FsUtils.GetLastEntry(a),
                                Message = DecryptResult.NoSuchFileMessage
                            });
                            continue;
                        }

                        foreach (var (node, _, _) in searchResults)
                        {
                            var pwd = await EncryptApi.MakeArchivePwd(db, node.FileName);
                            var pwdCorrect = await ArchiveUtils.TestRarPassword(a, pwd) == 0;
                            if (pwdCorrect)
                            {
                                decFilePath = FsUtils.GetLastEntry(node.FileName);
                                var exitCode = await ArchiveUtils.ExtractRar(a, decPath, pwd, overwriteMode);
                                extractSuccess = exitCode == 0 || exitCode == 10;
                                break;
                            }
                        }
                    }

                    if (extractSuccess)
                    {
                        decryptResults.Add(new DecryptResult
                        {
                            Status = DecryptResult.SuccessStatus,
                            EncFilePath = FsUtils.GetLastEntry(a),
                            DecFilePath = decFilePath
                        });
                    }
                    else
                    {
                        decryptResults.Add(new DecryptResult
                        {
                            Status = DecryptResult.FailedStatus,
                            EncFilePath = FsUtils.GetLastEntry(a),
                            Message = DecryptResult.NoPwdMessage
                        });
                    }
                }
            });

            progressDialog.Close();
            MessageBox.Show("Decryption Completed!\nView the decryption result by clicking the button.",
                "Decrypt");
        };

        progressDialog.ShowDialog();

        DecResultList.ItemsSource = decryptResults;
        ToggleDecResultButton.Visibility = Visibility.Visible;
    }

    private void ToggleDecResultButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DecResultList.Visibility == Visibility.Visible)
        {
            Width = 450;
            DecResultList.Visibility = Visibility.Collapsed;
        }
        else if (DecResultList.Visibility == Visibility.Collapsed)
        {
            DecResultList.Visibility = Visibility.Visible;
            Width = 1200;
        }
    }
}