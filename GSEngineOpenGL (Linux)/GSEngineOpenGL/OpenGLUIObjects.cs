using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GSEngine.OpenGL
{
    public class GameView
    {
        public int Width
        {
            get
            {
                return game.Width;
            }
            set
            {
                game.Width = value;
            }
        }
        public int Height
        {
            get
            {
                return game.Height;
            }
            set
            {
                game.Height = value;
            }
        }
        public static Thread mainTask;
        public GSList<Shape> Children = new GSList<Shape>();
        List<KeyValuePair<Polygon, int>> Polygons = new List<KeyValuePair<Polygon, int>>();
        GameWindow game;
        int Frames = 0;
        Bitmap FPSImage = new Bitmap(32, 32);
        static double NormOffsetRenderX;
        static double NormOffsetRenderY;
        Shape shapeMouseDown;
        Shape shapeMouseIn;
        Stopwatch FrameTimer = new Stopwatch();
        public Color BackColor
        {
            get
            {
                return backcolor;
            }
            set
            {
                backcolor = value;
            }
        }
        Color backcolor = Color.FromArgb(0, 0, 0);
        public delegate void GameWindowEvent(object sender);
        public event GameWindowEvent WindowClosed;
        public GameView(int SizeX, int SizeY, int Scale, int AntiAlising, string WindowText)
        {
            Polygon3D Dumby = new Polygon3D(
                LengthX: 0,
                LengthY: 0,
                LengthZ: 0,
                Scale: 2,
                PosX: 0,
                PosY: 0,
                PosZ: 0,
                LeftFace: new SolidBrush(Color.FromArgb(255, 255, 0, 0)),
                RightFace: new SolidBrush(Color.FromArgb(255, 0, 255, 0)),
                TopFace: new SolidBrush(Color.FromArgb(255, 0, 0, 255)),
                renderStyle: RenderStyle.Isometric);
            Children.Add(Dumby);
            mainTask = new Thread(() => Start(SizeX, SizeY, Scale, AntiAlising, WindowText));

            mainTask.Start();

        }

        //TODO Background graphics rendering
        //TODO Apply background
        //TODO Animations
        //TODO Loading screen
        void Start(int SizeX, int SizeY, int Scale, int AntiAlising, string WindowText)
        {
            game = new GameWindow(SizeX, SizeY, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8, 8, 8, 0), 24, 8, AntiAlising), WindowText);
            game.Closed += (sender, e) => { WindowClosed?.Invoke(this); };
            //game.MouseWheel += Game_MouseWheel;
            game.Load += Game_Load;
            game.Resize += Game_Resize;
            game.RenderFrame += Game_RenderFrame;
            game.MouseDown += Game_MouseDown;
            game.MouseUp += Game_MouseUp;
            game.MouseMove += Game_MouseMove;
            game.UpdateFrame += Game_UpdateFrame;
            NormOffsetRenderX = game.Width / 2;
            NormOffsetRenderY = game.Height / 4 * 3;
            Children.OnAdd += Children_OnAdd;
            game.WindowBorder = WindowBorder.Hidden;
            game.Run(1.0 / 60.0);

        }


        void Game_MouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            PointD MousePos = new PointD(e.X, e.Y);
            List<Shape> Shapes2D = Children.Where(x => x.GetType().ToString() != "GSEngine.OpenGL.Polygon3D").ToList();
            List<Shape> Shapes3D = Children.Where(x => x.GetType().ToString() == "GSEngine.OpenGL.Polygon3D").ToList();
            List<RenderRegion> Regions = Shapes2D.Select(x => new RenderRegion { shape = x, Points = x.GetPoints().Select(y => new PointD(y.X + GetOffsetX.Invoke(x), y.Y + GetOffsetY.Invoke(x))).ToArray() }).ToList();
            Shapes3D.ForEach(x => (x as Polygon3D).Polygons.ForEach(y => Regions.Add(new RenderRegion { shape = x, Points = y.GetPoints().Select(u => new PointD(u.X + GetOffsetX.Invoke(x), u.Y + GetOffsetY.Invoke(x))).ToArray() })));
            RenderRegion Clicked = Regions.LastOrDefault(x => IsPointInPolygon(x.Points, MousePos));
            if (shapeMouseIn != Clicked?.shape)
            {
                Clicked?.shape._invokehover(true, e);
                shapeMouseIn?._invokehover(false, e);
                shapeMouseIn = Clicked?.shape;
            }
        }

        void Game_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            PointD MousePos = new PointD(e.X, e.Y);
            switch (e.Button)
            {
                case OpenTK.Input.MouseButton.Left:
                    List<Shape> Shapes2D = Children.Where(x => x.GetType().ToString() != "GSEngine.OpenGL.Polygon3D").ToList();
                    List<Shape> Shapes3D = Children.Where(x => x.GetType().ToString() == "GSEngine.OpenGL.Polygon3D").ToList();
                    //Creates a RenderRegion with shape = original shape, then get it's points and applies the potentional Isometric offset
                    List<RenderRegion> Regions = Shapes2D.Select(x => new RenderRegion { shape = x, Points = x.GetPoints().Select(y => new PointD(y.X + GetOffsetX.Invoke(x), y.Y + GetOffsetY.Invoke(x))).ToArray() }).ToList();
                    //Creates a RenderRegion for each face of the Polygon, setting the original Polygon as the shape, then adds a RenderRegion for each face + the offset
                    Shapes3D.ForEach(x => (x as Polygon3D).Polygons.ForEach(y => Regions.Add(new RenderRegion { shape = x, Points = y.GetPoints().Select(u => new PointD(u.X + GetOffsetX.Invoke(x), u.Y + GetOffsetY.Invoke(x))).ToArray() })));
                    RenderRegion Clicked = Regions.LastOrDefault(x => IsPointInPolygon(x.Points, MousePos));

                    shapeMouseDown = Clicked?.shape;
                    Console.WriteLine(Clicked == null ? "false" : "true - " + Clicked.shape.GetType());
                    break;
            }
        }

        void Game_MouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            PointD MousePos = new PointD(e.X, e.Y);
            switch (e.Button)
            {
                case OpenTK.Input.MouseButton.Left:
                    if (shapeMouseDown != null)
                    {
                        List<Shape> Shapes2D = Children.Where(x => x.GetType().ToString() != "GSEngine.OpenGL.Polygon3D").ToList();
                        List<Shape> Shapes3D = Children.Where(x => x.GetType().ToString() == "GSEngine.OpenGL.Polygon3D").ToList();
                        //Creates a RenderRegion with shape = original shape, then get it's points and applies the potentional Isometric offset
                        List<RenderRegion> Regions = Shapes2D.Select(x => new RenderRegion { shape = x, Points = x.GetPoints().Select(y => new PointD(y.X + GetOffsetX.Invoke(x), y.Y + GetOffsetY.Invoke(x))).ToArray() }).ToList();
                        //Creates a RenderRegion for each face of the Polygon, setting the original Polygon as the shape, then adds a RenderRegion for each face + the offset
                        Shapes3D.ForEach(x => (x as Polygon3D).Polygons.ForEach(y => Regions.Add(new RenderRegion { shape = x, Points = y.GetPoints().Select(u => new PointD(u.X + GetOffsetX.Invoke(x), u.Y + GetOffsetY.Invoke(x))).ToArray() })));
                        RenderRegion Clicked = Regions.LastOrDefault(x => IsPointInPolygon(x.Points, MousePos));
                        if (shapeMouseDown == Clicked?.shape) //If Clicked != null and Clicked.shape == shapeMouseDown
                        {
                            shapeMouseDown._invokeclick(0, e);
                        }
                        shapeMouseDown = null;
                    }
                    break;
            }
        }
        //Returns an offset of 0 or Isometric offset, depending on the RenderStyle
        Func<Shape, double> GetOffsetX = x => x.RenderStyle == RenderStyle.Isometric ? NormOffsetRenderX - x.GetMiddleOffset().X : 0;
        Func<Shape, double> GetOffsetY = x => x.RenderStyle == RenderStyle.Isometric ? NormOffsetRenderY - x.GetMiddleOffset().Y : 0;

        //List<KeyValuePair<Polygon, int>> Generate 


        void Game_Load(object sender, EventArgs e)
        {
            GL.ClearColor(200, 200, 200, 1);
            FrameTimer.Start();
        }

        void Game_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, game.Width, game.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0 - (game.Width / 2), game.Width / 2, game.Height / 4, 0 - (game.Height / 4 * 3), -1.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        void Game_UpdateFrame(object sender, FrameEventArgs e)
        {

            if (FrameTimer.ElapsedMilliseconds >= 1000)
            {
                FPSImage.Dispose();
                FPSImage = new Bitmap(32, 32);
                Graphics g = Graphics.FromImage(FPSImage);
                g.DrawString(Frames.ToString(), new Font(SystemFonts.DefaultFont.FontFamily, 18), new SolidBrush(Color.FromArgb(200, 64, 201, 124)), new RectangleF(0, 0, 32, 32));
                g.Dispose();
                Frames = 0;
                FrameTimer.Restart();
            }
        }

        void Game_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.LoadIdentity();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(backcolor);

            #region Render Texture
            if (false)
            {
                int ID = GetTextureID();
                //GL.Color4(1.0, 1.0, 1.0, 1.0);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, ID);
                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0, 0);
                GL.Vertex2(0 - NormOffsetRenderX, 0 - NormOffsetRenderY);

                GL.TexCoord2(0, 1);
                GL.Vertex2(0 - NormOffsetRenderX, 32 - NormOffsetRenderY);

                GL.TexCoord2(1, 1);
                GL.Vertex2(32 - NormOffsetRenderX, 32 - NormOffsetRenderY);

                GL.TexCoord2(1, 0);
                GL.Vertex2(32 - NormOffsetRenderX, 0 - NormOffsetRenderY);
                GL.End();
                GL.Disable(EnableCap.Texture2D);
                GL.DeleteTexture(ID);
            }
            #endregion

            #region Test - Corners
            GL.Begin(PrimitiveType.Polygon);
            GL.Vertex2(10, 0);
            GL.Vertex2(90, 0);

            #endregion

            foreach (Shape a in Children)
            {
                Type Type = a.GetType();
                if (Type == typeof(Polygon3D))
                {
                    //PointD[][] Points = (a as Polygon3D).Polygons.Select(x => x.GetPoints().SelectMany(y => y.X);
                    double HighX = (a as Polygon3D).Polygons.SelectMany(x => x.GetPoints().Select(y => y.X)).OrderBy(x => x).LastOrDefault();
                    double HighY = (a as Polygon3D).Polygons.SelectMany(x => x.GetPoints().Select(y => y.Y)).OrderBy(x => x).LastOrDefault();
                    //double HighY = (a as Polygon3D).Polygons.OrderBy(x => x.Y).LastOrDefault().Y;
                    double OffsetRenderX = HighX / 2.0;
                    double OffsetRenderY = HighY / 2.0;
                    foreach (Polygon b in (a as Polygon3D).Polygons)
                    {
                        GL.Begin(PrimitiveType.Polygon);
                        //GL.Color4(b.Color.R / 255.0, b.Color.G / 255.0, b.Color.B / 255.0, b.Color.A / 255.0);
                        GL.Color4(b.Color);

                        switch (b.RenderStyle)
                        {

                            case RenderStyle.Normal:
                                foreach (PointD c in b.GetPoints())
                                {
                                    GL.Vertex2(c.X - NormOffsetRenderX + a.X, c.Y - NormOffsetRenderY + a.Y);
                                }
                                break;

                            case RenderStyle.Isometric:
                                foreach (PointD c in b.GetPoints())
                                {
                                    GL.Vertex2(c.X - OffsetRenderX + a.X, c.Y - OffsetRenderY + a.Y);
                                }
                                break;
                        }
                        GL.End();
                    }
                }
                else if (Type.IsSubclassOf(typeof(UIObject)) || Type == typeof(UIObject))
                {
                    UIObject UIObj = a as UIObject;
                    GL.Begin(PrimitiveType.QuadsExt);

                    foreach (VertexColored Vertex in UIObj._DrawPoints)
                    {
                        GL.Color4(Vertex.Color == Color.Empty ? UIObj.Color : Vertex.Color);
                        GL.Vertex2(Vertex.Point.X - NormOffsetRenderX, Vertex.Point.Y - NormOffsetRenderY);
                    }
                    GL.End();
                }
                else
                {
                    GL.Begin(PrimitiveType.Quads);
                    GL.Color4(a.Color.A / 255.0, a.Color.R / 255.0, a.Color.G / 255.0, a.Color.B / 255.0);
                    double HighXa = a.GetPoints().OrderBy(x => x.X).LastOrDefault().X;
                    double HighYa = a.GetPoints().OrderBy(x => x.X).LastOrDefault().Y;
                    double OffsetRenderXa = HighXa / 2.0;
                    double OffsetRenderYa = HighYa / 2.0;
                    switch (a.RenderStyle)
                    {
                        case RenderStyle.Normal:
                            foreach (PointD b in a.GetPoints())
                            {
                                //Random r = new Random();
                                //GL.Color4(r.NextDouble(), r.NextDouble(), r.NextDouble(), 255);
                                GL.Vertex2(b.X - NormOffsetRenderX, b.Y - NormOffsetRenderY);
                            }
                            break;

                        case RenderStyle.Isometric:
                            foreach (PointD b in a.GetPoints())
                            {
                                GL.Vertex2(b.X - OffsetRenderXa, b.Y - OffsetRenderYa);
                            }
                            break;
                    }

                    GL.End();
                }
            }
            game.SwapBuffers();
            Frames++;
        }

        private bool IsPointInPolygon(PointD[] polygon, PointD point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        void Children_OnAdd(object sender, EventArgs e)
        {
            (sender as Shape).Parent = Children;
        }


        public int GetTextureID()
        {
            int ID;
            //GL.Hint(HintTarget.)
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out ID);
            GL.BindTexture(TextureTarget.Texture2D, ID);

            BitmapData data = FPSImage.LockBits(new System.Drawing.Rectangle(0, 0, FPSImage.Width, FPSImage.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            FPSImage.UnlockBits(data);


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


            return ID;
        }

        public int GetTextureID(Bitmap Image)
        {
            int ID;
            //GL.Hint(HintTarget.)
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out ID);
            GL.BindTexture(TextureTarget.Texture2D, ID);

            BitmapData data = Image.LockBits(new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            Image.UnlockBits(data);


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return ID;
        }

    }

    public class GSList<T> : List<T>
    {
        public event EventHandler OnAdd;

        public void Add(T item)
        {
            OnAdd?.Invoke(item, null);
            base.Add(item);
        }
    }
}
