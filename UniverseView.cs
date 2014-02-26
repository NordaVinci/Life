using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ConwaysLife.Utility;

namespace ConwaysLife
{
    class UniverseView : FrameworkElement
    {

        #region Fields
        Universe _model;
        Location _last;

        List<Visual> _visuals = new List<Visual>();
        DrawingVisual _gridVisual = new DrawingVisual();
        DrawingVisual _cellsVisual = new DrawingVisual();
        DrawingVisual _adornerVisual = new DrawingVisual();
        #endregion

        #region Dependency properties

        public int CellSize
        {
            get { return (int)GetValue(CellSizeProperty); }
            set { SetValue(CellSizeProperty, value); }
        }

        public static readonly DependencyProperty CellSizeProperty =
            DependencyProperty.Register("CellSize", typeof(int), typeof(UniverseView), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Dimensions Dimensions
        {
            get { return (Dimensions)GetValue(DimensionsProperty); }
            set { SetValue(DimensionsProperty, value); }
        }

        public static readonly DependencyProperty DimensionsProperty =
            DependencyProperty.Register("Dimensions", typeof(Dimensions), typeof(UniverseView),
                new FrameworkPropertyMetadata(new Dimensions(100, 100), FrameworkPropertyMetadataOptions.AffectsMeasure, DimensionsChangedCallback));

        static void DimensionsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((UniverseView)d).UpdateDimensions();
        }

        public int Generation
        {
            get { return (int)GetValue(GenerationProperty); }
            set { SetValue(GenerationProperty, value); }
        }

        public static readonly DependencyProperty GenerationProperty =
            DependencyProperty.Register("Generation", typeof(int), typeof(UniverseView), new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty BackgroundProperty;
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty ForegroundProperty;
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public static readonly DependencyProperty PaddingProperty;
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        #endregion

        #region Ctors
        static UniverseView()
        {
            BackgroundProperty = Control.BackgroundProperty.AddOwner(typeof(UniverseView),
                new FrameworkPropertyMetadata(Brushes.LightGoldenrodYellow, FrameworkPropertyMetadataOptions.AffectsRender));
            ForegroundProperty = Control.ForegroundProperty.AddOwner(typeof(UniverseView), new FrameworkPropertyMetadata(Brushes.LightCoral, FrameworkPropertyMetadataOptions.AffectsRender));
            PaddingProperty = Control.PaddingProperty.AddOwner(typeof(UniverseView), new FrameworkPropertyMetadata(new Thickness(10), FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public UniverseView()
        {
            AddVisual(_gridVisual);
            AddVisual(_cellsVisual);
            AddVisual(_adornerVisual);
        }
        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            UpdateDimensions();
        }

        protected void UpdateDimensions()
        {
            _model = new Universe(this.Dimensions);
            DrawGrid();
            DrawModel();
        }

        protected void UpdateScreen()
        {
            DrawGrid();
            DrawModel();
        }


        protected override Size MeasureOverride(Size availableSize)
        {
            Dimensions dim = this.Dimensions;
            int cellSize = this.CellSize;
            Thickness padding = this.Padding;
            return new Size(dim.Width * cellSize + padding.Left + padding.Right, dim.Height * cellSize + padding.Top + padding.Bottom);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            DrawGrid();
            DrawModel();
        }

        void DrawGrid()
        {
            using (DrawingContext dc = _gridVisual.RenderOpen())
            {
                Dimensions dim = this.Dimensions;
                Thickness padding = this.Padding;
                int cellSize = this.CellSize;
                Rect grid = new Rect(
                    padding.Left,
                    padding.Top,
                    dim.Width * cellSize,
                    dim.Height * cellSize
                    );
                dc.DrawRectangle(this.Background, null, grid);
            }
        }

        public void DrawModel()
        {
            using (DrawingContext dc = _cellsVisual.RenderOpen())
            {
                Thickness padding = this.Padding;
                int cellSize = this.CellSize;
                Brush cellBrush = this.Foreground;
                foreach (var item in _model.LiveItems)
                {
                    Rect rect = new Rect(padding.Left + item.x * cellSize + 1, padding.Top + item.y * cellSize + 1, cellSize - 1, cellSize - 1);
                    dc.DrawRectangle(cellBrush, null, rect);
                }
            }
        }

        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Point pt = e.GetPosition(this);
            Location loc = GetLocationFromPt(pt);
            if (loc.IsValid)
            {
                var curValue = _model.Get(loc.X, loc.Y);
                if (curValue == 0)
                    _model.Set(loc.X, loc.Y, 1);
                else
                    _model.Set(loc.X, loc.Y, 0);
                
                DrawModel();
            }
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point pt = e.GetPosition(this);
            Location loc = GetLocationFromPt(pt);
            if (!loc.IsValid || loc == _last)
                return;

            _last = loc;
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                _model.Set(loc.X, loc.Y, 1);
                DrawModel();
            }
            else
            {
                Thickness padding = this.Padding;
                int cellSize = this.CellSize;
                Rect rect = new Rect(padding.Left + loc.X * cellSize + 1, padding.Top + loc.Y * cellSize + 1, cellSize - 1, cellSize - 1);
                using (DrawingContext dc = _adornerVisual.RenderOpen())
                    dc.DrawRectangle(Brushes.HotPink, null, rect);
            }
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            using (_adornerVisual.RenderOpen());// clears any previous drawing
        }

        public void Clear()
        {
            _model.Clear();
            this.Generation = 0;
            DrawModel();
        }

        public bool Next()// returns true if changed
        {
            try
            {
                bool ret = _model.NextGeneration();
                this.Generation++;
                DrawModel();
                return ret;
            }
            catch
            {
                return false;
            }
        }

        public Location GetLocationFromPt(Point pt)
        {
            Thickness padding = this.Padding;
            Dimensions dim = this.Dimensions;
            int cellSize = this.CellSize;
            return new Location((long)((pt.X - padding.Left) / cellSize), (long)((pt.Y - padding.Top) / cellSize), dim);
        }

        void AddVisual(Visual child)
        {
            this.AddLogicalChild(child);
            this.AddVisualChild(child);
            _visuals.Add(child);
        }

        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return _visuals.Count;
            }
        }

        public Universe GetUniverseModel ()
        {
            return _model;
        }

        public void SetModel (Universe model)
        {
            _model = model;
            this.Dimensions = new Dimensions(model.UNIVERSE_SIZE_X, model.UNIVERSE_SIZE_Y);
            UpdateScreen();
        }
    }
}
