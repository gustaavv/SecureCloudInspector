using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro.Controls;

namespace SCIDesktop.window;

public partial class ExceptionWindow : MetroWindow
{
    public ExceptionWindow(Exception ex)
    {
        InitializeComponent();
        ExInfoTextBox.Text += "Exception Message: \n" + ex.Message + "\n";
        ExInfoTextBox.Text += "Stack Trace: \n" + ex.StackTrace + "\n";
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(ExInfoTextBox.Text);

        NotificationTextBlock.Visibility = Visibility.Visible;
        Task.Delay(3000).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() => { NotificationTextBlock.Visibility = Visibility.Collapsed; });
        });
    }
}