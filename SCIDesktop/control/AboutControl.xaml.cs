using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace SCIDesktop.control;

public partial class AboutControl : UserControl
{
    public string Version { get; set; }

    public AboutControl()
    {
        InitializeComponent();
        DataContext = this;
        Version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "1.0.0";
    }

    private void CheckUpdateButton_OnClick(object sender, RoutedEventArgs e)
    {
        // TODO
        MessageBox.Show("check updates");
    }
}