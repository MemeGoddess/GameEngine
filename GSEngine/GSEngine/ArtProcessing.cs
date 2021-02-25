using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSEngine
{
    public class Object3D
    {
        public Size Space;
        public Bitmap Rendered;
        public List<KeyValuePair<Polygon, int>> Polygons = new List<KeyValuePair<Polygon, int>>();
        /// <summary>
        /// Add Polygon to 3D Object
        /// </summary>
        /// <param name="zIndex">The ZIndex is relative to the Object, not World</param>
        public void AddPolygon(Polygon Polygon, int ZIndex)
        {
            Polygons.Add(new KeyValuePair<Polygon, int>(Polygon, ZIndex));
            SetSpace();
        }

        void SetSpace()
        {

        }

        public void RemovePolygon(Polygon Polygon)
        {
            foreach (KeyValuePair<Polygon, int> A in Polygons)
            {
                if (A.Key == Polygon)
                {
                    Polygons.Remove(A);
                }
            }
        }

        public void SortPolygons()
        {
            Polygons.Sort(delegate(KeyValuePair<Polygon, int> x, KeyValuePair<Polygon, int> y)
            {
                return x.Value.CompareTo(y.Value);
            });
        }
    }

    public class GraphicsInfo
    {
        List<KeyValuePair<Polygon, int>> Polygons = new List<KeyValuePair<Polygon, int>>();
        List<KeyValuePair<Object3D, int>> Objects3D = new List<KeyValuePair<Object3D, int>>();
        int Scale;
        GameView GameWindow;
        Boolean AA = true;
        Boolean Shade = true;
        Boolean PreRender = true;
        public GraphicsInfo(int Scale, GameView GameWindow)
        {
            this.Scale = Scale;
            this.GameWindow = GameWindow;
        }

        public void AddPolygon(Polygon Polygon, int ZIndex)
        {
            Polygons.Add(new KeyValuePair<Polygon, int>(Polygon, ZIndex));
        }

        public void AddObject3D(Object3D A, int ZIndex)
        {
            Objects3D.Add(new KeyValuePair<Object3D, int>(A, ZIndex));
        }

        public void RemovePolygon(Polygon Polygon)
        {
            foreach (KeyValuePair<Polygon, int> A in Polygons)
            {
                if (A.Key == Polygon)
                {
                    Polygons.Remove(A);
                }
            }
        }

        public void AntiAliasing(Boolean Enabled)
        {
            AA = Enabled;
        }

        public void Shading(Boolean Enabled)
        {
            Shade = Enabled;
        }

        public void PreRendering(Boolean Enabled)
        {
            PreRender = Enabled;
        }

        public void Render()
        {
            new Task(() =>
            {
                try
                {
                    List<KeyValuePair<Polygon, int>> TempList = new List<KeyValuePair<Polygon, int>>();
                    SortPolygons();
                    TempList = Polygons;
                    foreach (KeyValuePair<Object3D, int> B in Objects3D)
                    {
                        int Offset = B.Value;
                        foreach (KeyValuePair<Polygon, int> C in B.Key.Polygons)
                        {
                            TempList.Add(new KeyValuePair<Polygon, int>(C.Key, C.Value + Offset));
                        }
                    }
                    TempList.Sort(delegate(KeyValuePair<Polygon, int> x, KeyValuePair<Polygon, int> y)
                    {
                        return x.Value.CompareTo(y.Value);
                    });
                    Bitmap A = new Bitmap(GameWindow.Size.Width * Scale, GameWindow.Size.Height * Scale);
                    Graphics g = Graphics.FromImage(A);
                    if (AA)
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    }

                    foreach (KeyValuePair<Polygon, int> B in TempList)
                    {
                        Polygon C = B.Key;
                        if (C.PolyBitmap == null && C.Poly != null)
                        {
                            RenderFrame(g, C, new Pen(Color.Black));
                        }
                        else if (C.PolyBitmap != null)
                        {
                            RenderBitmap(g, C);
                        }
                    }
                    GameWindow.Game.Dispatcher.Invoke(() =>
                    {
                        GameWindow.SetImage(A);
                    });

                }
                catch (Exception E)
                {
                    System.Windows.Forms.MessageBox.Show(E.Message + " - " + E.TargetSite);
                }

            }).Start();
        }

        void SortPolygons()
        {
            Polygons.Sort(delegate(KeyValuePair<Polygon, int> x, KeyValuePair<Polygon, int> y)
            {
                return x.Value.CompareTo(y.Value);
            });
        }

        void RenderBitmap(Graphics g, Polygon A)
        {
            g.DrawImage(A.PolyBitmap, A.ReferencePoint);
        }

        void RenderFrame(Graphics g, Polygon A, Pen pen)
        {
            g.DrawPolygon(pen, A.Poly);
        }
    }

    public static class ArtProcessing
    {
        public static string NumToFace(int FaceNumber, Boolean StartWithCapital)
        {
            switch (FaceNumber)
            {
                case 0:
                    if (StartWithCapital)
                    {
                        return FirstCharToUpper("left");
                    }
                    return "left";
                case 1:
                    if (StartWithCapital)
                    {
                        return FirstCharToUpper("right");
                    }
                    return "right";
                case 2:
                    if (StartWithCapital)
                    {
                        return FirstCharToUpper("top");
                    }
                    return "top";
            }
            return "";
        }

        static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case "": throw new ArgumentException("Empty");
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        /// <summary> 
        /// SizeX and Z for Top face
        /// </summary>
        public static TextureBrush TileBrush(Bitmap Image, double SizeA, double SizeB, int Scale)
        {
            Bitmap Bit = new Bitmap(Convert.ToInt32(SizeA * 100 * Scale), Convert.ToInt32(SizeB * 100 * Scale));
            Graphics g = Graphics.FromImage(Bit);
            int RendersX = (int)Math.Ceiling(SizeA);
            int RendersY = (int)Math.Ceiling(SizeB);
            for (int x = 0; x < RendersX; x++)
            {
                for (int y = 0; y < RendersY; y++)
                {
                    g.DrawImage(Image, x * Scale * 100, y * Scale * 100, 100 * Scale, 100 * Scale);
                }
            }
            return new TextureBrush(Bit);
        }

        public static Bitmap RenderGrid(int Scale)
        {
            //Polygon Floor = new Polygon(5, 0.25, 5, 0.25, 0, Scale);
            //g.DrawPolygon(C, new Polygon(5, 0.25, 5, 0.25, 0, Scale).Poly);
            //g.DrawPolygon(C, Floor.Poly);

            //g.DrawPolygon(C, new Polygon(Floor.ReferencePoint, 1, 1, 1, 0, 0, Scale, new SolidBrush(Color.White), new SolidBrush(Color.White), new SolidBrush(Color.White)).Poly);
            //Polygon FilledCube = new Polygon(Floor.ReferencePoint, 1, 1, 1, 0, 0, Scale, new SolidBrush(Color.FromArgb(255, 0, 0)), new SolidBrush(Color.FromArgb(0, 150, 0)), new SolidBrush(Color.FromArgb(0, 0, 150)));

            //g.DrawPolygon(C, B.SquareOne.ToArray<Point>());
            //g.DrawPolygon(C, new Polygon(1, 1, 1, 1, 0, Scale).SquareOne.ToArray<Point>());
            //g.DrawPolygon(C, new Polygon(1, 1, 1, 2, 0, Scale).SquareOne.ToArray<Point>());
            Bitmap A = new Bitmap(1100 * Scale, 900 * Scale);
            Graphics g = Graphics.FromImage(A);
            Pen C = new Pen(Pens.Black.Brush, Scale);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Shader Wall = new Shader(Color.FromArgb(150, 150, 150));
            Polygon WallRight = new Polygon(5, 3, 0.25, Scale, 3.25, -.99, 0.22, Wall.ShadeBrush(1), Wall.ShadeBrush(0.5), Wall.ShadeBrush(0.6), true);
            g.DrawImage(WallRight.PolyBitmap, WallRight.ReferencePoint);

            Polygon WallLeft = new Polygon(0.25, 3, 5, Scale, 3.25, -.50, 0.45, Wall.ShadeBrush(0.5), Wall.ShadeBrush(.9), Wall.ShadeBrush(0.6), true);
            g.DrawImage(WallLeft.PolyBitmap, WallLeft.ReferencePoint);

            Polygon Floor = new Polygon(5, 0.25, 5, Scale, 3, -1, 0, new SolidBrush(Color.FromArgb(120, 72, 0)), new SolidBrush(Color.FromArgb(109, 65, 0)), new SolidBrush(Color.FromArgb(200, 200, 200)), true);
            g.DrawImage(Floor.PolyBitmap, Floor.ReferencePoint);

            Shader Shade = new Shader(Color.FromArgb(64, 201, 124));
            Polygon Server1 = new Polygon(Floor, 0.75, 0.25, 1, Scale, 0.125, 0, 0, new SolidBrush(Color.FromArgb(64, 201, 124)), false);
            g.DrawImage(Server1.PolyBitmap, Server1.ReferencePoint);

            //Polygon Sever2 = new Polygon(Server1, 0.75, 0.25, 1, Scale, 0, 0, 0.1, Shade.ShadeBrush(.90), Shade.ShadeBrush(.80), Shade.ShadeBrush(1));
            Polygon Sever2 = new Polygon(Server1, 0.75, 0.25, 1, Scale, 0, 0, 0.1, new SolidBrush(Color.FromArgb(64, 201, 124)), false);
            g.DrawImage(Sever2.PolyBitmap, Sever2.ReferencePoint);

            Polygon ShadedCube = new Polygon(Floor, 1, 1, 1, Scale, 0, 2, 0, new SolidBrush(Color.FromArgb(240, 240, 240)), false);
            g.DrawImage(ShadedCube.PolyBitmap, ShadedCube.ReferencePoint);

            GraphicsInfo B = new GraphicsInfo(Scale, null);
            /*Polygon FillFloor = new Polygon(5, 0.125, 5, 0.25, 0, Scale, new SolidBrush(Color.FromArgb(120, 72, 0)), new SolidBrush(Color.FromArgb(109, 65, 0)), new SolidBrush(Color.FromArgb(64, 201, 124)));
            g.DrawImage(FillFloor.PolyBitmap, FillFloor.ReferencePoint);
            Polygon FilledCube = new Polygon(FillFloor, 1, 1, 0.1, 0, 0, 0, Scale, new SolidBrush(Color.FromArgb(220, 220, 220)), new SolidBrush(Color.FromArgb(190, 190, 190)), new SolidBrush(Color.FromArgb(230, 230, 230)));
            g.DrawImage(FilledCube.PolyBitmap, FilledCube.ReferencePoint);
            Polygon FilledCubeSmall = new Polygon(FilledCube, 0.25, 0.30, 0.25, 0, 0, 0.75, Scale, new SolidBrush(Color.FromArgb(220, 220, 220)), new SolidBrush(Color.FromArgb(190, 190, 190)), new SolidBrush(Color.FromArgb(230, 230, 230)));
            g.DrawImage(FilledCubeSmall.PolyBitmap, FilledCubeSmall.ReferencePoint);
            g.DrawPolygon(C, FilledCube.Poly);*/
            return A;
        }

        public static double ToRadians(this double val)
        {
            return (Math.PI / 180) * val;
        }

        public static Point ReturnAnglePoint(Point Origin, double Angle, int Length)
        {
            double LengthX = Math.Cos(Angle);
            double LengthY = Math.Sin(Angle);
            return new Point(Convert.ToInt32(Origin.X + Math.Cos(Angle) * Length), Convert.ToInt32(Origin.Y + Math.Sin(Angle) * Length));
        }

        static Bitmap RotateBitmap(Bitmap bm, float angle)
        {
            // Make a Matrix to represent rotation
            // by this angle.
            Matrix rotate_at_origin = new Matrix();
            rotate_at_origin.Rotate(angle);

            // Rotate the image's corners to see how big
            // it will be after rotation.
            PointF[] points =
    {
        new PointF(0, 0),
        new PointF(bm.Width, 0),
        new PointF(bm.Width, bm.Height),
        new PointF(0, bm.Height),
    };
            rotate_at_origin.TransformPoints(points);
            float xmin, xmax, ymin, ymax;
            GetPointBounds(points, out xmin, out xmax,
                out ymin, out ymax);

            // Make a bitmap to hold the rotated result.
            int wid = (int)Math.Round(xmax - xmin);
            int hgt = (int)Math.Round(ymax - ymin);
            Bitmap result = new Bitmap(wid, hgt);

            // Create the real rotation transformation.
            Matrix rotate_at_center = new Matrix();
            rotate_at_center.RotateAt(angle,
                new PointF(wid / 2f, hgt / 2f));

            // Draw the image onto the new bitmap rotated.
            using (Graphics gr = Graphics.FromImage(result))
            {
                // Use smooth image interpolation.
                gr.InterpolationMode = InterpolationMode.High;

                // Clear with the color in the image's upper left corner.
                gr.Clear(Color.Transparent);

                //// For debugging. (It's easier to see the background.)
                //gr.Clear(Color.LightBlue);

                // Set up the transformation to rotate.
                gr.Transform = rotate_at_center;

                // Draw the image centered on the bitmap.
                int x = (wid - bm.Width) / 2;
                int y = (hgt - bm.Height) / 2;
                gr.DrawImage(bm, x, y);
            }

            // Return the result bitmap.
            return result;
        }

        static void GetPointBounds(PointF[] points,
    out float xmin, out float xmax,
    out float ymin, out float ymax)
        {
            xmin = points[0].X;
            xmax = xmin;
            ymin = points[0].Y;
            ymax = ymin;
            foreach (PointF point in points)
            {
                if (xmin > point.X) xmin = point.X;
                if (xmax < point.X) xmax = point.X;
                if (ymin > point.Y) ymin = point.Y;
                if (ymax < point.Y) ymax = point.Y;
            }
        }

        static Bitmap SquishHeight(Bitmap Bit)
        {
            return new Bitmap(Bit, new Size(Bit.Width, Bit.Height / 2));
        }

        static Bitmap SquishWidth(Bitmap Bit)
        {
            return new Bitmap(Bit, new Size(Bit.Width / 2, Bit.Height));
        }

        public static Bitmap TopFaceIso(Bitmap Bit)
        {
            return SquishHeight(RotateBitmap(Bit, 45));
        }

        public static Bitmap LeftFaceIso(Bitmap Bit)
        {
            return SquishWidth(RotateBitmap(Bit, 13));
        }
    }

    public class Shader
    {
        public Color OriginalColor;
        public Shader(Color OriginalColor)
        {
            this.OriginalColor = OriginalColor;
        }

        public Color Shade(double Percentage)
        {
            return Color.FromArgb(Convert.ToInt32(OriginalColor.R * Percentage), Convert.ToInt32(OriginalColor.G * Percentage), Convert.ToInt32(OriginalColor.B * Percentage));
        }

        public SolidBrush ShadeBrush(double Percentage)
        {
            return new SolidBrush(Color.FromArgb(Convert.ToInt32(OriginalColor.R * Percentage), Convert.ToInt32(OriginalColor.G * Percentage), Convert.ToInt32(OriginalColor.B * Percentage)));
        }
    }

    public class PolygonContainer
    {
        System.Windows.Shapes.Polygon LeftFace, RightFace, TopFace;
        public delegate void ClickEvent(object sender, ClickEventHandler e);
        public event ClickEvent LeftFace_Clicked, RightFace_Clicked, TopFace_Clicked;
        public PolygonContainer(System.Windows.Shapes.Polygon LeftFace, System.Windows.Shapes.Polygon RightFace, System.Windows.Shapes.Polygon TopFace)
        {
            this.LeftFace = LeftFace;
            this.RightFace = RightFace;
            this.TopFace = TopFace;
            LeftFace.MouseUp += LeftFace_MouseUp;
            RightFace.MouseUp += RightFace_MouseUp;
            TopFace.MouseUp += TopFace_MouseUp;
        }

        void TopFace_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TopFace != null)
            {
                TopFace_Clicked(this, new ClickEventHandler());
            }
        }

        void RightFace_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (RightFace != null)
            {
                RightFace_Clicked(this, new ClickEventHandler());
            }
        }

        void LeftFace_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LeftFace != null)
            {
                LeftFace_Clicked(this, new ClickEventHandler());
            }
        }
    }

    public class Polygon
    {
        public Point[] Poly;
        public Point ReferencePoint; // Where to Draw
        public Point PolyReferencePoint;
        public Point OffsetReferencePoint; // Left Face's Top Left Corner, Left Face's lowest Y value
        public double OffsetY;
        public double OffsetX;
        public Bitmap PolyBitmap;
        public Boolean PreRender = false;
        public Bitmap Rotated;
        public Point[] Hitbox;
        public Point[] HitboxTop;
        public Point[] LeftFacePoints;
        public Point[] RightFacePoints;
        public Point[] TopFacePoints;
        public Color TopBrush, LeftBrush, RightBrush;
        public TextureBrush TopTBrush, LeftTBrush, RightTBrush;
        public Size ContainerSize;

        //Poly Square
        public Polygon(double LengthX, double LengthY, double PosX, double PosY, double Scale)
        {
            Point Starting = new Point(Convert.ToInt32(100 * Scale), Convert.ToInt32(100 * Scale));
            if (PosX > 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(26.57), Convert.ToInt32((PosX * 100) * Scale));
            }
            if (PosY > 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(153.43), Convert.ToInt32((PosY * 100) * Scale));
            }
            Poly = DrawSquare(Starting, LengthX, LengthY, Scale).ToArray<Point>();
            ReferencePoint = Starting;
            OffsetX = PosX;
            OffsetY = PosY;
            //OffsetReferencePoint = new Point(Convert.ToInt32(ReferencePoint.X + (OffsetX * 100 * Scale)), Convert.ToInt32(ReferencePoint.Y + (OffsetY * 100 * Scale)));
        }
        //Poly Cube
        public Polygon(double LengthX, double LengthY, double LengthZ, double PosX, double PosY, double Scale)
        {
            Point Starting = new Point(Convert.ToInt32(100 * Scale), Convert.ToInt32(100 * Scale));
            if (PosX > 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(26.57), Convert.ToInt32((PosX * 100) * Scale));
            }
            if (PosY > 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(153.43), Convert.ToInt32((PosY * 100) * Scale));
            }
            Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(153.43), Convert.ToInt32((LengthZ * 100) * Scale));
            Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(0), Convert.ToInt32(((LengthZ - 1) * 100) * Scale));
            Poly = DrawCube(Starting, LengthX, LengthY, LengthZ, Scale).ToArray<Point>();
            OffsetX = PosX;
            OffsetY = PosY;
            //OffsetReferencePoint = new Point(Convert.ToInt32(ReferencePoint.X + (OffsetX * 100 * Scale)), Convert.ToInt32(ReferencePoint.Y + (OffsetY * 100 * Scale)));
        }
        //Poly Cube with Refernce
        public Polygon(Polygon Reference, double LengthX, double LengthY, double LengthZ, double PosX, double PosY, double Scale)
        {
            Point Starting = new Point(Convert.ToInt32(100 * Scale), Convert.ToInt32(100 * Scale));
            if (PosX > 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(26.57), Convert.ToInt32((PosX * 100) * Scale));
            }
            if (PosY > 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(153.43), Convert.ToInt32((PosY * 100) * Scale));
            }
            Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(153.43), Convert.ToInt32((LengthZ * 100) * Scale));
            Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(0), Convert.ToInt32(((LengthZ - 1) * 100) * Scale));
            Poly = DrawCube(new Point(Convert.ToInt32((-90 * Scale) + Reference.ReferencePoint.X + (Reference.OffsetX * 100 * Scale)), Convert.ToInt32((-55 * Scale) + Reference.ReferencePoint.Y + (Reference.OffsetX * 100 * Scale))), LengthX, LengthY, LengthZ, Scale).ToArray<Point>();
            OffsetX = PosX;
            OffsetY = PosY;
            //OffsetReferencePoint = new Point(Convert.ToInt32(ReferencePoint.X + (OffsetX * 100 * Scale)), Convert.ToInt32(ReferencePoint.Y + (OffsetY * 100 * Scale)));

        }

        /// <summary> 
        /// Automatically shades cubes
        /// </summary>
        public Polygon(Polygon Reference, double LengthX, double LengthY, double LengthZ, double Scale, double PosX, double PosY, double PosZ, Brush FaceBrush, Boolean Render)
        {
            Shader Shade = new Shader(new Pen(FaceBrush).Color);
            Point Starting = Reference.OffsetReferencePoint;
            if (PosX != 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(26.57), Convert.ToInt32((PosX * 100) * Scale));
            }
            if (PosY != 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(153.43), Convert.ToInt32((PosY * 100) * Scale));
            }
            if (PosZ != 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(270), Convert.ToInt32((PosZ * 100) * Scale));
            }
            Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(270), Convert.ToInt32(1 * 100 * Scale));
            if (Render)
            {
                PolyBitmap = FillCube(Starting, LengthX, LengthY, LengthZ, Scale, Shade.ShadeBrush(0.9), Shade.ShadeBrush(.8), Shade.ShadeBrush(1));
            }
            else
            {
                FillCubeNoRender(Starting, LengthX, LengthY, LengthZ, Scale, Shade.ShadeBrush(0.9), Shade.ShadeBrush(.8), Shade.ShadeBrush(1));
            }
        }


        //Cube with Reference
        public Polygon(Polygon Reference, double LengthX, double LengthY, double LengthZ, double Scale, double PosX, double PosY, double PosZ, Brush LeftFace, Brush RightFace, Brush TopFace, Boolean Render)
        {
            Point Starting = Reference.OffsetReferencePoint;
            if (PosX != 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(26.57), Convert.ToInt32((PosX * 100) * Scale));
            }
            if (PosY != 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(153.43), Convert.ToInt32((PosY * 100) * Scale));
            }
            if (PosZ != 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(270), Convert.ToInt32((PosZ * 100) * Scale));
            }
            Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(270), Convert.ToInt32(1 * 100 * Scale));
            if (Render)
            {
                PolyBitmap = FillCube(Starting, LengthX, LengthY, LengthZ, Scale, LeftFace, RightFace, TopFace);
            }
            else
            {
                FillCubeNoRender(Starting, LengthX, LengthY, LengthZ, Scale, LeftFace, RightFace, TopFace);
            }
        }

        //Cube
        public Polygon(double LengthX, double LengthY, double LengthZ, double Scale, double PosX, double PosY, double PosZ, Brush LeftFace, Brush RightFace, Brush TopFace, Boolean Render)
        {
            Point Starting = new Point(Convert.ToInt32(100 * Scale), Convert.ToInt32(100 * Scale));
            if (PosX != 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(26.57), Convert.ToInt32((PosX * 100) * Scale));
            }
            if (PosY != 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(153.43), Convert.ToInt32((PosY * 100) * Scale));
            }
            if (PosZ != 0)
            {
                Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(270), Convert.ToInt32((PosZ * 100) * Scale));
            }
            //Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(153.43), Convert.ToInt32((LengthZ * 100) * Scale));// Goes from Tip of Top face to Top Left of Left Face
            //Starting = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(0), Convert.ToInt32(((LengthZ - 1) * 100) * Scale));
            OffsetX = PosX;
            OffsetY = PosY;
            if (Render)
            {

                PolyBitmap = FillCube(Starting, LengthX, LengthY, LengthZ, Scale, LeftFace, RightFace, TopFace);
            }
            else
            {
                FillCubeNoRender(Starting, LengthX, LengthY, LengthZ, Scale, LeftFace, RightFace, TopFace);
            }
            //FillCubeNoRender(Starting, LengthX, LengthY, LengthZ, Scale, LeftFace, RightFace, TopFace);

            //Poly = DrawCube(Starting, LengthX, LengthY, LengthZ, Scale).ToArray<Point>();

            //OffsetReferencePoint = new Point(Convert.ToInt32(ReferencePoint.X + (OffsetX * 100 * Scale)), Convert.ToInt32(ReferencePoint.Y + (OffsetY * 100 * Scale)));
            //OffsetReferencePoint = new Point(Starting.X + Convert.ToInt32(100 * PosX * Scale), Starting.Y);
            //ReferencePoint = new Point(Convert.ToInt32(ReferencePoint.X + (PosX * 100)), Convert.ToInt32(ReferencePoint.Y + (PosY * 100)));
        }

        List<Point> DrawSquare(Point StartingPoint, double SizeX, double SizeY, double Scale)
        {
            List<Point> List = new List<Point>();
            List.Add(StartingPoint);
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(26.57), Convert.ToInt32((SizeX * 100) * Scale)));
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(153.43), Convert.ToInt32((SizeY * 100) * Scale)));
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(180 + 26.57), Convert.ToInt32((SizeX * 100) * Scale)));

            return List;
        }

        List<Point> DrawCube(Point StartingPoint, double SizeX, double SizeY, double SizeZ, double Scale)
        {
            List<Point> List = new List<Point>();
            List.Add(StartingPoint);
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(26.57), Convert.ToInt32((SizeX * 100) * Scale)));
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(90), Convert.ToInt32((SizeY * 100) * Scale)));
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(206.57), Convert.ToInt32((SizeX * 100) * Scale)));
            List.Add(StartingPoint);
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(333.43), Convert.ToInt32((SizeZ * 100) * Scale)));
            PolyReferencePoint = List.Last<Point>();
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(26.57), Convert.ToInt32((SizeX * 100) * Scale)));
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(90), Convert.ToInt32((SizeY * 100) * Scale)));
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(153.43), Convert.ToInt32((SizeZ * 100) * Scale)));
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(270), Convert.ToInt32((SizeY * 100) * Scale)));
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(333.43), Convert.ToInt32((SizeZ * 100) * Scale)));
            List.Add(ArtProcessing.ReturnAnglePoint(List.Last<Point>(), ArtProcessing.ToRadians(153.43), Convert.ToInt32((SizeZ * 100) * Scale)));
            return List;
        }

        void FillCubeNoRender(Point StartingPoint, double SizeX, double SizeY, double SizeZ, double Scale, Brush LeftFaceBrush, Brush RightFaceBrush, Brush TopFaceBrush)
        {

            //StartingPoint = ArtProcessing.ReturnAnglePoint(StartingPoint, ArtProcessing.ToRadians(26.57), Convert.ToInt32(1 * 100 * Scale));
            StartingPoint = ArtProcessing.ReturnAnglePoint(StartingPoint, ArtProcessing.ToRadians(90), Convert.ToInt32(1 * 100 * Scale));
            Point TopStartingPoint = ArtProcessing.ReturnAnglePoint(StartingPoint, ArtProcessing.ToRadians(270), Convert.ToInt32(SizeY * 100 * Scale));
            Point TopLeftCorner = TopStartingPoint;
            TopStartingPoint = ArtProcessing.ReturnAnglePoint(TopStartingPoint, ArtProcessing.ToRadians(333.43), Convert.ToInt32(1 * 100 * Scale));
            List<Point> TopFace = new List<Point>();
            TopFace.Add(TopStartingPoint);
            //Point TopLeftCorner = TopStartingPoint;
            int YOffset = TopStartingPoint.Y;
            TopFace.Add(ArtProcessing.ReturnAnglePoint(TopStartingPoint, ArtProcessing.ToRadians(26.57), Convert.ToInt32(SizeX * 100 * Scale)));
            int XEnd = TopFace.Last<Point>().X;
            TopFace.Add(ArtProcessing.ReturnAnglePoint(TopFace.Last<Point>(), ArtProcessing.ToRadians(153.43), Convert.ToInt32(SizeZ * 100 * Scale)));
            TopFace.Add(ArtProcessing.ReturnAnglePoint(TopFace.Last<Point>(), ArtProcessing.ToRadians(206.57), Convert.ToInt32(SizeX * 100 * Scale)));

            List<Point> LeftFace = new List<Point>();
            List<Point> RightFace = new List<Point>();
            LeftFace.Add(TopFace.Last<Point>());
            LeftFace.Add(ArtProcessing.ReturnAnglePoint(TopFace.Last<Point>(), ArtProcessing.ToRadians(26.57), Convert.ToInt32(SizeX * 100 * Scale)));
            RightFace.Add(LeftFace.Last<Point>());
            LeftFace.Add(ArtProcessing.ReturnAnglePoint(LeftFace.Last<Point>(), ArtProcessing.ToRadians(90), Convert.ToInt32(SizeY * 100 * Scale)));
            int YEnd = LeftFace.Last<Point>().Y;
            LeftFace.Add(ArtProcessing.ReturnAnglePoint(LeftFace.Last<Point>(), ArtProcessing.ToRadians(206.57), Convert.ToInt32(SizeX * 100 * Scale)));
            int XOffset = LeftFace.Last<Point>().X;

            RightFace.Add(ArtProcessing.ReturnAnglePoint(RightFace.Last<Point>(), ArtProcessing.ToRadians(333.43), Convert.ToInt32(SizeZ * 100 * Scale)));
            RightFace.Add(ArtProcessing.ReturnAnglePoint(RightFace.Last<Point>(), ArtProcessing.ToRadians(90), Convert.ToInt32(SizeY * 100 * Scale)));
            RightFace.Add(ArtProcessing.ReturnAnglePoint(RightFace.Last<Point>(), ArtProcessing.ToRadians(153.43), Convert.ToInt32(SizeZ * 100 * Scale)));

            for (int i = 0; i < RightFace.Count; i++)
            {
                RightFace[i] = new Point(RightFace[i].X - XOffset, RightFace[i].Y - YOffset);
            }
            for (int i = 0; i < LeftFace.Count; i++)
            {
                LeftFace[i] = new Point(LeftFace[i].X - XOffset, LeftFace[i].Y - YOffset);
            }
            for (int i = 0; i < TopFace.Count; i++)
            {
                TopFace[i] = new Point(TopFace[i].X - XOffset, TopFace[i].Y - YOffset);
            }
            LeftFacePoints = LeftFace.ToArray<Point>();
            RightFacePoints = RightFace.ToArray<Point>();
            TopFacePoints = TopFace.ToArray<Point>();

            if (TopFaceBrush.GetType() == typeof(TextureBrush))
            {
                TextureBrush Temp = (TextureBrush)TopFaceBrush;
                Bitmap Temp2 = new Bitmap(Temp.Image, Convert.ToInt32(SizeX * 100 * Scale + (107 * SizeX)), Convert.ToInt32(SizeZ * 100 * Scale + (107 * SizeZ)));
                TopTBrush = new TextureBrush(ArtProcessing.TopFaceIso(Temp2), WrapMode.Clamp);
            }
            else
            {
                TopBrush = new Pen(TopFaceBrush).Color;
            }

            if (LeftFaceBrush.GetType() == typeof(TextureBrush))
            {
                List<Point> LeftFaceDraw = new List<Point>(LeftFace);
                LeftFaceDraw.RemoveAt(2);
                int LowestSizeX = -1, LowestSizeY = -1;
                int SizeXA = 0, SizeYA = 0;
                foreach (Point PA in LeftFaceDraw)
                {
                    if (PA.X > SizeXA)
                    {
                        SizeXA = PA.X;
                    }
                    if (PA.Y > SizeYA)
                    {
                        SizeYA = PA.Y;
                    }
                    if (PA.X < LowestSizeX || LowestSizeX == -1)
                    {
                        LowestSizeX = PA.X;
                    }
                    if (PA.Y < LowestSizeY || LowestSizeY == -1)
                    {
                        LowestSizeY = PA.Y;
                    }
                }
                for (int i = 0; i < LeftFaceDraw.Count; i++)
                {
                    LeftFaceDraw[i] = new Point(LeftFaceDraw[i].X, LeftFaceDraw[i].Y - LowestSizeY);
                }
                LeftFaceDraw[1] = new Point(LeftFaceDraw[1].X, LeftFaceDraw[1].Y - 6);
                LeftFaceDraw[2] = new Point(LeftFaceDraw[2].X, LeftFaceDraw[2].Y + 6);
                TextureBrush Temp = (TextureBrush)LeftFaceBrush;
                Bitmap Temp2 = new Bitmap(SizeXA - LowestSizeX, Convert.ToInt32(SizeYA * (1.25 * SizeX) - LowestSizeY));
                Graphics G = Graphics.FromImage(Temp2);
                G.DrawImage(Temp.Image, LeftFaceDraw.ToArray<Point>());
                LeftTBrush = new TextureBrush(Temp2, WrapMode.Clamp);
            }
            else
            {
                this.LeftBrush = new Pen(LeftFaceBrush).Color;
            }

            if (RightFaceBrush.GetType() == typeof(TextureBrush))
            {
                List<Point> RightFaceDraw = new List<Point>(RightFace);
                RightFaceDraw.RemoveAt(2);
                int SizeXA = 0, SizeYA = 0;
                int LowestSizeX = -1, LowestSizeY = -1;
                foreach (Point PA in RightFaceDraw)
                {

                    if (PA.X < LowestSizeX || LowestSizeX == -1)
                    {
                        LowestSizeX = PA.X;
                    }
                    if (PA.Y < LowestSizeY || LowestSizeY == -1)
                    {
                        LowestSizeY = PA.Y;
                    }
                }
                for (int i = 0; i < RightFaceDraw.Count; i++)
                {
                    RightFaceDraw[i] = new Point(RightFaceDraw[i].X - LowestSizeX, RightFaceDraw[i].Y - LowestSizeY);
                }
                foreach (Point PA in RightFaceDraw)
                {
                    if (PA.X > SizeXA)
                    {
                        SizeXA = PA.X;
                    }
                    if (PA.Y > SizeYA)
                    {
                        SizeYA = PA.Y;
                    }
                }
                TextureBrush Temp = (TextureBrush)RightFaceBrush;
                Bitmap Temp2 = new Bitmap(SizeXA, Convert.ToInt32(SizeYA));
                Graphics G = Graphics.FromImage(Temp2);
                G.DrawImage(Temp.Image, RightFaceDraw.ToArray<Point>());
                RightTBrush = new TextureBrush(Temp2, WrapMode.Clamp);
            }
            else
            {
                RightBrush = new Pen(RightFaceBrush).Color;
            }

            Point RenderPoint = new Point(XOffset, YOffset);
            Size RenderSize = new Size(XEnd - XOffset, YEnd - YOffset);
            ContainerSize = RenderSize;
            OffsetReferencePoint = TopLeftCorner;
            ReferencePoint = RenderPoint;
        }

        Bitmap FillCube(Point StartingPoint, double SizeX, double SizeY, double SizeZ, double Scale, Brush LeftFaceBrush, Brush RightFaceBrush, Brush TopFaceBrush)
        {

            //StartingPoint = ArtProcessing.ReturnAnglePoint(StartingPoint, ArtProcessing.ToRadians(26.57), Convert.ToInt32(1 * 100 * Scale));
            StartingPoint = ArtProcessing.ReturnAnglePoint(StartingPoint, ArtProcessing.ToRadians(90), Convert.ToInt32(1 * 100 * Scale));
            Point TopStartingPoint = ArtProcessing.ReturnAnglePoint(StartingPoint, ArtProcessing.ToRadians(270), Convert.ToInt32(SizeY * 100 * Scale));
            Point TopLeftCorner = TopStartingPoint;
            TopStartingPoint = ArtProcessing.ReturnAnglePoint(TopStartingPoint, ArtProcessing.ToRadians(333.43), Convert.ToInt32(1 * 100 * Scale));
            List<Point> TopFace = new List<Point>();
            TopFace.Add(TopStartingPoint);
            //Point TopLeftCorner = TopStartingPoint;
            int YOffset = TopStartingPoint.Y;
            TopFace.Add(ArtProcessing.ReturnAnglePoint(TopStartingPoint, ArtProcessing.ToRadians(26.57), Convert.ToInt32(SizeX * 100 * Scale)));
            int XEnd = TopFace.Last<Point>().X;
            TopFace.Add(ArtProcessing.ReturnAnglePoint(TopFace.Last<Point>(), ArtProcessing.ToRadians(153.43), Convert.ToInt32(SizeZ * 100 * Scale)));
            TopFace.Add(ArtProcessing.ReturnAnglePoint(TopFace.Last<Point>(), ArtProcessing.ToRadians(206.57), Convert.ToInt32(SizeX * 100 * Scale)));

            List<Point> LeftFace = new List<Point>();
            List<Point> RightFace = new List<Point>();
            LeftFace.Add(TopFace.Last<Point>());
            LeftFace.Add(ArtProcessing.ReturnAnglePoint(TopFace.Last<Point>(), ArtProcessing.ToRadians(26.57), Convert.ToInt32(SizeX * 100 * Scale)));
            RightFace.Add(LeftFace.Last<Point>());
            LeftFace.Add(ArtProcessing.ReturnAnglePoint(LeftFace.Last<Point>(), ArtProcessing.ToRadians(90), Convert.ToInt32(SizeY * 100 * Scale)));
            int YEnd = LeftFace.Last<Point>().Y;
            LeftFace.Add(ArtProcessing.ReturnAnglePoint(LeftFace.Last<Point>(), ArtProcessing.ToRadians(206.57), Convert.ToInt32(SizeX * 100 * Scale)));
            int XOffset = LeftFace.Last<Point>().X;

            RightFace.Add(ArtProcessing.ReturnAnglePoint(RightFace.Last<Point>(), ArtProcessing.ToRadians(333.43), Convert.ToInt32(SizeZ * 100 * Scale)));
            RightFace.Add(ArtProcessing.ReturnAnglePoint(RightFace.Last<Point>(), ArtProcessing.ToRadians(90), Convert.ToInt32(SizeY * 100 * Scale)));
            RightFace.Add(ArtProcessing.ReturnAnglePoint(RightFace.Last<Point>(), ArtProcessing.ToRadians(153.43), Convert.ToInt32(SizeZ * 100 * Scale)));

            for (int i = 0; i < RightFace.Count; i++)
            {
                RightFace[i] = new Point(RightFace[i].X - XOffset, RightFace[i].Y - YOffset);
            }
            for (int i = 0; i < LeftFace.Count; i++)
            {
                LeftFace[i] = new Point(LeftFace[i].X - XOffset, LeftFace[i].Y - YOffset);
            }
            for (int i = 0; i < TopFace.Count; i++)
            {
                TopFace[i] = new Point(TopFace[i].X - XOffset, TopFace[i].Y - YOffset);
            }
            LeftFacePoints = LeftFace.ToArray<Point>();
            RightFacePoints = RightFace.ToArray<Point>();
            TopFacePoints = TopFace.ToArray<Point>();

            if (TopFaceBrush.GetType() == typeof(TextureBrush))
            {
                TextureBrush Temp = (TextureBrush)TopFaceBrush;
                Bitmap Temp2 = new Bitmap(Temp.Image, Convert.ToInt32(SizeX * 100 * Scale + (107 * SizeX)), Convert.ToInt32(SizeZ * 100 * Scale + (107 * SizeZ)));
                TopTBrush = new TextureBrush(ArtProcessing.TopFaceIso(Temp2), WrapMode.Clamp);
            }
            else
            {
                TopBrush = new Pen(TopFaceBrush).Color;
            }

            if (LeftFaceBrush.GetType() == typeof(TextureBrush))
            {
                List<Point> LeftFaceDraw = new List<Point>(LeftFace);
                LeftFaceDraw.RemoveAt(2);
                int LowestSizeX = -1, LowestSizeY = -1;
                int SizeXA = 0, SizeYA = 0;
                foreach (Point PA in LeftFaceDraw)
                {
                    if (PA.X > SizeXA)
                    {
                        SizeXA = PA.X;
                    }
                    if (PA.Y > SizeYA)
                    {
                        SizeYA = PA.Y;
                    }
                }
                TextureBrush Temp = (TextureBrush)LeftFaceBrush;
                Bitmap Temp2 = new Bitmap(SizeXA - LowestSizeX, Convert.ToInt32(SizeYA * (1.25 * SizeX) - LowestSizeY));
                Graphics G = Graphics.FromImage(Temp2);
                G.DrawImage(Temp.Image, LeftFaceDraw.ToArray<Point>());
                LeftTBrush = new TextureBrush(Temp2, WrapMode.Clamp);
            }
            else
            {
                this.LeftBrush = new Pen(LeftFaceBrush).Color;
            }

            if (RightFaceBrush.GetType() == typeof(TextureBrush))
            {
                List<Point> RightFaceDraw = new List<Point>(RightFace);
                RightFaceDraw.RemoveAt(2);
                int SizeXA = 0, SizeYA = 0;
                foreach (Point PA in RightFaceDraw)
                {
                    if (PA.X > SizeXA)
                    {
                        SizeXA = PA.X;
                    }
                    if (PA.Y > SizeYA)
                    {
                        SizeYA = PA.Y;
                    }
                }
                TextureBrush Temp = (TextureBrush)RightFaceBrush;
                Bitmap Temp2 = new Bitmap(SizeXA, Convert.ToInt32(SizeYA));
                Graphics G = Graphics.FromImage(Temp2);
                G.DrawImage(Temp.Image, RightFaceDraw.ToArray<Point>());
                RightTBrush = new TextureBrush(Temp2, WrapMode.Clamp);
            }
            else
            {
                RightBrush = new Pen(RightFaceBrush).Color;
            }

            Point RenderPoint = new Point(XOffset, YOffset);
            Size RenderSize = new Size(XEnd - XOffset, YEnd - YOffset);
            ContainerSize = RenderSize;
            OffsetReferencePoint = TopLeftCorner;
            ReferencePoint = RenderPoint;

            Bitmap Bit = new Bitmap(RenderSize.Width, RenderSize.Height);
            Graphics g = Graphics.FromImage(Bit);
            if (LeftTBrush != null)
            {
                g.FillPolygon(LeftTBrush, LeftFace.ToArray<Point>());
            }
            else
            {
                g.FillPolygon(LeftFaceBrush, LeftFace.ToArray<Point>());
            }
            if (RightTBrush != null)
            {
                g.FillPolygon(RightTBrush, RightFace.ToArray<Point>());
            }
            else
            {
                g.FillPolygon(RightFaceBrush, RightFace.ToArray<Point>());
            }


            if (TopTBrush != null)
            {
                g.FillPolygon(TopTBrush, TopFace.ToArray<Point>());
            }
            else
            {
                g.FillPolygon(TopFaceBrush, TopFace.ToArray<Point>());
            }

            return Bit;
        }

        public Bitmap Rotate()
        {
            throw new NotImplementedException();
        }

        //Not happy with this code, redoing it from the start of top left of cube
        /*
        Bitmap FillCube(Point StartingPoint, double SizeX, double SizeY, double SizeZ, double Scale, Brush LeftFaceBrush, Brush RightFaceBrush, Brush TopFaceBrush)
        {
            List<Point> LeftFace = new List<Point>();
            LeftFace.Add(StartingPoint);
            int RenderXStart = StartingPoint.X;
            LeftFace.Add(ArtProcessing.ReturnAnglePoint(LeftFace.Last<Point>(), ArtProcessing.ToRadians(26.57), Convert.ToInt32((SizeX * 100.1) * Scale)));
            LeftFace.Add(ArtProcessing.ReturnAnglePoint(LeftFace.Last<Point>(), ArtProcessing.ToRadians(90), Convert.ToInt32((SizeY * 100) * Scale)));
            int RenderY = LeftFace.Last<Point>().Y;
            LeftFace.Add(ArtProcessing.ReturnAnglePoint(LeftFace.Last<Point>(), ArtProcessing.ToRadians(206.57), Convert.ToInt32((SizeX * 100.1) * Scale)));

            List<Point> TopFace = new List<Point>();
            TopFace.Add(StartingPoint);
            TopFace.Add(ArtProcessing.ReturnAnglePoint(TopFace.Last<Point>(), ArtProcessing.ToRadians(333.47), Convert.ToInt32((SizeZ * 100) * Scale)));
            int RenderYStart = TopFace.Last<Point>().Y;
            
            TopFace.Add(ArtProcessing.ReturnAnglePoint(TopFace.Last<Point>(), ArtProcessing.ToRadians(26.57), Convert.ToInt32((SizeX * 100) * Scale)));
            int RenderX = TopFace.Last<Point>().X;
            Point RightFaceReference = TopFace.Last<Point>();
            TopFace.Add(ArtProcessing.ReturnAnglePoint(TopFace.Last<Point>(), ArtProcessing.ToRadians(153.43), Convert.ToInt32((SizeZ * 100) * Scale)));

            List<Point> RightFace = new List<Point>();
            RightFace.Add(RightFaceReference);
            RightFace.Add(ArtProcessing.ReturnAnglePoint(RightFace.Last<Point>(), ArtProcessing.ToRadians(90), Convert.ToInt32((SizeY * 100) * Scale)));
            RightFace.Add(ArtProcessing.ReturnAnglePoint(RightFace.Last<Point>(), ArtProcessing.ToRadians(153.43), Convert.ToInt32((SizeZ * 100) * Scale)));
            RightFace.Add(ArtProcessing.ReturnAnglePoint(RightFace.Last<Point>(), ArtProcessing.ToRadians(270), Convert.ToInt32((SizeY * 100) * Scale)));
            OffsetReferencePoint = TopFace[1];
            for (int i = 0; i < RightFace.Count; i++)
            {
                RightFace[i] = new Point(RightFace[i].X - RenderXStart, RightFace[i].Y - RenderYStart);
            }
            for (int i = 0; i < LeftFace.Count; i++)
            {
                LeftFace[i] = new Point(LeftFace[i].X - RenderXStart, LeftFace[i].Y - RenderYStart);
            }
            for (int i = 0; i < TopFace.Count; i++)
            {
                TopFace[i] = new Point(TopFace[i].X - RenderXStart, TopFace[i].Y - RenderYStart);
            }
            Bitmap Bit = new Bitmap(RenderX - RenderXStart, RenderY - RenderYStart);
            Graphics g = Graphics.FromImage(Bit);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //g.FillRectangle(new SolidBrush(Color.Pink), new Rectangle(0, 0, RenderX, RenderY));
            g.FillPolygon(LeftFaceBrush, LeftFace.ToArray<Point>());
            g.FillPolygon(RightFaceBrush, RightFace.ToArray<Point>());
            g.FillPolygon(TopFaceBrush, TopFace.ToArray<Point>());
            ReferencePoint = new Point(RenderXStart, RenderYStart);
            
            return Bit;
        }*/
    }
}
