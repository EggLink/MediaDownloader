using System.Windows;

namespace MediaDownloader.Control;

public class Card : System.Windows.Controls.Control
{
    // Text Property
    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(Card), new PropertyMetadata(""));

    public string HintText
    {
        get => (string)GetValue(HintTextProperty);
        set => SetValue(HintTextProperty, value);
    }

    public static readonly DependencyProperty HintTextProperty =
        DependencyProperty.Register(nameof(HintText), typeof(string), typeof(Card), new PropertyMetadata(""));

    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly DependencyProperty IconTextProperty =
        DependencyProperty.Register(nameof(IconText), typeof(string), typeof(Card), new PropertyMetadata(""));

    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Card),
            new FrameworkPropertyMetadata(typeof(Card)));
    }
}