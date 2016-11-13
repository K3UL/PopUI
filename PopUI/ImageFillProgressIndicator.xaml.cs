using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PopUI
{
    public partial class ImageFillProgressIndicator : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageFillProgressIndicator), new PropertyMetadata(null));
        public static readonly DependencyProperty EmptyOpacityProperty = DependencyProperty.Register("EmptyOpacity", typeof(double), typeof(ImageFillProgressIndicator), new PropertyMetadata(0.3, OpacityChangedCallback, OpacityCoerceValueCallback));
        public static readonly DependencyProperty FullOpacityProperty = DependencyProperty.Register("FullOpacity", typeof(double), typeof(ImageFillProgressIndicator), new PropertyMetadata(1.0, OpacityChangedCallback, OpacityCoerceValueCallback));
        public static readonly DependencyProperty AnimationsDurationProperty = DependencyProperty.Register("AnimationsDuration", typeof(Duration), typeof(ImageFillProgressIndicator), new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200))));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(int), typeof(ImageFillProgressIndicator), new PropertyMetadata(0));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(int), typeof(ImageFillProgressIndicator), new PropertyMetadata(100));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(ImageFillProgressIndicator), new PropertyMetadata(0, ProgressValueChangedCallback, ProgressCoerceValueCallback));
        public static readonly DependencyProperty ImageStretchProperty = DependencyProperty.Register("ImageStretch", typeof(Stretch), typeof(ImageFillProgressIndicator), new PropertyMetadata(Stretch.Uniform));
        public static readonly DependencyProperty FillOrientationProperty = DependencyProperty.Register("FillOrientation", typeof(Orientation), typeof(ImageFillProgressIndicator), new PropertyMetadata(Orientation.Horizontal, FillOrientationChagedCallback));
        public static readonly DependencyProperty IsFillOrientationInvertedProperty = DependencyProperty.Register("IsFillOrientationInverted", typeof(bool), typeof(ImageFillProgressIndicator), new PropertyMetadata(false, FillOrientationChagedCallback));

        /// <summary>
        /// Image used to show progress
        /// </summary>
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        /// <summary>
        /// Decimal value for opacity of image where there's no progress yet
        /// </summary>
        public double EmptyOpacity
        {
            get { return (double)GetValue(EmptyOpacityProperty); }
            set
            {
                SetValue(EmptyOpacityProperty, value);
            }
        }

        /// <summary>
        /// Decimal value for opacity of image where there is progress
        /// </summary>
        public double FullOpacity
        {
            get { return (double)GetValue(FullOpacityProperty); }
            set
            {
                SetValue(FullOpacityProperty, value);
            }
        }

        /// <summary>
        /// Duration of the animation between two values of progress
        /// </summary>
        public Duration AnimationsDuration
        {
            get { return (Duration)GetValue(AnimationsDurationProperty); }
            set { SetValue(AnimationsDurationProperty, value); }
        }

        /// <summary>
        /// Minimum value of the progress
        /// </summary>
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set
            {
                SetValue(MinimumProperty, value);
            }
        }

        /// <summary>
        /// Maximum value of the progress
        /// </summary>
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Current value of progress
        /// </summary>
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        /// <summary>
        /// Method to use to stretch the image to the size of the control
        /// </summary>
        public Stretch ImageStretch
        {
            get { return (Stretch)GetValue(ImageStretchProperty); }
            set { SetValue(ImageStretchProperty, value); }
        }

        /// <summary>
        /// Orientation of the filling effect on progress
        /// </summary>
        public Orientation FillOrientation
        {
            get { return (Orientation)GetValue(FillOrientationProperty); }
            set { SetValue(FillOrientationProperty, value); }
        }

        /// <summary>
        /// Indicates the direction of the filling effect. Normal is left to right or bottom to top.
        /// Inverted is right to left or top to bottom
        /// </summary>
        public bool IsFillOrientationInverted
        {
            get { return (bool)GetValue(IsFillOrientationInvertedProperty); }
            set { SetValue(IsFillOrientationInvertedProperty, value); }
        }

        private static object OpacityCoerceValueCallback(DependencyObject d, object val)
        {
            if ((double)val < 0)
                return 0.0d;
            else if ((double)val > 1)
                return 1.0d;
            else
                return val;
        }
        private static void OpacityChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ImageFillProgressIndicator;

            if (e.Property.Name == "EmptyOpacity")
            {
                Color colorEmpty = GetColorFromOpacity(ctrl.EmptyOpacity);
                ctrl.SetColorEmpty(colorEmpty);
            }
            else if (e.Property.Name == "FullOpacity")
            {
                Color colorFull = GetColorFromOpacity(ctrl.FullOpacity);
                ctrl.SetColorFull(colorFull);
            }
        }
        private static void FillOrientationChagedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressImage = d as ImageFillProgressIndicator;
            if (progressImage.FillOrientation == Orientation.Horizontal)
                progressImage.SetHorizontalProgress(progressImage.IsFillOrientationInverted);
            else if (progressImage.FillOrientation == Orientation.Vertical)
                progressImage.SetVerticalProgress(progressImage.IsFillOrientationInverted);
        }
        private static object ProgressCoerceValueCallback(DependencyObject d, object val)
        {
            var progressImage = d as ImageFillProgressIndicator;
            if ((int)val < progressImage.Minimum)
                return progressImage.Minimum;
            else if ((int)val > progressImage.Maximum)
                return progressImage.Maximum;
            else
                return val;
        }
        private static void ProgressValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressImage = d as ImageFillProgressIndicator;
            progressImage.AnimateToValue();
        }
        #endregion
        private Storyboard storyboard { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageFillProgressIndicator()
        {
            InitializeComponent();

            // Name registration of the GradientStops so they can be animated with the storyboard
            NameScope.SetNameScope(this, new NameScope());
            this.RegisterName("gradientFullEnd", gradientFullEnd);
            this.RegisterName("gradientEmptyStart", gradientEmptyStart);

            // Initialization of the Storyboard with a 60 FPS target
            storyboard = new Storyboard();
            Timeline.SetDesiredFrameRate(storyboard, 60);
        }

        /// <summary>
        /// Simple helper to get a black color with the right alpha channel
        /// </summary>
        /// <param name="opacity">Decimal value of desired opacity, between 0 and 1.</param>
        /// <returns>Color BLACK with alpha channel corresponding to required opacity</returns>
        private static Color GetColorFromOpacity(double opacity)
        {
            byte alpha = Convert.ToByte(255 * opacity);
            Color color = Color.FromArgb(alpha, 0, 0, 0);

            return color;
        }

        /// <summary>
        /// Sets the color (and transparency) of the "empty progress" GradientStops
        /// </summary>
        /// <param name="c">Color to be used. Should be black color with desired alpha channel</param>
        private void SetColorEmpty(Color c)
        {
            gradientEmptyStart.Color = c;
            gradientEmptyEnd.Color = c;
        }
        /// <summary>
        /// Sets the color (and transparency) of the "full progress" GradientStops
        /// </summary>
        /// <param name="c">Color to be used. Should be black color with desired alpha channel</param>
        private void SetColorFull(Color c)
        {
            gradientFullStart.Color = c;
            gradientFullEnd.Color = c;
        }
        /// <summary>
        /// Shows the progress filling the image from Left to Right (normal)
        /// or Right to Left (inverted)
        /// </summary>
        private void SetHorizontalProgress(bool isInverted = false)
        {
            if (!isInverted)
            {
                // Left to Right
                gradientBrush.StartPoint = new Point(0, 0.5);
                gradientBrush.EndPoint = new Point(1, 0.5);
            }
            else
            {
                // Right to Left
                gradientBrush.StartPoint = new Point(1, 0.5);
                gradientBrush.EndPoint = new Point(0, 0.5);
            }
        }
        /// <summary>
        /// Shows the progress filling the image from Bottom to Top (normal)
        /// or Top to Bottom (inverted)
        /// </summary>
        private void SetVerticalProgress(bool isInverted = false)
        {
            if (!isInverted)
            {
                // Bottom to Top
                gradientBrush.StartPoint = new Point(0.5, 1);
                gradientBrush.EndPoint = new Point(0.5, 0);
            }
            else
            {
                // Top to Bottom
                gradientBrush.StartPoint = new Point(0.5, 0);
                gradientBrush.EndPoint = new Point(0.5, 1);
            }
        }
        /// <summary>
        /// Starts the animation from the old value to the new value
        /// by animating the two GradientStops
        /// </summary>
        private void AnimateToValue()
        {
            // Calculation of percentage from Minimum/Maximum/Value
            // And transformation to decimal value
            int percent = ((Value - Minimum) * 100) / (Maximum - Minimum);
            double target = percent * 0.01d;

            // Animation of two GradientStops, in order to actually have NO gradient effect
            // NB : Animations seem to work only when registered by name (no SetTarget)
            DoubleAnimation offsetAnim1 = new DoubleAnimation();
            offsetAnim1.From = gradientFullEnd.Offset;
            offsetAnim1.To = target;
            offsetAnim1.Duration = AnimationsDuration;
            Storyboard.SetTargetName(offsetAnim1, "gradientFullEnd");
            Storyboard.SetTargetProperty(offsetAnim1, new PropertyPath(GradientStop.OffsetProperty));
            offsetAnim1.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }; // @TODO make EasingFunction a property

            DoubleAnimation offsetAnim2 = new DoubleAnimation();
            offsetAnim2.From = gradientEmptyStart.Offset;
            offsetAnim2.To = target;
            offsetAnim2.Duration = AnimationsDuration;
            Storyboard.SetTargetName(offsetAnim2, "gradientEmptyStart");
            Storyboard.SetTargetProperty(offsetAnim2, new PropertyPath(GradientStop.OffsetProperty));
            offsetAnim2.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };

            // Starts the animation
            storyboard.Stop(this);
            storyboard.Children.Clear();
            storyboard.Children.Add(offsetAnim1);
            storyboard.Children.Add(offsetAnim2);
            storyboard.Begin(this);
        }
    }
}
