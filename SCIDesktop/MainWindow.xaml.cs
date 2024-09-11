using System;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using SCICore.dao;
using SCIDesktop.control;

namespace SCIDesktop;

public partial class MainWindow : MetroWindow
{
    private ConfigDao ConfigDao { get; set; }

    public MainWindow()
    {
        InitializeComponent();
    }

    public EncryptionControl EncryptionControl { get; set; }
    public DecryptionControl DecryptionControl { get; set; }
    public SettingControl SettingControl { get; set; }
    public AboutControl AboutControl { get; set; }

    public MainWindow(ConfigDao configDao)
    {
        InitializeComponent();
        ConfigDao = configDao;

        EncryptionControl = new EncryptionControl(ConfigDao);
        DecryptionControl = new DecryptionControl();
        SettingControl = new SettingControl(ConfigDao);
        AboutControl = new AboutControl();

        ContentControl.Content = EncryptionControl;
    }

    private void MainHamburgerMenu_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
    {
        if (args.InvokedItem is HamburgerMenuIconItem menuItem)
        {
            var tag = (string)menuItem.Tag;
            ContentControl.Content = typeof(MainWindow).GetProperty(tag)!.GetValue(this);
        }
    }
}