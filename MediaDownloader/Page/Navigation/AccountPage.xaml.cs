using System.Windows;
using MediaDownloader.Common.Enum;
using MediaDownloader.Page.Popup;

namespace MediaDownloader.Page.Navigation;

/// <summary>
/// AccountPage.xaml 的交互逻辑
/// </summary>
public partial class AccountPage
{
    public AccountPage()
    {
        InitializeComponent();
    }

    private void BilibiliLoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        LoginWindow loginWindow = new(PlatformEnum.Bilibili);
        loginWindow.ShowDialog();
    }
}