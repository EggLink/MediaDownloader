using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaDownloader.Control;

public class MenuItem : System.Windows.Controls.Control
{
    // Text Property
    public string Text
    {
        get => (string) GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
 
    public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(MenuItem), new PropertyMetadata(""));

    public Brush ClickBackground
    {
        get => (Brush) GetValue(ClickBackgroundProperty);
        set => SetValue(ClickBackgroundProperty, value);
    }
 
    public static readonly DependencyProperty ClickBackgroundProperty =
            DependencyProperty.Register(nameof(ClickBackground), typeof(Brush), typeof(MenuItem), new PropertyMetadata());

    public Brush DefaultBackground
    {
        get => (Brush) GetValue(DefaultBackgroundProperty);
        set => SetValue(DefaultBackgroundProperty, value);
    }
 
    public static readonly DependencyProperty DefaultBackgroundProperty =
            DependencyProperty.Register(nameof(DefaultBackground), typeof(Brush), typeof(MenuItem), new PropertyMetadata(Brushes.Transparent));

    public delegate void OnClickDelegate(object sender, MouseButtonEventArgs e);

    public event OnClickDelegate OnClick;
    public bool IsClick;
    public bool IsSelect;

    public MenuItem()
    {
        // initialize
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        MouseLeave += OnMouseLeave;
        MouseEnter += OnMouseEnter;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (!IsSelect)
            Background = ClickBackground.Clone();
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        IsClick = false;

        if (!IsSelect)
            Background = DefaultBackground.Clone();  // clear background
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsClick) return;
        OnClick?.Invoke(this, e);

        if (!IsSelect)
            Background = ClickBackground.Clone();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        IsClick = true;
        if (!IsSelect)
            Background = DefaultBackground.Clone();  // clear background
    }

    public void Select()
    {
        if (!IsSelect)
        {
            IsSelect = true;
            Background = ClickBackground.Clone();
        }
        else
        {
            IsSelect = false;
            Background = DefaultBackground.Clone();
        }
    }

    static MenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuItem),
            new FrameworkPropertyMetadata(typeof(MenuItem)));
    }
}
