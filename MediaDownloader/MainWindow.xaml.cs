using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MediaDownloader.Common.Enum;
using MediaDownloader.Common.Module;
using MediaDownloader.Control;
using MediaDownloader.Page.Navigation;
using Wpf.Ui;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace MediaDownloader;

public partial class MainWindow
{
    private MenuItem _selectMenuItem;
    private Hint _hint;

    public MainWindow()
    {
        InitializeComponent();
        //_selectMenuItem = HomeMenuButton;
        //HomeMenuButton.Select();

        Loaded += (_, _) => SelectMenuNavigationView.Navigate(typeof(HomePage));

        ModBase.ShowHint += ShowHint;
        ModBase.GetCookies();
        ModBase.GetConfig();
    }

    private void ShowHint(string message, HintLevelEnum level = HintLevelEnum.Success)
    {
        var color = System.Drawing.Color.FromKnownColor((KnownColor)level);
        var hint = new Hint
        {
            IconText = level switch
            {
                HintLevelEnum.Success => "\ue609",
                HintLevelEnum.Error => "\ue603",
                HintLevelEnum.Warning => "\ue649",
                _ => "\ue609"
            },
            VerticalContentAlignment = VerticalAlignment.Center,
            Text = message,
            FontSize = 15,
            Padding = new Thickness(10),
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B)),
            Height = 60,
            Width = 300,
            IsHitTestVisible = false,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        FrameGrid.Children.Add(hint);

        _hint = hint;

        // wait 3 seconds and then disappear
        ModBase.RunInNewThread(() =>
        {
            Thread.Sleep(3000);
            Dispatcher.Invoke(() => FrameGrid.Children.Remove(_hint));
        });
    }
}