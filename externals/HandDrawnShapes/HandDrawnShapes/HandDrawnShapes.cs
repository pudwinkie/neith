using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls;

namespace HandDrawnShapes
{
    public class HandDrawnShape : Shape
    {
        #region Constructor

        private static Random _Random = new Random();
        public HandDrawnShape()
        {
            Seed = _Random.Next();
        }

        #endregion

        #region Protected Properties and Fields

        protected PathGeometry _EmptyGeometry = new PathGeometry();

        #endregion

        #region Randomness Property

        public double Randomness
        {
            get { return (double)GetValue(RandomnessProperty); }
            set { SetValue(RandomnessProperty, value); }
        }
        public static readonly DependencyProperty RandomnessProperty =
            DependencyProperty.Register(
                "Randomness",
                typeof(double),
                typeof(HandDrawnShape),
                new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region Seed Property

        public int Seed
        {
            get { return (int)GetValue(SeedProperty); }
            set { SetValue(SeedProperty, value); }
        }
        public static readonly DependencyProperty SeedProperty =
            DependencyProperty.Register(
                "Seed",
                typeof(int),
                typeof(HandDrawnShape),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region StrokeOffsetRandomness Property

        public double StrokeOffsetRandomness
        {
            get { return (double)GetValue(StrokeOffsetRandomnessProperty); }
            set { SetValue(StrokeOffsetRandomnessProperty, value); }
        }
        public static readonly DependencyProperty StrokeOffsetRandomnessProperty =
            DependencyProperty.Register(
                "StrokeOffsetRandomness",
                typeof(double),
                typeof(HandDrawnShape),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region Messiness Property

        public int Messiness
        {
            get { return (int)GetValue(MessinessProperty); }
            set { SetValue(MessinessProperty, value); }
        }
        public static readonly DependencyProperty MessinessProperty =
            DependencyProperty.Register(
                "Messiness",
                typeof(int),
                typeof(HandDrawnShape),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region ScaleToBounds Property

        public bool ScaleToBounds
        {
            get { return (bool)GetValue(ScaleToBoundsProperty); }
            set { SetValue(ScaleToBoundsProperty, value); }
        }
        public static readonly DependencyProperty ScaleToBoundsProperty =
            DependencyProperty.Register(
                "ScaleToBounds",
                typeof(bool),
                typeof(HandDrawnShape),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region Overridden Methods

        protected override Geometry DefiningGeometry
        {
            get { return _EmptyGeometry; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return base.MeasureOverride(constraint);
            InvalidateVisual();
        }

        #endregion
    }

    public static class HandDrawnShapeUtil
    {
        #region GetRectangleGeometry

        public static PathGeometry GetRectangleGeometry(double Width, double Height, int Seed, double Randomness,
            double RadiusX, double RadiusY)
        {
            return GetRectangleGeometry(Width, Height, Seed, Randomness, new CornerRadius(RadiusX), new CornerRadius(RadiusY), new Thickness(1));
        }

        public static PathGeometry GetRectangleGeometry(double Width, double Height, int Seed, double Randomness,
            CornerRadius CornerRadius, Thickness BorderThickness)
        {
            return GetRectangleGeometry(Width, Height, Seed, Randomness, CornerRadius, CornerRadius, BorderThickness);
        }

        private static PathGeometry GetRectangleGeometry(double Width, double Height, int Seed, double Randomness,
            CornerRadius CornerRadiusX, CornerRadius CornerRadiusY, Thickness BorderThickness)
        {
            Random R = new Random(Seed);

            double _TopLeftX = Math.Min(Width / 2, CornerRadiusX.TopLeft + (R.NextDouble() * Randomness));
            double _TopRightX = Math.Min(Width / 2, CornerRadiusX.TopRight + (R.NextDouble() * Randomness));
            double _BottomLeftX = Math.Min(Width / 2, CornerRadiusX.BottomLeft + (R.NextDouble() * Randomness));
            double _BottomRightX = Math.Min(Width / 2, CornerRadiusX.BottomRight + (R.NextDouble() * Randomness));

            double _TopLeftY = Math.Min(Height / 2, CornerRadiusY.TopLeft + (R.NextDouble() * Randomness));
            double _TopRightY = Math.Min(Height / 2, CornerRadiusY.TopRight + (R.NextDouble() * Randomness));
            double _BottomLeftY = Math.Min(Height / 2, CornerRadiusY.BottomLeft + (R.NextDouble() * Randomness));
            double _BottomRightY = Math.Min(Height / 2, CornerRadiusY.BottomRight + (R.NextDouble() * Randomness));

            PathFigure _PathFigure = new PathFigure();
            _PathFigure.StartPoint = new Point(0, _TopLeftY);
            _PathFigure.IsClosed = true;

            // top left corner
            BezierSegment topleft = new BezierSegment(
                new Point(0, _TopLeftY / 2),
                new Point(_TopLeftX / 2, 0),
                new Point(_TopLeftX, 0),
                (BorderThickness.Top > 0 && BorderThickness.Left > 0));

            // top
            BezierSegment top = new BezierSegment(
                new Point(Width / 2, RandomScale(0, R, Randomness, Height)),
                new Point(Width / 2, RandomScale(0, R, Randomness, Height)),
                new Point(Width - _TopRightX, 0),
                BorderThickness.Top > 0);

            // top right corner
            BezierSegment topright = new BezierSegment(
                new Point(Width - _TopRightX / 2, 0),
                new Point(Width, _TopRightY / 2),
                new Point(Width, _TopRightY),
                (BorderThickness.Top > 0 && BorderThickness.Right > 0));

            // right
            BezierSegment right = new BezierSegment(
                new Point(RandomScale(1, R, Randomness, Width), Height / 2),
                new Point(RandomScale(1, R, Randomness, Width), Height / 2),
                new Point(Width, Height - _BottomRightY),
                BorderThickness.Right > 0);

            // bottom right corner
            BezierSegment bottomright = new BezierSegment(
                new Point(Width, Height - _BottomRightY / 2),
                new Point(Width - _BottomRightX / 2, Height),
                new Point(Width - _BottomRightX, Height),
                (BorderThickness.Bottom > 0 && BorderThickness.Right > 0));

            // bottom
            BezierSegment bottom = new BezierSegment(
                new Point(Width / 2, RandomScale(1, R, Randomness, Height)),
                new Point(Width / 2, RandomScale(1, R, Randomness, Height)),
                new Point(_BottomLeftX, Height),
                BorderThickness.Bottom > 0);

            // bottom left corner
            BezierSegment bottomleft = new BezierSegment(
                new Point(_BottomLeftX / 2, Height),
                new Point(0, Height - _BottomLeftY / 2),
                new Point(0, Height - _BottomLeftY),
                (BorderThickness.Bottom > 0 && BorderThickness.Left > 0));

            // left
            BezierSegment left = new BezierSegment(
                new Point(RandomScale(0, R, Randomness, Width), Height / 2),
                new Point(RandomScale(0, R, Randomness, Width), Height / 2),
                new Point(0, _TopLeftY),
                BorderThickness.Left > 0);

            _PathFigure.Segments = new PathSegmentCollection();
            _PathFigure.Segments.Add(topleft);
            _PathFigure.Segments.Add(top);
            _PathFigure.Segments.Add(topright);
            _PathFigure.Segments.Add(right);
            _PathFigure.Segments.Add(bottomright);
            _PathFigure.Segments.Add(bottom);
            _PathFigure.Segments.Add(bottomleft);
            _PathFigure.Segments.Add(left);

            PathGeometry _PathGeometry = new PathGeometry();
            _PathGeometry.Figures.Add(_PathFigure);

            return _PathGeometry;
        }

        #endregion

        #region GetEllipseGeometry

        public static PathGeometry GetEllipseGeometry(double Width, double Height, int Seed, double Randomness)
        {
            Random R = new Random(Seed);

            PathFigure _PathFigure = new PathFigure();
            _PathFigure.StartPoint = new Point(Width, Height / 2);
            _PathFigure.IsClosed = true;

            BezierSegment b1 = new BezierSegment(
                new Point(RandomScale(1, R, Randomness, Width), RandomScale(0.775, R, Randomness, Height)),
                new Point(RandomScale(0.775, R, Randomness, Width), RandomScale(1, R, Randomness, Height)),
                new Point(RandomScale(0.5, R, Randomness, Width), RandomScale(1, R, Randomness, Height)),
                true);
            b1.IsSmoothJoin = true;

            BezierSegment b2 = new BezierSegment(
                new Point(RandomScale(0.225, R, Randomness, Width), RandomScale(1, R, Randomness, Height)),
                new Point(RandomScale(0, R, Randomness, Width), RandomScale(0.775, R, Randomness, Height)),
                new Point(RandomScale(0, R, Randomness, Width), RandomScale(0.5, R, Randomness, Height)),
                true);
            b2.IsSmoothJoin = true;

            BezierSegment b3 = new BezierSegment(
                new Point(RandomScale(0, R, Randomness, Width), RandomScale(0.225, R, Randomness, Height)),
                new Point(RandomScale(0.225, R, Randomness, Width), RandomScale(0, R, Randomness, Height)),
                new Point(RandomScale(0.5, R, Randomness, Width), RandomScale(0, R, Randomness, Height)),
                true);
            b3.IsSmoothJoin = true;

            BezierSegment b4 = new BezierSegment(
                new Point(RandomScale(0.775, R, Randomness, Width), RandomScale(0, R, Randomness, Height)),
                new Point(RandomScale(1, R, Randomness, Width), RandomScale(0.225, R, Randomness, Height)),
                new Point(Width, Height / 2),
                true);
            b4.IsSmoothJoin = true;

            _PathFigure.Segments = new PathSegmentCollection(4);
            _PathFigure.Segments.Add(b1);
            _PathFigure.Segments.Add(b2);
            _PathFigure.Segments.Add(b3);
            _PathFigure.Segments.Add(b4);

            PathGeometry _PathGeometry = new PathGeometry();
            _PathGeometry.Figures.Add(_PathFigure);

            return _PathGeometry;
        }

        #endregion

        #region Private Helper Methods


        public static double RandomScale(double d, Random random, double randomness, double scaler)
        {
            double r = random.NextDouble();

            // this strange line is because we generally want a negative here -- for
            // some reason this seems to produce nicer looking results

            if (random.Next() % 3 != 1) r = r * -1;

            double t = (r * randomness);

            if (t > 0) Debug.WriteLine("+");
            else Debug.WriteLine("-");

            return (scaler * d) + t;
        }

        #endregion
    }

    public class HandDrawnRectangle : HandDrawnShape
    {
        #region RadiusX Property

        public double RadiusX
        {
            get { return (double)GetValue(RadiusXProperty); }
            set { SetValue(RadiusXProperty, value); }
        }
        public static readonly DependencyProperty RadiusXProperty =
            DependencyProperty.Register("RadiusX", typeof(double), typeof(HandDrawnRectangle), new UIPropertyMetadata(0.0));

        #endregion

        #region RadiusX Property

        public double RadiusY
        {
            get { return (double)GetValue(RadiusYProperty); }
            set { SetValue(RadiusYProperty, value); }
        }
        public static readonly DependencyProperty RadiusYProperty =
            DependencyProperty.Register("RadiusY", typeof(double), typeof(HandDrawnRectangle), new UIPropertyMetadata(0.0));

        #endregion

        #region OnRender

        protected override void OnRender(DrawingContext dc)
        {
            if (Messiness <= 1 && StrokeOffsetRandomness == 0)
            {
                PathGeometry FillGeometry =
                    HandDrawnShapeUtil.GetRectangleGeometry(ActualWidth, ActualHeight, Seed, Randomness, RadiusX, RadiusY);

                Pen Pen = new Pen(Stroke, StrokeThickness);

                if (ScaleToBounds)
                {
                    double scaleX = ActualWidth / FillGeometry.GetRenderBounds(Pen).Width;
                    double scaleY = ActualHeight / FillGeometry.GetRenderBounds(Pen).Height;

                    double ScaleOffsetX = ActualWidth * (1 - scaleX);
                    double ScaleOffsetY = ActualHeight * (1 - scaleY);

                    TransformGroup g = new TransformGroup();
                    g.Children.Add(new ScaleTransform(scaleX, scaleY));
                    g.Children.Add(new TranslateTransform(ScaleOffsetX, ScaleOffsetY));
                    FillGeometry.Transform = g;
                }

                dc.DrawGeometry(Fill, Pen, FillGeometry);
            }
            else
            {
                // calculate the offset values

                int NewSeed = Seed;
                double OffsetX = 0;
                double OffsetY = 0;

                if (StrokeOffsetRandomness > 0)
                {
                    NewSeed += 100;
                    Random R = new Random(NewSeed);
                    OffsetX = HandDrawnShapeUtil.RandomScale(0, R, StrokeOffsetRandomness, 100);
                    OffsetY = OffsetX; //HandDrawnShapeUtil.RandomScale(0, R, StrokeOffsetRandomness, 100);
                }

                // calculate the fill first

                PathGeometry FillGeometry =
                    HandDrawnShapeUtil.GetRectangleGeometry(ActualWidth, ActualHeight, Seed, Randomness, RadiusX, RadiusY);

                // calculate the stroke 

                int AdjustedMessiness = Math.Max(Messiness, 1);
                StrokeGeometry[] StrokeGeometries = new StrokeGeometry[AdjustedMessiness];

                for (int i = 0; i < AdjustedMessiness; i++)
                {
                    NewSeed += 100;
                    Random R = new Random(NewSeed + 50);
                    double AdjustedThickness = Math.Max(StrokeThickness / AdjustedMessiness, StrokeThickness / 3);

                    StrokeGeometries[i].Geometry = HandDrawnShapeUtil.GetRectangleGeometry(ActualWidth, ActualHeight, NewSeed, Randomness, RadiusX, RadiusY);
                    StrokeGeometries[i].Pen = new Pen(Stroke, AdjustedThickness);
                    StrokeGeometries[i].OffsetX = OffsetX + HandDrawnShapeUtil.RandomScale(1, R, StrokeThickness + AdjustedMessiness / 3, 1);
                    StrokeGeometries[i].OffsetY = OffsetY + HandDrawnShapeUtil.RandomScale(1, R, StrokeThickness + AdjustedMessiness / 3, 1);
                }

                double scaleX = 1;
                double scaleY = 1;

                if (ScaleToBounds)
                {

                    // calculate the bounds so we can scale properly and corresponding scaling factors

                    Rect MaxBounds = FillGeometry.Bounds;

                    foreach (StrokeGeometry s in StrokeGeometries)
                    {
                        Rect Bounds = s.Geometry.GetRenderBounds(s.Pen);
                        if ((Bounds.Width + Math.Abs(s.OffsetX)) > MaxBounds.Width) MaxBounds.Width = Bounds.Width + Math.Abs(s.OffsetX);
                        if ((Bounds.Height + Math.Abs(s.OffsetY)) > MaxBounds.Height) MaxBounds.Height = Bounds.Height + Math.Abs(s.OffsetY);
                    }

                    scaleX = ActualWidth / MaxBounds.Width;
                    scaleY = ActualHeight / MaxBounds.Height;
                }

                // draw everything

                double ScaleOffsetX = ActualWidth * (1 - scaleX);
                double ScaleOffsetY = ActualHeight * (1 - scaleY);

                TransformGroup g = new TransformGroup();
                g.Children.Add(new ScaleTransform(scaleX, scaleY));
                g.Children.Add(new TranslateTransform(ScaleOffsetX, ScaleOffsetY));
                FillGeometry.Transform = g;
                dc.DrawGeometry(Fill, null, FillGeometry);

                foreach (StrokeGeometry s in StrokeGeometries)
                {
                    g = new TransformGroup();
                    g.Children.Add(new ScaleTransform(scaleX, scaleY));
                    g.Children.Add(new TranslateTransform(ScaleOffsetX + s.OffsetX, ScaleOffsetY + s.OffsetY));
                    s.Geometry.Transform = g;
                    dc.DrawGeometry(null, s.Pen, s.Geometry);

                }
            }
        }

        #endregion
    }

    public class HandDrawnEllipse : HandDrawnShape
    {
        #region OnRender

        protected override void OnRender(DrawingContext dc)
        {
            if (Messiness <= 1 && StrokeOffsetRandomness == 0)
            {
                PathGeometry FillGeometry =
                    HandDrawnShapeUtil.GetEllipseGeometry(ActualWidth, ActualHeight, Seed, Randomness);

                Pen Pen = new Pen(Stroke, StrokeThickness);

                if (ScaleToBounds)
                {
                    double scaleX = ActualWidth / FillGeometry.GetRenderBounds(Pen).Width;
                    double scaleY = ActualHeight / FillGeometry.GetRenderBounds(Pen).Height;

                    double ScaleOffsetX = ActualWidth * (1 - scaleX);
                    double ScaleOffsetY = ActualHeight * (1 - scaleY);

                    TransformGroup g = new TransformGroup();
                    g.Children.Add(new ScaleTransform(scaleX, scaleY));
                    g.Children.Add(new TranslateTransform(ScaleOffsetX, ScaleOffsetY));
                    FillGeometry.Transform = g;
                }

                dc.DrawGeometry(Fill, Pen, FillGeometry);
            }
            else
            {
                // calculate the offset values

                int NewSeed = Seed;
                double OffsetX = 0;
                double OffsetY = 0;

                if (StrokeOffsetRandomness > 0)
                {
                    NewSeed += 100;
                    Random R = new Random(NewSeed);
                    OffsetX = HandDrawnShapeUtil.RandomScale(0, R, StrokeOffsetRandomness, 100);
                    OffsetY = OffsetX; // HandDrawnShapeUtil.RandomScale(0, R, StrokeOffsetRandomness, 100);
                }

                // calculate the fill first

                PathGeometry FillGeometry =
                    HandDrawnShapeUtil.GetEllipseGeometry(ActualWidth, ActualHeight, Seed, Randomness);

                // calculate the stroke 

                int AdjustedMessiness = Math.Max(Messiness, 1);
                StrokeGeometry[] StrokeGeometries = new StrokeGeometry[AdjustedMessiness];

                for (int i = 0; i < AdjustedMessiness; i++)
                {
                    NewSeed += 100;
                    Random R = new Random(NewSeed + 50);
                    double AdjustedThickness = Math.Max(StrokeThickness / AdjustedMessiness, StrokeThickness / 3);

                    StrokeGeometries[i].Geometry = HandDrawnShapeUtil.GetEllipseGeometry(ActualWidth, ActualHeight, NewSeed, Randomness);
                    StrokeGeometries[i].Pen = new Pen(Stroke, AdjustedThickness);
                    StrokeGeometries[i].OffsetX = OffsetX + HandDrawnShapeUtil.RandomScale(1, R, StrokeThickness + AdjustedMessiness/3, 1);
                    StrokeGeometries[i].OffsetY = OffsetY + HandDrawnShapeUtil.RandomScale(1, R, StrokeThickness + AdjustedMessiness/3, 1);
                }

                double scaleX = 1;
                double scaleY = 1;

                if (ScaleToBounds)
                {

                    // calculate the bounds so we can scale properly and corresponding scaling factors

                    Rect MaxBounds = FillGeometry.Bounds;

                    foreach (StrokeGeometry s in StrokeGeometries)
                    {
                        Rect Bounds = s.Geometry.GetRenderBounds(s.Pen);
                        if ((Bounds.Width + Math.Abs(s.OffsetX)) > MaxBounds.Width) MaxBounds.Width = Bounds.Width + Math.Abs(s.OffsetX);
                        if ((Bounds.Height + Math.Abs(s.OffsetY)) > MaxBounds.Height) MaxBounds.Height = Bounds.Height + Math.Abs(s.OffsetY);
                    }

                    scaleX = ActualWidth / MaxBounds.Width;
                    scaleY = ActualHeight / MaxBounds.Height;
                }

                // draw everything

                double ScaleOffsetX = ActualWidth * (1 - scaleX);
                double ScaleOffsetY = ActualHeight * (1 - scaleY);

                TransformGroup g = new TransformGroup();
                g.Children.Add(new ScaleTransform(scaleX, scaleY));
                g.Children.Add(new TranslateTransform(ScaleOffsetX, ScaleOffsetY));
                FillGeometry.Transform = g;
                dc.DrawGeometry(Fill, null, FillGeometry);

                foreach (StrokeGeometry s in StrokeGeometries)
                {
                    g = new TransformGroup();
                    g.Children.Add(new ScaleTransform(scaleX, scaleY));
                    g.Children.Add(new TranslateTransform(ScaleOffsetX + s.OffsetX, ScaleOffsetY + s.OffsetY));
                    s.Geometry.Transform = g;
                    dc.DrawGeometry(null, s.Pen, s.Geometry);

                }
            }
        }

        #endregion

        #region Private Helper Methods

        private void ApplyScaleToBounds(ref PathGeometry Geometry, Pen Pen, double OffsetX, double OffsetY)
        {
            ApplyScaleToBounds(ref Geometry, Pen, OffsetX, OffsetY, true);
        }

        private void ApplyScaleToBounds(ref PathGeometry Geometry, Pen Pen, double OffsetX, double OffsetY, bool ApplyOffset)
        {
            TransformGroup g = new TransformGroup();

            double scaleX = 1;
            double scaleY = 1;

            if (ScaleToBounds)
            {
                if (Pen != null)
                {
                    scaleX = this.ActualWidth / (Geometry.GetRenderBounds(Pen).Width + Math.Abs(OffsetX));
                    scaleY = this.ActualHeight / (Geometry.GetRenderBounds(Pen).Height + Math.Abs(OffsetY));
                }
                else
                {
                    scaleX = this.ActualWidth / (Geometry.Bounds.Width + Math.Abs(OffsetX));
                    scaleY = this.ActualHeight / (Geometry.Bounds.Height + Math.Abs(OffsetY));
                }

                g.Children.Add(new ScaleTransform(scaleX, scaleY));
            }

            if (ApplyOffset)
            {
                g.Children.Add(new TranslateTransform((OffsetX + scaleX / 2), (OffsetY + scaleY / 2)));
            }
            else
            {
                g.Children.Add(new TranslateTransform((scaleX / 2), (scaleY / 2)));
            }

            Random R = new Random(Seed);
            int degrees = R.Next(360);
            Transform rotate = new RotateTransform(degrees, Width / 2, Height / 2);
            g.Children.Add(rotate);

            Geometry.Transform = g;
        }

        #endregion
    }

    public class HandDrawnBorder : Border
    {
        #region Constructor

        private static Random _Random = new Random();
        public HandDrawnBorder()
        {
            Seed = _Random.Next();
            Seed = _Random.Next();
            Seed = _Random.Next();
        }

        #endregion

        #region OnRender

        protected override void OnRender(DrawingContext dc)
        {
            double AdjustedBorderThickness = Math.Max(BorderThickness.Left, Math.Max(BorderThickness.Top, Math.Max(BorderThickness.Right, BorderThickness.Bottom)));

            if (Messiness <= 1 && StrokeOffsetRandomness == 0)
            {
                PathGeometry FillGeometry =
                    HandDrawnShapeUtil.GetRectangleGeometry(ActualWidth, ActualHeight, Seed, Randomness, CornerRadius, BorderThickness);

                Pen Pen = new Pen(BorderBrush, AdjustedBorderThickness);

                if (ScaleToBounds)
                {
                    double scaleX = ActualWidth / FillGeometry.GetRenderBounds(Pen).Width;
                    double scaleY = ActualHeight / FillGeometry.GetRenderBounds(Pen).Height;

                    double ScaleOffsetX = ActualWidth * (1 - scaleX);
                    double ScaleOffsetY = ActualHeight * (1 - scaleY);

                    TransformGroup g = new TransformGroup();
                    g.Children.Add(new ScaleTransform(scaleX, scaleY));
                    g.Children.Add(new TranslateTransform(ScaleOffsetX, ScaleOffsetY));
                    FillGeometry.Transform = g;
                }

                dc.DrawGeometry(Background, Pen, FillGeometry);
            }
            else
            {
                // calculate the offset values

                int NewSeed = Seed;
                double OffsetX = 0;
                double OffsetY = 0;

                if (StrokeOffsetRandomness > 0)
                {
                    NewSeed += 100;
                    Random R = new Random(NewSeed);
                    OffsetX = HandDrawnShapeUtil.RandomScale(0, R, StrokeOffsetRandomness, 100);
                    OffsetY = OffsetX; // HandDrawnShapeUtil.RandomScale(0, R, StrokeOffsetRandomness, 100);
                }

                // calculate the fill first

                PathGeometry FillGeometry =
                    HandDrawnShapeUtil.GetRectangleGeometry(ActualWidth, ActualHeight, Seed, Randomness, CornerRadius, BorderThickness);

                // calculate the stroke 

                int AdjustedMessiness = Math.Max(Messiness, 1);
                StrokeGeometry[] StrokeGeometries = new StrokeGeometry[AdjustedMessiness];

                for (int i = 0; i < AdjustedMessiness; i++)
                {
                    NewSeed += 100;
                    Random R = new Random(NewSeed + 50);
                    double AdjustedThickness = Math.Max(AdjustedBorderThickness / AdjustedMessiness, AdjustedBorderThickness / 3);

                    StrokeGeometries[i].Geometry = HandDrawnShapeUtil.GetRectangleGeometry(ActualWidth, ActualHeight, NewSeed, Randomness, CornerRadius, BorderThickness);
                    StrokeGeometries[i].Pen = new Pen(BorderBrush, AdjustedThickness);
                    StrokeGeometries[i].OffsetX = OffsetX + HandDrawnShapeUtil.RandomScale(1, R, AdjustedBorderThickness + AdjustedMessiness / 3, 1);
                    StrokeGeometries[i].OffsetY = OffsetY + HandDrawnShapeUtil.RandomScale(1, R, AdjustedBorderThickness + AdjustedMessiness / 3, 1);
                }

                double scaleX = 1;
                double scaleY = 1;

                if (ScaleToBounds)
                {

                    // calculate the bounds so we can scale properly and corresponding scaling factors

                    Rect MaxBounds = FillGeometry.Bounds;

                    foreach (StrokeGeometry s in StrokeGeometries)
                    {
                        Rect Bounds = s.Geometry.GetRenderBounds(s.Pen);
                        if ((Bounds.Width + Math.Abs(s.OffsetX)) > MaxBounds.Width) MaxBounds.Width = Bounds.Width + Math.Abs(s.OffsetX);
                        if ((Bounds.Height + Math.Abs(s.OffsetY)) > MaxBounds.Height) MaxBounds.Height = Bounds.Height + Math.Abs(s.OffsetY);
                    }

                    scaleX = ActualWidth / MaxBounds.Width;
                    scaleY = ActualHeight / MaxBounds.Height;
                }

                // draw everything

                double ScaleOffsetX = ActualWidth * (1 - scaleX);
                double ScaleOffsetY = ActualHeight * (1 - scaleY);

                TransformGroup g = new TransformGroup();
                g.Children.Add(new ScaleTransform(scaleX, scaleY));
                g.Children.Add(new TranslateTransform(ScaleOffsetX, ScaleOffsetY));
                FillGeometry.Transform = g;
                dc.DrawGeometry(Background, null, FillGeometry);

                foreach (StrokeGeometry s in StrokeGeometries)
                {
                    g = new TransformGroup();
                    g.Children.Add(new ScaleTransform(scaleX, scaleY));
                    g.Children.Add(new TranslateTransform(ScaleOffsetX + s.OffsetX, ScaleOffsetY + s.OffsetY));
                    s.Geometry.Transform = g;
                    dc.DrawGeometry(null, s.Pen, s.Geometry);

                }
            }
        }

        #endregion

        #region MeasureOverride

        protected override Size MeasureOverride(Size constraint)
        {
            return base.MeasureOverride(constraint);
            InvalidateVisual();

        }

        #endregion

        #region Randomness Property

        public double Randomness
        {
            get { return (double)GetValue(RandomnessProperty); }
            set { SetValue(RandomnessProperty, value); }
        }
        public static readonly DependencyProperty RandomnessProperty =
            DependencyProperty.Register(
                "Randomness",
                typeof(double),
                typeof(HandDrawnBorder),
                new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region Seed Property

        public int Seed
        {
            get { return (int)GetValue(SeedProperty); }
            set { SetValue(SeedProperty, value); }
        }
        public static readonly DependencyProperty SeedProperty =
            DependencyProperty.Register(
                "Seed",
                typeof(int),
                typeof(HandDrawnBorder),
                new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region StrokeOffsetRandomness Property

        public double StrokeOffsetRandomness
        {
            get { return (double)GetValue(StrokeOffsetRandomnessProperty); }
            set { SetValue(StrokeOffsetRandomnessProperty, value); }
        }
        public static readonly DependencyProperty StrokeOffsetRandomnessProperty =
            DependencyProperty.Register(
                "StrokeOffsetRandomness",
                typeof(double),
                typeof(HandDrawnBorder),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region Messiness Property

        public int Messiness
        {
            get { return (int)GetValue(MessinessProperty); }
            set { SetValue(MessinessProperty, value); }
        }
        public static readonly DependencyProperty MessinessProperty =
            DependencyProperty.Register(
                "Messiness",
                typeof(int),
                typeof(HandDrawnBorder),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region ScaleToBounds Property

        public bool ScaleToBounds
        {
            get { return (bool)GetValue(ScaleToBoundsProperty); }
            set { SetValue(ScaleToBoundsProperty, value); }
        }
        public static readonly DependencyProperty ScaleToBoundsProperty =
            DependencyProperty.Register(
                "ScaleToBounds",
                typeof(bool),
                typeof(HandDrawnBorder),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

    }

    #region Helper Classes

    internal struct StrokeGeometry
    {
        public PathGeometry Geometry;
        public double OffsetX;
        public double OffsetY;
        public Pen Pen;
    }


    #endregion

}
