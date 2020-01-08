using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AnnotationTool
{
    class SelectiveBoxCanvas : IDisposable
    {
        private readonly Canvas _canvas;
        private ShapeRectangle _rectangle;
        public SelectiveBoxCanvas(Canvas canvas)
        {
            _canvas = canvas;
            _rectangle = new ShapeRectangle(_canvas);
            _rectangle.OnShapeChanged += RectangleOnOnShapeChanged;
            _canvas.SizeChanged += CanvasOnSizeChanged;
            _canvas.MouseDown += CanvasOnMouseDown;
        }

        private void RectangleOnOnShapeChanged(Rect shape)
        {
            OnShapeChanged?.Invoke(shape);
        }

        private void CanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size newSize = e.NewSize;
            _rectangle.FitToSize(newSize);
        }

        public void Create(Rect shape)
        {
            _rectangle.Destroy();
            _rectangle.Create(shape);
        }
        
        public delegate void ShapeChangedHandler(Rect shape);

        public event ShapeChangedHandler OnShapeChanged;

        private bool _statefulSizeChange;

        public void ShrinkTopBottom()
        {
            if (_statefulSizeChange)
                ShrinkTop();
            else
                ShrinkBottom();
            _statefulSizeChange = !_statefulSizeChange;
        }

        public void ShrinkTop()
        {
            _rectangle.ShrinkTop();
        }

        public void ShrinkBottom()
        {
            _rectangle.ShrinkBottom();
        }

        public void ShrinkLeftRight()
        {
            if (_statefulSizeChange)
                ShrinkLeft();
            else
                ShrinkRight();
            _statefulSizeChange = !_statefulSizeChange;
        }

        public void ShrinkLeft()
        {
            _rectangle.ShrinkLeft();
        }
        public void ShrinkRight()
        {
            _rectangle.ShrinkRight();
        }

        public void ExpandTopBottom()
        {
            if (_statefulSizeChange)
                ExpandTop();
            else
                ExpandBottom();
            _statefulSizeChange = !_statefulSizeChange;
        }

        public void ExpandTop()
        {
            _rectangle.ExpandTop();
        }

        public void ExpandBottom()
        {
            _rectangle.ExpandBottom();
        }

        public void ExpandLeftRight()
        {
            if (_statefulSizeChange)
                ExpandLeft();
            else
                ExpandRight();
            _statefulSizeChange = !_statefulSizeChange;
        }

        public void ExpandLeft()
        {
            _rectangle.ExpandLeft();
        }

        public void ExpandRight()
        {
            _rectangle.ExpandRight();
        }

        public void MoveLeft()
        {
            _rectangle.MoveLeft();
        }

        public void MoveRight()
        {
            _rectangle.MoveRight();
        }

        public void MoveUp()
        {
            _rectangle.MoveUp();
        }

        public void MoveDown()
        {
            _rectangle.MoveDown();
        }

        public Rect GetShape()
        {
            return _rectangle.GetShape();
        }

        public void SetShape(Rect shape)
        {
            if (!_rectangle.IsExists())
            {
                _rectangle.Create(shape);
                return;
            }

            _rectangle.SetShape(shape);
        }

        public bool IsExists()
        {
            return _rectangle.IsExists();
        }

        bool IsInValidResion(Point point)
        {
            if (point.X >= 0 && point.X < _canvas.Width && point.Y >= 0 && point.Y < _canvas.Height)
                return true;
            return false;
        }

        public void Clear()
        {
            _rectangle.Destroy();
        }

        private void SetShapeOnNewMousePosition(Point position)
        {
            double W = position.X - _initialPosition.X;
            double H = position.Y - _initialPosition.Y;
            if (W < 0 || H < 0)
                return;
            if (Convert.ToInt32(Math.Round(W)) == 0)
                W = 1;
            if (Convert.ToInt32(Math.Round(H)) == 0)
                H = 1;
            _rectangle.SetShape(new Rect(_initialPosition.X, _initialPosition.Y, W, H));
            return;
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(_canvas);
            if (!IsInValidResion(mousePosition))
                return;

            mousePosition.X = Math.Round(mousePosition.X);
            mousePosition.Y = Math.Round(mousePosition.Y);

            if (_inCreatingRectangle)
            {
                SetShapeOnNewMousePosition(mousePosition);
                return;
            }

            _rectangle.OnMouseMove(sender, e);
        }

        public void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(_canvas);
            if (!IsInValidResion(mousePosition))
                return;

            mousePosition.X = Math.Round(mousePosition.X);
            mousePosition.Y = Math.Round(mousePosition.Y);

            if (_inCreatingRectangle)
            {
                SetShapeOnNewMousePosition(mousePosition);

                _inCreatingRectangle = false;

                RectangleOnOnShapeChanged(_rectangle.GetShape());

                return;
            }

            _rectangle.OnMouseUp(sender, e);
        }

        private bool _inCreatingRectangle = false;
        private Point _initialPosition;

        private void CanvasOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(_canvas);
            if (!IsInValidResion(mousePosition))
                return;

            mousePosition.X = Math.Round(mousePosition.X);
            mousePosition.Y = Math.Round(mousePosition.Y);

            _rectangle.Destroy();
            _rectangle.Create(new Rect(mousePosition.X, mousePosition.Y, 0, 0));

            _initialPosition = mousePosition;
            _inCreatingRectangle = true;
        }

        public void Dispose()
        {
            _rectangle.Destroy();

            _canvas.MouseDown -= CanvasOnMouseDown;
            _canvas.SizeChanged -= CanvasOnSizeChanged;
        }
    }
}
