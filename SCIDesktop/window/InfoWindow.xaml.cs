using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;

namespace SCIDesktop.window;

/// <summary>
/// For displaying (customized) POCO
/// </summary>
public partial class InfoWindow : MetroWindow
{
    public class InfoRow
    {
        public object Key { get; set; }
        public object Value { get; set; }

        public InfoRow(object key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    public InfoWindow(List<InfoRow> infoRows, string title = "")
    {
        InitializeComponent();

        TitleTextBlock.Text = title;
        InfoList.ItemsSource = infoRows;
    }

    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(((InfoRow)InfoList.SelectedItem).Value.ToString()!);

        NotificationTextBlock.Visibility = Visibility.Visible;
        Task.Delay(3000).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() => { NotificationTextBlock.Visibility = Visibility.Collapsed; });
        });
    }
}