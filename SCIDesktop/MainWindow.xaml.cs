using System.Windows;
using SCICore.dao;
using SCIDesktop.control;

namespace SCIDesktop;

public partial class MainWindow : Window
{
    private ConfigDao ConfigDao { get; set; }

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(ConfigDao configDao)
    {
        InitializeComponent();
        ConfigDao = configDao;

        EncTab.Content = new EncryptionControl(ConfigDao);
        DecTab.Content = new DecryptionControl();
        SetTab.Content = new SettingControl(ConfigDao);
        AboutTab.Content = new AboutControl();
    }
}