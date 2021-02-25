using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using OpenTK.Input;

namespace GSEngine.OpenGL
{
    #region Containers
    public enum RenderStyle
    {
        Normal,
        Isometric
    }
    public class SolidColorBrush
    {
        public Color Color = Color.FromArgb(255, 255, 255, 255);
        public double Opacity = 1;

        public SolidColorBrush()
        {

        }

        public SolidColorBrush(Color color)
        {
            Color = color;
        }
    }
    public class Thickness
    {
        public double Bottom = 1, Left = 1, Top = 1, Right = 1;
        public Thickness()
        {

        }

        public Thickness(double uniformLength)
        {
            Bottom = uniformLength;
            Left = uniformLength;
            Top = uniformLength;
            Right = uniformLength;
        }

        public Thickness(double left, double top, double right, double bottom)
        {
            Bottom = bottom;
            Left = left;
            Top = top;
            Right = right;
        }
    }
    #endregion

    #region Shapes
    public abstract class Shape
    {
        public GSList<Shape> Parent;
        public double Width, Height, X, Y;
        public Rectangle Rect { get { return new Rectangle((int)X, (int)Y, (int)Width, (int)Height); } }
        public Color Color = Color.FromArgb(255, 255, 255, 255);
        public SolidColorBrush BorderBrush = new SolidColorBrush();
        public Thickness BorderThickness = new Thickness();
        public RenderStyle RenderStyle = RenderStyle.Normal;
        public delegate void ClickHandler(object Sender, MouseEventArgs e);
        public delegate void MouseHandler(object Sender, MouseMoveEventArgs e);
        public event ClickHandler MouseLeftClick;
        public event ClickHandler MouseRightClick;
        public event MouseHandler MouseEnter;
        public event MouseHandler MouseLeave;


        public virtual PointD[] GetPoints()
        {
            return new PointD[]
            {
                new PointD(X, Y),
                new PointD(X + Width, Y),
                new PointD(X + Width, Y + Height),
                new PointD(X, Y + Height),
            };
        }

        public virtual PointD GetMiddleOffset()
        {
            PointD[] points = GetPoints();
            double HighXa = points.OrderBy(x => x.X).LastOrDefault().X;
            double HighYa = points.OrderBy(x => x.X).LastOrDefault().Y;
            double OffsetRenderXa = HighXa / 2.0;
            double OffsetRenderYa = HighYa / 2.0;
            return new PointD(OffsetRenderXa, OffsetRenderYa);
        }

        public void _invokeclick(int Button, MouseButtonEventArgs e)
        {
            switch (Button)
            {
                case 0:
                    MouseLeftClick?.Invoke(this, e);
                    break;
                case 1:
                    MouseRightClick?.Invoke(this, e);
                    break;
            }
        }

        public void _invokehover(Boolean IN, MouseMoveEventArgs e)
        {
            if (IN)
            {
                MouseEnter?.Invoke(this, e);
            }
            else
            {
                MouseLeave?.Invoke(this, e);
            }
        }

        public virtual void BeginAnimation(PropertyInfo dp, Animation animation)
        {

        }
    }



    public class Square : Shape
    {

        public Square(double X, double Y, double Width, double Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }

        public Square(double X, double Y, double Width, double Height, Color Color)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
            this.Color = Color;
        }

        public Square(Rectangle Rectangle)
        {
            this.Width = Rectangle.Width;
            this.Height = Rectangle.Height;
            this.X = Rectangle.X;
            this.Y = Rectangle.Y;
        }

        public Square(Rectangle Rectangle, Color Color)
        {
            this.Width = Rectangle.Width;
            this.Height = Rectangle.Height;
            this.X = Rectangle.X;
            this.Y = Rectangle.Y;
            this.Color = Color;
        }

        public override PointD[] GetPoints()
        {
            return new PointD[]
            {
                new PointD(X, Y),
                new PointD(X, Y + Height),
                new PointD(X + Width, Y + Height),
                new PointD(X + Width, Y),
            };
        }

        public override PointD GetMiddleOffset()
        {
            PointD[] points = GetPoints();
            double HighXa = points.OrderBy(x => x.X).LastOrDefault().X;
            double HighYa = points.OrderBy(x => x.X).LastOrDefault().Y;
            double OffsetRenderXa = HighXa / 2.0;
            double OffsetRenderYa = HighYa / 2.0;
            return new PointD(OffsetRenderXa, OffsetRenderYa);
        }

