using moddingSuite.View.SpinningWheel.Enums;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace moddingSuite.View.SpinningWheel;

[TemplatePart(Name = "PART_Container", Type = typeof(Canvas))]
public class SpinningWheel : Control
{
    private Canvas container = null;
    private readonly Storyboard storyBoard = new();
    private readonly DoubleAnimation rotateAnimation = new(0, 360, new Duration(TimeSpan.FromSeconds(1)));

    static SpinningWheel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SpinningWheel), new FrameworkPropertyMetadata(typeof(SpinningWheel)));
    }

    public int DotCount
    {
        get => (int)GetValue(DotCountProperty);
        set => SetValue(DotCountProperty, value);
    }

    public static readonly DependencyProperty DotCountProperty =
      DependencyProperty.Register("DotCount", typeof(int), typeof(SpinningWheel), new FrameworkPropertyMetadata(12, FrameworkPropertyMetadataOptions.AffectsMeasure, OnDotCountChanged));

    public Brush DotColor
    {
        get => (Brush)GetValue(DotColorProperty);
        set => SetValue(DotColorProperty, value);
    }

    public static readonly DependencyProperty DotColorProperty =
      DependencyProperty.Register("DotColor", typeof(Brush), typeof(SpinningWheel), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 0, 122, 204)), FrameworkPropertyMetadataOptions.AffectsRender)); // windows 7 blue

    public double DotRadius
    {
        get => (double)GetValue(DotRadiusProperty);
        set => SetValue(DotRadiusProperty, value);
    }

    public static readonly DependencyProperty DotRadiusProperty =
      DependencyProperty.Register("DotRadius", typeof(double), typeof(SpinningWheel), new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsArrange, OnDotRadiusChanged));

    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    public static readonly DependencyProperty RadiusProperty =
      DependencyProperty.Register("Radius", typeof(double), typeof(SpinningWheel), new FrameworkPropertyMetadata(14.0, FrameworkPropertyMetadataOptions.AffectsMeasure ,OnRadiusChanged));

    public bool IsSpinning
    {
        get => (bool)GetValue(IsSpinningProperty);
        set => SetValue(IsSpinningProperty, value);
    }

    public static readonly DependencyProperty IsSpinningProperty =
      DependencyProperty.Register("IsSpinning", typeof(bool), typeof(SpinningWheel), new UIPropertyMetadata(true, OnIsSpinningChanged));

    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public static readonly DependencyProperty SpeedProperty =
      DependencyProperty.Register("Speed", typeof(double), typeof(SpinningWheel), new UIPropertyMetadata(1.0, OnSpeedChanged));

    public RotateDirection Direction
    {
        get => (RotateDirection)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public static readonly DependencyProperty DirectionProperty =
      DependencyProperty.Register("Direction", typeof(RotateDirection), typeof(SpinningWheel), new UIPropertyMetadata(RotateDirection.CW, OnDirectionChanged));

    public bool SymmetricalArrange
    {
        get => (bool)GetValue(SymmetricalArrangeProperty);
        set => SetValue(SymmetricalArrangeProperty, value);
    }

    public static readonly DependencyProperty SymmetricalArrangeProperty =
      DependencyProperty.Register("SymmetricalArrange", typeof(bool), typeof(SpinningWheel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange, OnRadiusChanged));

    private static void OnDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is SpinningWheel wheel && e.NewValue != null && wheel.storyBoard != null)
        {
            bool prevState = wheel.IsSpinning;

            wheel.ToggleSpinning(false);
            wheel.rotateAnimation.To *= -1;
            wheel.ToggleSpinning(prevState);
        }
    }

    private static void OnIsSpinningChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is SpinningWheel wheel && e.NewValue != null && wheel.storyBoard != null)
            wheel.ToggleSpinning((bool)e.NewValue);
    }

    private void ToggleSpinning(bool value)
    {
        if (value)
        {
            storyBoard.Begin();
        }
        else
        {
            storyBoard.Stop();
        }
    }

    private static void OnDotCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is SpinningWheel wheel && wheel.container != null && e.NewValue != null)
        {
            wheel.container.Children.RemoveRange(0, (int)e.OldValue);

            wheel.GenerateDots();
        }
    }

    private static void OnRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is SpinningWheel wheel && wheel.container != null && e.NewValue != null)
            UpdateEllipses(wheel.container.Children, (c, ellipse) => wheel.SetEllipsePosition(ellipse, c));
    }

    private static void UpdateEllipses(IEnumerable ellipses, Action<int, Ellipse> updateMethod)
    {
        if (updateMethod != null && ellipses != null)
        {
            int i = 1;
            foreach (object child in ellipses)
            {
                if (child is Ellipse ellipse)
                    updateMethod(i++, ellipse);
            }
        }
    }

    private static void OnDotRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is SpinningWheel wheel && wheel.container != null && e.NewValue != null)
        {
            double newRadius = (double)e.NewValue;
            UpdateEllipses(wheel.container.Children, (c, ellipse) =>
               {
                   ellipse.Width = newRadius * 2;
                   ellipse.Height = newRadius * 2;

                   wheel.SetEllipsePosition(ellipse, c);
               });
        }
    }

    private static void OnSpeedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is SpinningWheel wheel && wheel.storyBoard != null)
        {
            // don't ask
            wheel.storyBoard.SetSpeedRatio((double)e.NewValue);
            wheel.rotateAnimation.SpeedRatio = (double)e.NewValue;
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        container = GetTemplateChild("PART_Container") as Canvas;

        InitializeControl();
    }

    private void InitializeControl()
    {
        GenerateDots();
        CreateAnimation();

        ToggleSpinning(IsSpinning);
    }

    private Ellipse CreateEllipse(int counter)
    {
        Ellipse ellipse = new()
        {
            Fill = DotColor,
            Width = DotRadius * 2,
            Height = DotRadius * 2,
            Opacity = counter / (double)DotCount
        };

        SetEllipsePosition(ellipse, counter);

        return ellipse;
    }

    private Point CalculatePosition(double radian)
    {
        double x = 0 + (Radius * Math.Cos(radian));
        double y = 0 + (Radius * Math.Sin(radian));

        return new Point(x - DotRadius, y - DotRadius);
    }

    private void SetEllipsePosition(Ellipse ellipse, int ellipseCounter)
    {
        double maxCount = SymmetricalArrange ? DotCount : 2 * Radius * Math.PI / ((2 * DotRadius) + 2);

        Point position = CalculatePosition(ellipseCounter * 2 * Math.PI / maxCount);
        Canvas.SetLeft(ellipse, position.X);
        Canvas.SetTop(ellipse, position.Y);
    }

    private void GenerateDots()
    {
        for (int i = 0; i < DotCount; i++)
        {
            Ellipse ellipse = CreateEllipse(i);

            _ = container.Children.Add(ellipse);
        }
    }

    private void CreateAnimation()
    {
        rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
        rotateAnimation.SpeedRatio = Speed;
        if (Direction == RotateDirection.CCW)
            rotateAnimation.To *= -1;

        Storyboard.SetTarget(rotateAnimation, container);
        Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

        storyBoard.Children.Add(rotateAnimation);
    }
}
