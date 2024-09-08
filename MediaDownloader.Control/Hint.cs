using System;
using System.Windows;

namespace MediaDownloader.Control;

public class Hint : System.Windows.Controls.Control
{
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(Hint), new PropertyMetadata(""));

    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly DependencyProperty IconTextProperty =
        DependencyProperty.Register(nameof(IconText), typeof(string), typeof(Hint), new PropertyMetadata(""));

    static Hint()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Hint),
            new FrameworkPropertyMetadata(typeof(Hint)));
    }
}