        public Square()
        {
            this.X = 0;
            this.Y = 0;
            this.Width = 100;
            this.Height = 100;
        }
    }

    public class Polygon : Shape
    {
        public PointD[] Points;
        double OriginalX = 0, OriginalY;
        //Color Color = Color.FromArgb(255, 255, 255, 255);

        public Polygon(PointD[] Points)
        {
            this.Points = Points;
            X = GetPoints().Select(y => y.X).OrderBy(x => x).FirstOrDefault();
            Y = GetPoints().Select(y => y.Y).OrderBy(x => x).FirstOrDefault();
            OriginalX = X;
            OriginalY = Y;
        }

        public Polygon(PointD[] Points, Color Color)
        {
            this.Points = Points;
            this.Color = Color;
            X = GetPoints().Select(y => y.X).OrderBy(x => x).FirstOrDefault();
            Y = GetPoints().Select(y => y.Y).OrderBy(x => x).FirstOrDefault();
            OriginalX = X;
            OriginalY = Y;
        }

        public override PointD[] GetPoints()
        {

            return Points.Select(x => new PointD(x.X + (X - OriginalX), x.Y + (X - OriginalX))).ToArray();

        }

        public override PointD GetMiddleOffset()
        {
            PointD[] points = GetPoints();
            double HighXa = points.OrderBy(x => x.X).LastOrDefault().X;
            double HighYa = points.OrderBy(x => x.X).LastOrDefault().Y;
            double OffsetRenderXa = HighXa / 2.0;
            double OffsetRenderYa = HighYa / 2.0;
            return new PointD(OffsetRenderXa, OffsetRenderYa);
        }



    }

    public class PolygonBitmap : Shape
    {
        //TODO Implement PolygonBitmap for optimizations
    }



    public class Polygon3D : Shape
    {
        public List<Polygon> Polygons = new List<Polygon>();
        public PolygonRenderer polygonRenderer;
        public Polygon3D(GSEngine.PolygonRenderer Polygon, RenderStyle renderStyle)
        {
            SetThings(Polygon, renderStyle);
            polygonRenderer = Polygon;
        }

        public Polygon3D(double LengthX, double LengthY, double LengthZ, double Scale, double PosX, double PosY, double PosZ, Brush LeftFace, Brush RightFace, Brush TopFace, RenderStyle renderStyle)
        {
            PolygonRenderer render = new PolygonRenderer(LengthX, LengthY, LengthZ, Scale, PosX, PosY, PosZ, LeftFace, RightFace, TopFace, false);
            polygonRenderer = render;
            SetThings(render, renderStyle);
        }

        public Polygon3D(double LengthX, double LengthY, double LengthZ, double Scale, double PosX, double PosY, double PosZ, Color color, RenderStyle renderStyle)
        {
            Shader shade = new Shader(color);
            PolygonRenderer render = new PolygonRenderer(LengthX, LengthY, LengthZ, Scale, PosX, PosY, PosZ, shade.ShadeBrush(1), shade.ShadeBrush(0.9), shade.ShadeBrush(0.8), false);
            polygonRenderer = render;
        }

        void SetThings(PolygonRenderer render, RenderStyle renderStyle)
        {
            Polygons.Add(new Polygon(render.LeftFacePoints.Select(x => new PointD(x.X, x.Y)).ToArray(), render.LeftBrush) { RenderStyle = renderStyle });
            Polygons.Add(new Polygon(render.RightFacePoints.Select(x => new PointD(x.X, x.Y)).ToArray(), render.RightBrush) { RenderStyle = renderStyle });
            Polygons.Add(new Polygon(render.TopFacePoints.Select(x => new PointD(x.X, x.Y)).ToArray(), render.TopBrush) { RenderStyle = renderStyle });
            X = Polygons.SelectMany(x => x.GetPoints().Select(y => y.X)).OrderBy(x => x).FirstOrDefault();
            Y = Polygons.SelectMany(x => x.GetPoints().Select(y => y.Y)).OrderBy(x => x).FirstOrDefault();
        }


        public override PointD GetMiddleOffset()
        {
            double HighX = Polygons.SelectMany(x => x.GetPoints().Select(y => y.X)).OrderBy(x => x).LastOrDefault();
            double HighY = Polygons.SelectMany(x => x.GetPoints().Select(y => y.Y)).OrderBy(x => x).LastOrDefault();
            //double HighY = (a as Polygon3D).Polygons.OrderBy(x => x.Y).LastOrDefault().Y;
            double OffsetRenderX = HighX / 2.0;
            double OffsetRenderY = HighY / 2.0;
            return new PointD(OffsetRenderX, OffsetRenderY);
        }


    }

    public class PointD
    {
        public double X, Y;

        public PointD(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    public class RenderRegion
    {
        public Shape shape;
        public PointD[] Points;

    }
    #endregion

    #region Animations
    public abstract class Animation { }

    public class DoubleAnimation : Animation
    {
        public Nullable<double> From;
        public Nullable<double> To;
        public TimeSpan Duration;
        public PropertyInfo TargetPropertyType;
        public object[] Timeline { get; }
        object[] timeline;
    }
    #endregion

    #region Controls
    public class UIObject : Square
    {
        public List<VertexColored> _DrawPoints = new List<VertexColored>();
        public Size Size
        {
            get
            {
                return Rect.Size;
            }
            set
            {
                Width = value.Width;
                Height = value.Height;
                _DrawNewPoints();
            }
        }
        public Point Location
        {
            get
            {
                return Rect.Location;
            }
            set
            {
                X = value.X;
                Y = value.Y;
                _DrawNewPoints();
            }
        }
        public Brush Background = new LinearGradientBrush(new Point(0, 0), new Point(0, 0), Color.FromArgb(200, 200, 200), Color.FromArgb(150, 150, 150));

        public virtual void _DrawNewPoints()
        {
            _DrawPoints = new List<VertexColored>
            {
                new VertexColored { Point = new PointD(0,0)},
                new VertexColored { Point = new PointD(Width,0)},
                new VertexColored { Point = new PointD(Width,Height), Color = Color.Red},
                new VertexColored { Point = new PointD(0,Height), Color = Color.Red},
            };
        }

        public UIObject()
        {
            _DrawNewPoints();
        }
    }


    public class Button : UIObject
    {
        public Color BackColor { get { return Color; } set { Color = value; _DrawNewPoints(); } }
        public Color BackColour { get { return Color; } set { Color = value; _DrawNewPoints(); } }


        public Button()
        {
            Color = Color.FromArgb(255, 247, 246);
            Size = new Size(100, 30);

        }

        public override void _DrawNewPoints()
        {
            if (true)
            {
                _DrawPoints = new List<VertexColored>
            {
                new VertexColored{ Point = new PointD(0, 0) },
                new VertexColored{ Point = new PointD(Width, 0) },
                    new VertexColored{ Point = new PointD(Width, Height * 0.5) },
                    new VertexColored{ Point = new PointD(0, Height * 0.5) },

                new VertexColored{ Point = new PointD(0, Height), Color = Color.FromArgb(214,214,214) },
                new VertexColored{ Point = new PointD(Width, Height), Color = Color.FromArgb(214,214,214) },
                    //new VertexColored{ Point = new PointD(0, Height * 0.5) },
                    new VertexColored{ Point = new PointD(Width, Height * 0.5), Color = Color.FromArgb(214,214,214) },
                    new VertexColored{ Point = new PointD(0, Height * 0.5), Color = Color.FromArgb(214,214,214) },
            };
            }
            else
            {
                _DrawPoints = new List<VertexColored>
            {
                new VertexColored{ Point = new PointD(0, 0) },
                new VertexColored{ Point = new PointD(Width, 0) },
                    new VertexColored{ Point = new PointD(Width, Height * 0.75) },
                    new VertexColored{ Point = new PointD(0, Height * 0.75) },

                new VertexColored{ Point = new PointD(0, Height), Color = Color.FromArgb(100,100,100) },
                new VertexColored{ Point = new PointD(Width, Height), Color = Color.FromArgb(100,100,100) },
                    //new VertexColored{ Point = new PointD(0, Height * 0.5) },
                    new VertexColored{ Point = new PointD(Width, Height * 0.75) },
                    new VertexColored{ Point = new PointD(0, Height * 0.75) },
            };
            }

        }
    }
    #endregion

    public class VertexColored
    {
        public PointD Point;
        public Color Color = Color.Empty;
    }
}
