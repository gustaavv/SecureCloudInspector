﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using SCICore.dao;
using SCICore.entity;

namespace SCIDesktop.window;

public partial class SearchDbWindow : Window
{
    private DatabaseIndexDao IndexDao { get; set; }

    public SearchDbWindow(DatabaseIndexDao indexDao, string dbName)
    {
        InitializeComponent();

        IndexDao = indexDao;

        var valueTuples = Task.Run(IndexDao.ListDatabases).Result;
        ChooseDbComboBox.ItemsSource = valueTuples.Select(v => v.name).ToList();
        ChooseDbComboBox.SelectedItem = dbName;

        var dbDao = Task.Run(() => new DbDao(IndexDao.GetIndex()[dbName].Filepath)).Result;
        if (dbDao.Db.Node == null!)
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
        var dbDao = Task.Run(() => new DbDao(IndexDao.GetIndex()[dbName].Filepath)).Result;

        var keywords = input.Split(' ')
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.ToLower())
            .ToArray();

        if (dbDao.Db.Node == null!) return;

        var results = dbDao.Db.Node.Search(keywords)
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