using System.Windows;
using System.Windows.Media;

namespace RentalPro.Presentation.Desktop.Controls;

public sealed class DonutChart : FrameworkElement
{
    public static readonly DependencyProperty AvailableProperty =
        DependencyProperty.Register(nameof(Available), typeof(int), typeof(DonutChart),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RentedProperty =
        DependencyProperty.Register(nameof(Rented), typeof(int), typeof(DonutChart),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BookedProperty =
        DependencyProperty.Register(nameof(Booked), typeof(int), typeof(DonutChart),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty InRepairProperty =
        DependencyProperty.Register(nameof(InRepair), typeof(int), typeof(DonutChart),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public int Available
    {
        get => (int)GetValue(AvailableProperty);
        set => SetValue(AvailableProperty, value);
    }

    public int Rented
    {
        get => (int)GetValue(RentedProperty);
        set => SetValue(RentedProperty, value);
    }

    public int Booked
    {
        get => (int)GetValue(BookedProperty);
        set => SetValue(BookedProperty, value);
    }

    public int InRepair
    {
        get => (int)GetValue(InRepairProperty);
        set => SetValue(InRepairProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        var total = Available + Rented + Booked + InRepair;

        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        var radius = Math.Min(ActualWidth, ActualHeight) / 2 - 10;
        const double thickness = 14;

        var backgroundPen = new Pen(
            new SolidColorBrush(Color.FromRgb(238, 242, 248)),
            thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };

        drawingContext.DrawEllipse(
            null,
            backgroundPen,
            center,
            radius,
            radius);

        if (total <= 0)
            return;

        var startAngle = -90d;

        DrawSegment(drawingContext, center, radius, thickness, startAngle, Available, total, "#16A34A");
        startAngle += 360d * Available / total;

        DrawSegment(drawingContext, center, radius, thickness, startAngle, Rented, total, "#1E73FF");
        startAngle += 360d * Rented / total;

        DrawSegment(drawingContext, center, radius, thickness, startAngle, Booked, total, "#F59E0B");
        startAngle += 360d * Booked / total;

        DrawSegment(drawingContext, center, radius, thickness, startAngle, InRepair, total, "#F97316");
    }

    private static void DrawSegment(
        DrawingContext drawingContext,
        Point center,
        double radius,
        double thickness,
        double startAngle,
        int value,
        int total,
        string color)
    {
        if (value <= 0)
            return;

        var sweepAngle = 360d * value / total;

        if (sweepAngle >= 360)
            sweepAngle = 359.99;

        var startPoint = GetPoint(center, radius, startAngle);
        var endPoint = GetPoint(center, radius, startAngle + sweepAngle);

        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            context.BeginFigure(startPoint, false, false);
            context.ArcTo(
                endPoint,
                new Size(radius, radius),
                0,
                sweepAngle > 180,
                SweepDirection.Clockwise,
                true,
                false);
        }

        geometry.Freeze();

        var pen = new Pen(
            (SolidColorBrush)new BrushConverter().ConvertFromString(color)!,
            thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };

        drawingContext.DrawGeometry(null, pen, geometry);
    }

    private static Point GetPoint(Point center, double radius, double angle)
    {
        var radians = Math.PI * angle / 180d;

        return new Point(
            center.X + radius * Math.Cos(radians),
            center.Y + radius * Math.Sin(radians));
    }
}