using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RadialProgressbar
{
    public class RadialProgressBar : Control
    {
        #region Dependency Properties

        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(double),
            typeof(RadialProgressBar),
            new PropertyMetadata(0d));

        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(double),
            typeof(RadialProgressBar),
            new PropertyMetadata(100d, OnMinOrMaxOrValueChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value",
            typeof(double),
            typeof(RadialProgressBar),
            new PropertyMetadata(64.2d, OnMinOrMaxOrValueChanged));

        private static void OnMinOrMaxOrValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialProgressBar pg = d as RadialProgressBar;
            if (pg != null && pg.IsLoaded)
                pg.OnValueChanged((double)e.NewValue);
        }
        

        public enum NumericStyle
        {
            Percentage_0_DecimalPlaces,
            Percentage_1_DecimalPlaces,
            Percentage_2_DecimalPlaces,
            AbsoluteValue_0_DecimalPlaces,
            AbsoluteValue_1_DecimalPlaces,
            AbsoluteValue_2_DecimalPlaces,
        }
        public NumericStyle DigitStyle
        {
            get { return (NumericStyle)GetValue(DigitStyleProperty); }
            set { SetValue(DigitStyleProperty, value); }
        }
        public static readonly DependencyProperty DigitStyleProperty =
            DependencyProperty.Register("DigitStyle",
            typeof(NumericStyle),
            typeof(RadialProgressBar),
            new PropertyMetadata(NumericStyle.Percentage_0_DecimalPlaces));


        public Brush ValueArcBrush
        {
            get { return (Brush)GetValue(ValueArcBrushProperty); }
            set { SetValue(ValueArcBrushProperty, value); }
        }
        public static readonly DependencyProperty ValueArcBrushProperty =
            DependencyProperty.Register("ValueArcBrush",
            typeof(Brush),
            typeof(RadialProgressBar),
            new PropertyMetadata(Brushes.OrangeRed));

        public Brush BackArcBrush
        {
            get { return (Brush)GetValue(BackArcBrushProperty); }
            set { SetValue(BackArcBrushProperty, value); }
        }
        public static readonly DependencyProperty BackArcBrushProperty =
            DependencyProperty.Register("BackArcBrush",
            typeof(Brush),
            typeof(RadialProgressBar),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(77, 77, 77))));

        public Brush NumericBrush
        {
            get { return (Brush)GetValue(NumericBrushProperty); }
            set { SetValue(NumericBrushProperty, value); }
        }
        public static readonly DependencyProperty NumericBrushProperty =
            DependencyProperty.Register("NumericBrush",
            typeof(Brush),
            typeof(RadialProgressBar),
            new PropertyMetadata(Brushes.OrangeRed));


        public bool Glow
        {
            get { return (bool)GetValue(GlowProperty); }
            set { SetValue(GlowProperty, value); }
        }
        public static readonly DependencyProperty GlowProperty =
            DependencyProperty.Register("Glow",
            typeof(bool),
            typeof(RadialProgressBar),
            new PropertyMetadata(false));
        #endregion

        private const double ANGLE_OFFSET = (Math.PI / 180.0) * 30.0;
        private const double ANGLE_COORDINATE_ROTATION = (Math.PI / 180.0) * 270.0;
        private const double FULL_ANGLE = (2 * Math.PI) - (2 * ANGLE_OFFSET);

        private double mRadius;
        private Point mMiddle = new Point();

        private PathFigure mValuePathFigure;
        private ArcSegment mValueArc;
        private ArcSegment mBackgroundArc;
        private TextBlock mValueText;

        static RadialProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadialProgressBar), new FrameworkPropertyMetadata(typeof(RadialProgressBar)));
        }

        public RadialProgressBar()
        {
            this.Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs unused)
        {
            // Referenzen der Template Objekte holen
            InitializeComponents();

            // ArcSegment-Punkte und Radius initialisieren
            Grid rootGrid = this.Template.FindName("_RootGrid", this) as Grid;
            CalculateNewDemensions(rootGrid.ActualWidth, rootGrid.ActualHeight);

            // Enpunkt zurücksetzen
            OnValueChanged(Value);

            // Radius und Startpunkte neu berechnen wenn
            // sich die Gräße des Controls ändert
            rootGrid.SizeChanged += (s, e) => CalculateNewDemensions(e.NewSize.Width, e.NewSize.Height);
        }

        private void InitializeComponents()
        {
            mValuePathFigure = this.Template.FindName("ValuePathFigure", this) as PathFigure;
            mValueArc = this.Template.FindName("ValueArc", this) as ArcSegment;
            mBackgroundArc = this.Template.FindName("BackgroundArc", this) as ArcSegment;
            mValueText = this.Template.FindName("ValueText", this) as TextBlock;
        }

        private void CalculateNewDemensions(double width, double height)
        {
            // 1) Mittelpunkt neu berechnen
            mMiddle = new Point(width / 2.0, height / 2.0);

            // 2) Radius 10% kleiner als halbe Breite
            mRadius = mMiddle.X - (mMiddle.X / 10.0);
            mValueArc.Size = new Size(mRadius, mRadius);

            // 3) Startpunkt neu berechnen
            mValuePathFigure.StartPoint = new Point
                (Math.Cos(ANGLE_COORDINATE_ROTATION - ANGLE_OFFSET) * mRadius + mMiddle.X,
                (-Math.Sin(ANGLE_COORDINATE_ROTATION - ANGLE_OFFSET) * mRadius) + mMiddle.Y);

            // 4) Endpunkt für Hintergrund ArcSegment neu berechnen
            mBackgroundArc.Point = CalculateEndpoint(100.0);

            // 5) Value-Arc Punkt neu berechnen
            OnValueChanged(Value);
        }


        public void OnValueChanged(double newValue)
        {
            double percentage = 100 * (newValue - Min) / (Max - Min); 
           
            UpdateTextBlock(newValue, percentage);

            mValueArc.Point = CalculateEndpoint(percentage);
        }


        private Point CalculateEndpoint(double percentage)
        {
            double angle = (percentage / 100.0) * FULL_ANGLE;

            // Ab einen Winkel von 180 Grad muss der
            // große Bogen verwendet werden
            mValueArc.IsLargeArc = angle > Math.PI;

            // Offset und Drehung des Koordinatensystems berücksichtigen
            angle = ANGLE_COORDINATE_ROTATION - ANGLE_OFFSET - angle;

            return new Point(
                    Math.Cos(angle) * mRadius + mMiddle.X,
                   -Math.Sin(angle) * mRadius + mMiddle.Y);
        }


        private void UpdateTextBlock(double value, double percentage)
        {
            switch (DigitStyle)
            {
                case NumericStyle.AbsoluteValue_0_DecimalPlaces:
                    mValueText.Text = ((int)value).ToString();
                    break;
                case NumericStyle.AbsoluteValue_1_DecimalPlaces:
                    mValueText.Text = value.ToString("0.0");
                    break;
                case NumericStyle.AbsoluteValue_2_DecimalPlaces:
                    mValueText.Text = value.ToString("0.00");
                    break;
                case NumericStyle.Percentage_1_DecimalPlaces:
                    mValueText.Text = percentage.ToString("0.0") + "%";
                    break;
                case NumericStyle.Percentage_2_DecimalPlaces:
                    mValueText.Text = percentage.ToString("0.00") + "%";
                    break;
                case NumericStyle.Percentage_0_DecimalPlaces:
                default:
                    mValueText.Text = (int)percentage + "%";
                    break;
            }
        }
    }
}