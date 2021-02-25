using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace GSEngine
{
    class UIObjects
    {

    }

    public class PolygonClickHandler
    {

        public delegate void ClickEvent(PolygonClickObject Object, int FaceClicked);
        public delegate void Click3DEvent(PolygonClickObject3D Object, Polygon Poly, int FaceClicked);
        public event ClickEvent Clicked;
        public event Click3DEvent Clicked3D;

        List<PolygonClickObject> Polygons = new List<PolygonClickObject>();
        List<PolygonClickObject3D> Polygon3D = new List<PolygonClickObject3D>();
        public PolygonClickHandler()
        {

        }

        public void Pass(System.Windows.Shapes.Polygon A, int Face)
        {
            PolygonClickObject3D T = GetPolygonFromFace3D(A);
            if (T != null)
            {
                Clicked3D(T, T.IfFacePoly(A), Face);
            }
            else
            {
                Clicked(GetPolygonFromFace(A), Face);
            }
        }

        public void AddPolygonClickObject(PolygonClickObject A)
        {
            Polygons.Add(A);
        }

        public void AddPolygonClickObject3D(PolygonClickObject3D A)
        {
            Polygon3D.Add(A);
        }

        public PolygonClickObject GetPolygonFromFace(System.Windows.Shapes.Polygon A)
        {
            foreach (PolygonClickObject B in Polygons)
            {
                if (B.IfFace(A) > -1)
                {
                    return B;
                }
            }
            return null;
        }

        public PolygonClickObject3D GetPolygonFromFace3D(System.Windows.Shapes.Polygon A)
        {
            foreach (PolygonClickObject3D B in Polygon3D)
            {
                if (B.IfFace(A) > -1)
                {
                    return B;
                }
            }
            return null;
        }
    }

    public class PolygonClickObject3D
    {
        List<PolygonClickObject> Polygons = new List<PolygonClickObject>();
        public Object3D Main;

        public PolygonClickObject3D(Object3D Object)
        {
            Main = Object;
        }

        public void AddPolygon(Polygon A)
        {
            Polygons.Add(new PolygonClickObject(A));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Face"></param>
        /// <param name="FaceNum">0 = Left, 1 = Right, 2 = Top</param>
        public void AddFace(System.Windows.Shapes.Polygon Face, int FaceNum)
        {
            Polygons.Last<PolygonClickObject>().SetFace(Face, FaceNum);
        }

        public int IfFace(System.Windows.Shapes.Polygon Face)
        {
            foreach (PolygonClickObject A in Polygons)
            {
                int FaceNum = A.IfFace(Face);
                if (FaceNum > -1)
                {
                    return FaceNum;
                }
            }
            return -1;
        }

        public Polygon IfFacePoly(System.Windows.Shapes.Polygon Face)
        {
            foreach (PolygonClickObject A in Polygons)
            {
                int FaceNum = A.IfFace(Face);
                if (FaceNum > -1)
                {
                    return A.Main;
                }
            }
            return null;
        }

        public void AddPolygon(PolygonClickObject A)
        {
            Polygons.Add(A);
        }
    }

    public class PolygonClickObject
    {
        System.Windows.Shapes.Polygon LeftFace, RightFace, TopFace;
        public Polygon Main;

        public PolygonClickObject(Polygon A)
        {
            Main = A;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Face"></param>
        /// <param name="FaceNum">0 = Left, 1 = Right, 2 = Top</param>
        public void SetFace(System.Windows.Shapes.Polygon Face, int FaceNum)
        {
            switch (FaceNum)
            {
                case 0:
                    LeftFace = Face;
                    break;

                case 1:
                    RightFace = Face;
                    break;

                case 2:
                    TopFace = Face;
                    break;
            }
        }

        public int IfFace(System.Windows.Shapes.Polygon Face)
        {
            if (LeftFace == Face)
            {
                return 0;
            }
            else if (RightFace == Face)
            {
                return 1;
            }
            else if (TopFace == Face)
            {
                return 2;
            }
            return -1;
        }
        
    }

    public class PolygonClickEventArgs : EventArgs
    {
        int ClickedFace; //0 == Left, 1 == Right, 2 == Top
    }

    class AnimationHost : System.Windows.Forms.Integration.ElementHost
    {
        System.Windows.Forms.PictureBox Parent;
        public AnimationHost()
        {
            LocationChanged += AnimationHost_LocationChanged;
            System.Threading.Thread RenderBackThread = new System.Threading.Thread(new System.Threading.ThreadStart(RenderBackground));
            //Parent = (System.Windows.Forms.PictureBox)this.TopLevelControl.Controls.OfType<System.Windows.Forms.PictureBox>().First<System.Windows.Forms.PictureBox>();
            RenderBackThread.Start();
        }

        void AnimationHost_LocationChanged(object sender, EventArgs e)
        {

        }

        public void RenderBackground()
        {
            while (true)
            {
                if (Parent != null)
                {
                    Bitmap Background = new Bitmap(Size.Width, Size.Height);
                    Parent.DrawToBitmap(Background, new Rectangle(Location, Size));
                    BackgroundImage = Background;
                }
                else if (TopLevelControl != null)
                {
                    Parent = (System.Windows.Forms.PictureBox)this.TopLevelControl.Controls.OfType<System.Windows.Forms.PictureBox>().First<System.Windows.Forms.PictureBox>();
                }
            }
        }
    }

    public class InteractionMenu
    {
        internal Canvas Can;
        public delegate void Selected(int SelectedIndex);
        public event Selected Clicked;
        List<InteractionMenuObject> Objects = new List<InteractionMenuObject>();
        System.Windows.Media.Color Foreground, Background, Border;
        Canvas Parent;
        public int ID = 0;
        DoubleAnimation FadeOut = new DoubleAnimation();
        DoubleAnimation FadeIn = new DoubleAnimation();
        public InteractionMenu(int ID)
        {
            this.ID = ID;
            Can = new Canvas();
            Can.Opacity = 0;
            Can.Width = 200;
            Canvas.SetZIndex(Can, 300);
        }

        public InteractionMenu(System.Drawing.Color Foreground, System.Drawing.Color Background, System.Drawing.Color Border, int ID)
        {
            this.ID = ID;
            Can = new Canvas();
            Can.Opacity = 0;
            Can.Width = 200;
            Canvas.SetZIndex(Can, 300);
            this.Foreground = System.Windows.Media.Color.FromArgb(Foreground.A, Foreground.R, Foreground.G, Foreground.B);
            this.Background = System.Windows.Media.Color.FromArgb(Background.A, Background.R, Background.G, Background.B);
            this.Border = System.Windows.Media.Color.FromArgb(Border.A, Border.R, Border.G, Border.B);
        }

        public InteractionMenu(MenuTemplate Template, int ID)
        {
            this.ID = ID;
            Can = new Canvas();
            Can.Opacity = 0;
            Can.Width = 200;
            Canvas.SetZIndex(Can, 300);
            this.Foreground = System.Windows.Media.Color.FromArgb(Template.Foreground.A, Template.Foreground.R, Template.Foreground.G, Template.Foreground.B);
            this.Background = System.Windows.Media.Color.FromArgb(Template.Background.A, Template.Background.R, Template.Background.G, Template.Background.B);
            this.Border = System.Windows.Media.Color.FromArgb(Template.Border.A, Template.Border.R, Template.Border.G, Template.Border.B);
        }

        public void MoveToMouse(GameView A)
        {
            FadeOut.From = 1;
            FadeOut.To = 0;
            FadeOut.Duration = TimeSpan.FromMilliseconds(100);
            FadeOut.Completed += FadeOut_Completed;

            FadeIn.From = 0;
            FadeIn.To = 1;
            FadeIn.Duration = TimeSpan.FromMilliseconds(100);
            
            Can.Width = A.Main.Width;
            Can.Height = A.Main.Height;
            System.Windows.Shapes.Rectangle Rect = new System.Windows.Shapes.Rectangle();
            Rect.Width = Can.Width;
            Rect.Height = Can.Height;
            Rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 0, 0, 0));
            Canvas.SetZIndex(Rect, -1);
            Can.Children.Add(Rect);
            Rect.MouseDown += Rect_MouseDown;
            //Can.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 0, 0, 0));
            System.Windows.Point P = Mouse.GetPosition(A.Main);
            int LoopCount = 0;
            foreach (InteractionMenuObject B in Objects)
            {
                int Offset = 0;
                if(B.ID > 0)
                {
                    Offset = 1;
                }
                Canvas.SetLeft(B, P.X);
                Canvas.SetTop(B, P.Y + (Offset + LoopCount * 34));
                LoopCount++;
            }
        }

        internal void FadeInAnimation()
        {
            Can.BeginAnimation(Canvas.OpacityProperty, FadeIn);
        }

        void FadeOut_Completed(object sender, EventArgs e)
        {
            Can.Children.Clear();
            Parent.Children.Remove(Can);
        }

        void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Can.BeginAnimation(Canvas.OpacityProperty, FadeOut);
        }

        void Can_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Hai");
        }

        public void AddObject(string Text, int ClickID)
        {
            InteractionMenuObject T = new InteractionMenuObject(Text, Objects.Count, this, ClickID);
            
            Can.Height = 35 + (Objects.Count * 35);
            T.SetColors(Foreground, Background, Border);
            int Offset = 0;
            if (Objects.Count != 0)
            {
                Offset = 1;
            }
            Canvas.SetTop(T, Offset + (Objects.Count * 34));
            Objects.Add(T);
            Can.Children.Add(T);
        }

        internal void SetParent(Canvas P)
        {
            Parent = P;
        }

        Boolean SingleClicked = false;
        internal void SendClick(int ID)
        {
            if(!SingleClicked)
            {
            Clicked(ID);
            Can.BeginAnimation(Canvas.OpacityProperty, FadeOut);
            }
            SingleClicked = true;
            
        }
    }

    public class MenuTemplate
    {
        public System.Drawing.Color Foreground, Background, Border, SelectedBackground;
        public MenuTemplate(System.Drawing.Color Foreground, System.Drawing.Color Background, System.Drawing.Color Border, System.Drawing.Color SelectedBackground)
        {
            this.Foreground = Foreground;
            this.Background = Background;
            this.Border = Border;
            this.SelectedBackground = SelectedBackground;
        }

        public System.Windows.Media.Color ConvertToWPF(System.Drawing.Color C)
        {
            return System.Windows.Media.Color.FromArgb(C.A, C.R, C.G, C.B);
        }

        public SolidColorBrush GetBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(ConvertToWPF(C));
        }
    }

    public class UIMenu
    {
        public Canvas Main;
        Canvas WorkArea;
        DoubleAnimation FadeIn = new DoubleAnimation();
        public delegate void Selected(int ClickID);
        public event Selected Clicked;
        List<KeyValuePair<Button, int>> ButtonClickID = new List<KeyValuePair<Button, int>>();
        DoubleAnimation FadeOut = new DoubleAnimation();
        GameView A;
        public UIMenu(GameView A, System.Drawing.Color BackgroundColor, System.Drawing.Color BorderColor)
        {
            this.A = A;
            Main = new Canvas();
            Main.Opacity = 0;
            Main.Width = A.Width;
            Main.Height = A.Height;
            Canvas.SetZIndex(Main, 400);
            System.Windows.Shapes.Rectangle Back = new System.Windows.Shapes.Rectangle();
            Back.Width = A.Width;
            Back.Height = A.Height;
            Back.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 0, 0,0));
            Back.MouseDown += Back_MouseDown;
            Main.Children.Add(Back);
            WorkArea = new Canvas();
            WorkArea.Width = 700;
            WorkArea.Height = 600;
            

            Border B = new Border();
            B.BorderThickness = new Thickness(2,2,2,2);
            B.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(BorderColor.A, BorderColor.R, BorderColor.G, BorderColor.B));
            WorkArea.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(BackgroundColor.A, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B));

            B.Child = WorkArea;
            Main.Children.Add(B);

            Canvas.SetLeft(B, Main.Width / 2 - (WorkArea.Width / 2) - 31);
            Canvas.SetTop(B, Main.Height / 2 - (WorkArea.Height / 2) - 31);

            FadeIn.From = 0;
            FadeIn.To = 1;
            FadeIn.Duration = TimeSpan.FromMilliseconds(100);
            Main.BeginAnimation(Canvas.OpacityProperty, FadeIn);

            FadeOut.From = 1;
            FadeOut.To = 0;
            FadeOut.Duration = TimeSpan.FromMilliseconds(100);

        }

        void Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FadeOut.Completed += FadeOut_Completed;
            Main.BeginAnimation(Canvas.OpacityProperty, FadeOut);
        }

        void FadeOut_Completed(object sender, EventArgs e)
        {
            Main.Children.Clear();
            A.Main.Children.Remove(Main);
            Main = null;
        }

        public void AddButton(int X, int Y, int SizeX, int SizeY, MenuTemplate Template, string Text, int ClickID, Boolean CloseOnClick, Boolean Enabled)
        {
            ExtButton A = new ExtButton();
            A.Template = Template;
            A.CloseOnClick = CloseOnClick;
            A.Width = SizeX;
            A.Height = SizeY;
            Canvas.SetLeft(A, X);
            Canvas.SetTop(A, Y);
            A.Content = Text;
            ButtonClickID.Add(new KeyValuePair<Button, int>(A, ClickID));
            A.Click += A_Click;
            A.Background = new SolidColorBrush(Template.ConvertToWPF(Template.Background));
            A.BorderThickness = new Thickness(1, 1, 1, 1);
            A.BorderBrush = new SolidColorBrush(Template.ConvertToWPF(Template.Border));
            A.Foreground = new SolidColorBrush(Template.ConvertToWPF(Template.Foreground));
            WorkArea.Children.Add(A);
        }
        /// <summary>
        /// Draws a Shape on the UI, Faces as follows: Circle = 1, Triangle = 3, Rectangle = 4, Pentagon = 5, Hexagon = 6
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="SizeX"></param>
        /// <param name="SizeY"></param>
        /// <param name="Template"></param>
        /// <param name="NumberOfSides"></param>
        /// <param name="Thickness"></param>
        public void AddShape(int X, int Y, int SizeX, int SizeY, MenuTemplate Template, int NumberOfSides, double Thickness, int ClickID)
        {
            switch (NumberOfSides)
            {
                #region Circle
                case 1:
                    System.Windows.Shapes.Ellipse Circle = new System.Windows.Shapes.Ellipse();
                    Circle.Stroke = Template.GetBrush(Template.Border);
                    Circle.StrokeThickness = Thickness;
                    Circle.Width = SizeX;
                    Circle.Height = SizeY;
                    Circle.Fill = Template.GetBrush(Template.Background);
                    Canvas.SetLeft(Circle, X);
                    Canvas.SetTop(Circle, Y);
                    if (ClickID != -1)
                    {
                        Circle.MouseDown += (s, e) => Clicked(ClickID);
                    }
                    WorkArea.Children.Add(Circle);
                    break;
                #endregion

                #region Triangle
                case 3:
                    System.Windows.Shapes.Polygon Triangle = new System.Windows.Shapes.Polygon();
                    Triangle.Points = new PointCollection()
                    {
                        new System.Windows.Point(PolyScale(2, SizeX), Thickness),
                        new System.Windows.Point(PolyScale(4, SizeX) - Thickness, PolyScale(4, SizeY) - Thickness),
                        new System.Windows.Point(Thickness, PolyScale(4, SizeY) - Thickness),
                    };
                    Triangle.Stroke = Template.GetBrush(Template.Border);
                    Triangle.StrokeThickness = Thickness;
                    Triangle.Fill = Template.GetBrush(Template.Background);
                    Triangle.Width = SizeX;
                    Triangle.Height = SizeY;
                    Canvas.SetLeft(Triangle, X);
                    Canvas.SetTop(Triangle, Y);
                    if (ClickID != -1)
                    {
                        Triangle.MouseDown += (s, e) => Clicked(ClickID);
                    }
                    WorkArea.Children.Add(Triangle);
                    break;
                #endregion

                #region Rectangle
                case 4:
                    System.Windows.Shapes.Rectangle RectShape = new System.Windows.Shapes.Rectangle();
                    RectShape.Fill = Template.GetBrush(Template.Background);
                    Border BRect = new Border();
                    BRect.BorderThickness = new Thickness(Thickness, Thickness, Thickness, Thickness);
                    BRect.BorderBrush = Template.GetBrush(Template.Border);
                    RectShape.Width = SizeX;
                    RectShape.Height = SizeY;
                    BRect.Child = RectShape;
                    Canvas.SetLeft(BRect, X);
                    Canvas.SetTop(BRect, Y);
                    if (ClickID != -1)
                    {
                        RectShape.MouseDown += (s, e) => Clicked(ClickID);
                    }
                    WorkArea.Children.Add(BRect);
                    break;
                #endregion

                #region Pentagon
                case 5:
                    System.Windows.Shapes.Polygon Pentagon = new System.Windows.Shapes.Polygon();
                    Pentagon.Points = new PointCollection()
                    {
                        new System.Windows.Point(PolyScale(2, SizeX), Thickness),
                        new System.Windows.Point(PolyScale(4, SizeX) - Thickness, PolyScale(1.5, SizeY)),
                        new System.Windows.Point(PolyScale(3, SizeX), PolyScale(4, SizeY) - Thickness),
                        new System.Windows.Point(PolyScale(1, SizeX), PolyScale(4, SizeY) - Thickness),
                        new System.Windows.Point(Thickness, PolyScale(1.5, SizeY)),
                    };
                    Pentagon.Stroke = Template.GetBrush(Template.Border);
                    Pentagon.StrokeThickness = Thickness;
                    Pentagon.Fill = Template.GetBrush(Template.Background);
                    Pentagon.Width = SizeX;
                    Pentagon.Height = SizeY;
                    Canvas.SetLeft(Pentagon, X);
                    Canvas.SetTop(Pentagon, Y);
                    if (ClickID != -1)
                    {
                        Pentagon.MouseDown += (s, e) => Clicked(ClickID);
                    }
                    WorkArea.Children.Add(Pentagon);
                    break;
                #endregion

                #region Hexagon
                case 6:
                    System.Windows.Shapes.Polygon Hexagon = new System.Windows.Shapes.Polygon();
                    Hexagon.Points = new PointCollection()
                    {
                        new System.Windows.Point(PolyScale(1, SizeX), PolyScale(0.25, SizeY) + Thickness),
                        new System.Windows.Point(PolyScale(3, SizeX), PolyScale(0.25, SizeY) + Thickness),
                        new System.Windows.Point(PolyScale(4, SizeX) - Thickness, PolyScale(2, SizeY)),
                        new System.Windows.Point(PolyScale(3, SizeX), PolyScale(3.75, SizeY) - Thickness),
                        new System.Windows.Point(PolyScale(1, SizeX), PolyScale(3.75, SizeY) - Thickness),
                        new System.Windows.Point(Thickness, PolyScale(2, SizeY)),
                    };
                    Hexagon.Stroke = Template.GetBrush(Template.Border);
                    Hexagon.StrokeThickness = Thickness;
                    Hexagon.Fill = Template.GetBrush(Template.Background);

                    Hexagon.Width = SizeX * 1;
                    Hexagon.Height = SizeY * 1;
                    Canvas.SetLeft(Hexagon, X);
                    Canvas.SetTop(Hexagon, Y);
                    if (ClickID != -1)
                    {
                        Hexagon.MouseDown += (s, e) => Clicked(ClickID);
                    }
                    WorkArea.Children.Add(Hexagon);
                    break;
                #endregion

                #region Octagon
                case 8:
                    System.Windows.Shapes.Polygon Octagon = new System.Windows.Shapes.Polygon();
                    PointCollection OctagonPoints = new PointCollection()
                    {
                        new System.Windows.Point(PolyScale(1, SizeX), Thickness),
                        new System.Windows.Point(PolyScale(3, SizeX), Thickness),
                        new System.Windows.Point(PolyScale(4, SizeX) - Thickness, PolyScale(1, SizeY)),
                        new System.Windows.Point(PolyScale(4, SizeX) - Thickness, PolyScale(3, SizeY)),
                        new System.Windows.Point(PolyScale(3, SizeX), PolyScale(4, SizeY) - Thickness),
                        new System.Windows.Point(PolyScale(1, SizeX), PolyScale(4, SizeY) - Thickness),
                        new System.Windows.Point(Thickness, PolyScale(3, SizeY)),
                        new System.Windows.Point(Thickness, PolyScale(1, SizeY)),
                    };
                    Octagon.Points = OctagonPoints;
                    Octagon.Stroke = Template.GetBrush(Template.Border);
                    Octagon.StrokeThickness = Thickness;
                    Octagon.Fill = Template.GetBrush(Template.Background);
                    
                    Octagon.Width = SizeX;
                    Octagon.Height = SizeY;
                    Canvas.SetLeft(Octagon, X);
                    Canvas.SetTop(Octagon, Y);
                    if (ClickID != -1)
                    {
                        Octagon.MouseDown += (s, e) => Clicked(ClickID);
                    }
                    WorkArea.Children.Add(Octagon);
                    break;
                #endregion
            }
        }

        double PolyScale(double Length, int Size)
        {
            return Length * (Size / 4);
        }

        public void AddText(int X, int Y, int SizeX, int SizeY, MenuTemplate Template, string Text, int Align, double TextSize)
        {
            Label A = new Label();
            
            A.Foreground = new SolidColorBrush(Template.ConvertToWPF(Template.Foreground));
            A.Width = SizeX;
            A.Height = SizeY;
            Canvas.SetLeft(A, X);
            Canvas.SetTop(A, Y);
            switch (Align)
            {
                case 0:
                    A.HorizontalContentAlignment = HorizontalAlignment.Left;
                    break;
                case 1:
                    A.HorizontalContentAlignment = HorizontalAlignment.Center;
                    break;

                case 2:
                    A.HorizontalContentAlignment = HorizontalAlignment.Right;
                    break;
            }
            A.VerticalContentAlignment = VerticalAlignment.Center;
            A.Content = Text;
            WorkArea.Children.Add(A);
            A.FontSize = TextSize;
        }

        public void AddTextHint(int X, int Y, int SizeX, int SizeY, MenuTemplate Template, string TextForHint, double TextSize, Boolean OpenHintLeft)
        {
            Border B = new Border();
            B.BorderThickness = new Thickness(2, 2, 2, 2);
            B.BorderBrush = new SolidColorBrush(Template.ConvertToWPF(Template.Border));
            System.Windows.Shapes.Rectangle E = new System.Windows.Shapes.Rectangle();
            B.CornerRadius = new CornerRadius(8, 8, 8, 8);
            E.Width = SizeX;
            E.Height = SizeY;
            B.Background = new SolidColorBrush(Template.ConvertToWPF(Template.Background));
            B.Child = E;
            WorkArea.Children.Add(B);
            TextBlock TextHint = new TextBlock();
            TextHint.Width = SizeX - 10;
            TextHint.TextWrapping = TextWrapping.Wrap;
            TextHint.Opacity = 0;
            TextHint.Text = TextForHint;
            TextHint.FontSize = TextSize;
            B.Opacity = 0;
            B.IsHitTestVisible = false;
            E.IsHitTestVisible = false;
            TextHint.IsHitTestVisible = false;
            Canvas.SetZIndex(B, 10);
            Canvas.SetZIndex(TextHint, 10);
            if (Y < SizeY + 35)
            {
                Canvas.SetTop(B, Y - (SizeY / 4));
                Canvas.SetTop(TextHint, Y - (SizeY / 4) + 10);
            }
            else
            {
                Canvas.SetTop(B, Y - SizeY);
                Canvas.SetTop(TextHint, Y - SizeY + 10);
            }
            //E.Fill = new SolidColorBrush(Template.ConvertToWPF(Template.Background));
            TextHint.Foreground = new SolidColorBrush(Template.ConvertToWPF(Template.Foreground));
            if (OpenHintLeft)
            {
                Canvas.SetLeft(TextHint, X + 0 - SizeX);
                Canvas.SetLeft(B, X - 10 - SizeX);
            }
            else
            {
                Canvas.SetLeft(TextHint, X + 40);
                Canvas.SetLeft(B, X + 30);
            }

            WorkArea.Children.Add(TextHint);
            System.Windows.Shapes.Ellipse Tip = new System.Windows.Shapes.Ellipse();
            Tip.Width = 25;
            Tip.Height = 25;
            Tip.Fill = new SolidColorBrush(Template.ConvertToWPF(Template.Border));
            WorkArea.Children.Add(Tip);
            Canvas.SetTop(Tip, Y);
            Canvas.SetLeft(Tip, X);
            Label QM = new Label();
            QM.Content = "?";
            QM.Width = 25;
            QM.Height = 25;
            QM.HorizontalContentAlignment = HorizontalAlignment.Center;
            QM.VerticalContentAlignment = VerticalAlignment.Center;
            Canvas.SetLeft(QM, X);
            Canvas.SetTop(QM, Y);
            Canvas.SetZIndex(QM, 1);
            //QM.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            QM.FontSize = 10;
            QM.Foreground = new SolidColorBrush(Template.ConvertToWPF(Template.Background));
            WorkArea.Children.Add(QM);

            Tip.MouseEnter += (s, e) => {B.BeginAnimation(Canvas.OpacityProperty, FadeIn); TextHint.BeginAnimation(Label.OpacityProperty, FadeIn); };
            QM.MouseEnter += (s, e) => { B.BeginAnimation(Canvas.OpacityProperty, FadeIn); TextHint.BeginAnimation(Label.OpacityProperty, FadeIn); };

            Tip.MouseLeave += (s, e) => { B.BeginAnimation(Canvas.OpacityProperty, FadeOut); TextHint.BeginAnimation(Label.OpacityProperty, FadeOut); };
            QM.MouseLeave += (s, e) => { B.BeginAnimation(Canvas.OpacityProperty, FadeOut); TextHint.BeginAnimation(Label.OpacityProperty, FadeOut); };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="SizeX"></param>
        /// <param name="SizeY"></param>
        /// <param name="Template">Background for Background, Foreground for 2nd Bar and Border for 1st bar</param>
        public MultiProgressBar AddProgress(int X, int Y, int SizeX, int SizeY, MenuTemplate Template)
        {
            Canvas Bor = new Canvas();
            Bor.Width = SizeX + 4;
            Bor.Height = SizeY + 4;
            MultiProgressBar A = new MultiProgressBar(SizeX, SizeY, Template.ConvertToWPF(Template.Border), Template.ConvertToWPF(Template.Foreground), Template.ConvertToWPF(Template.Background));
            Canvas.SetLeft(Bor, X);
            Canvas.SetTop(Bor, Y);
            Bor.Background = new SolidColorBrush(Template.ConvertToWPF(Template.Border));
            Bor.Children.Add(A.Main);
            Canvas.SetLeft(A.Main, 2);
            Canvas.SetTop(A.Main, 2);
            WorkArea.Children.Add(Bor);
            return A;
        }

        void A_Click(object sender, RoutedEventArgs e)
        {
            ExtButton T = (ExtButton)sender;
            foreach (KeyValuePair<Button, int> A in ButtonClickID)
            {
                if (T == A.Key)
                {
                    try
                    {
                        Clicked(A.Value);
                        if (T.CloseOnClick)
                        {

                            FadeOut.Completed +=FadeOut_Completed;
                            FadeOut.BeginTime = TimeSpan.FromMilliseconds(100);
                            Main.BeginAnimation(Canvas.OpacityProperty, FadeOut);
                        }
                        else
                        {
                            if (!T.Selected)
                            {
                                T.Background = new SolidColorBrush(T.Template.ConvertToWPF(T.Template.SelectedBackground));
                                T.Selected = true;
                            }
                            else
                            {
                                T.Background = new SolidColorBrush(T.Template.ConvertToWPF(T.Template.Background));
                                T.Selected = false;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
    }

    public class ExtButton : Button
    {
        public Boolean CloseOnClick;
        public Boolean Selected;
        public MenuTemplate Template;
    }

    public class MultiProgressBar
    {
        public StackPanel Main;
        System.Windows.Shapes.Rectangle FirstValue, SecondValue;
        public MultiProgressBar(int SizeX, int SizeY, System.Windows.Media.Color MainBar, System.Windows.Media.Color SecondBar, System.Windows.Media.Color Back)
        {
            Main = new StackPanel();

            Main.Width = SizeX;
            Main.Height = SizeY;

            FirstValue = new System.Windows.Shapes.Rectangle();
            SecondValue = new System.Windows.Shapes.Rectangle();

            FirstValue.Fill = new SolidColorBrush(MainBar);
            SecondValue.Fill = new SolidColorBrush(SecondBar);

            FirstValue.Width = SizeX;
            SecondValue.Width = SizeX;
            FirstValue.Height = SizeY;
            SecondValue.Height = SizeY;

            FirstValue.Margin = new Thickness(-(SizeX * 2), 0, 0, 0);
            SecondValue.Margin = new Thickness(-(SizeX * 2), -SizeY, 0, 0);
            Main.Children.Add(FirstValue);
            Main.Children.Add(SecondValue);
            Main.ClipToBounds = true;
            Main.Background = new SolidColorBrush(Back);
            //Main.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            Canvas.SetZIndex(FirstValue, 1);
        }

        public void SetFirstValue(double Value)
        {
            double FlippedValue = Main.Width - ((Value / 100) * Main.Width);
            ThicknessAnimation A = new ThicknessAnimation();
            A.From = FirstValue.Margin;
            A.To = new System.Windows.Thickness(FlippedValue * 2 * -1, 0, 0, 0);
            A.Duration = TimeSpan.FromMilliseconds(750);
            A.AccelerationRatio = 0.5;
            A.DecelerationRatio = 0.5;
            FirstValue.BeginAnimation(System.Windows.Shapes.Rectangle.MarginProperty, A);
        }

        public void SetSecondValue(double Value)
        {
            double FlippedValue = Main.Width - ((Value / 100) * Main.Width);
            ThicknessAnimation A = new ThicknessAnimation();
            A.From = SecondValue.Margin;
            A.To = new System.Windows.Thickness(FlippedValue * 2 * -1, -Main.Height, 0, 0);
            A.Duration = TimeSpan.FromMilliseconds(750);
            A.AccelerationRatio = 0.5;
            A.DecelerationRatio = 0.5;
            SecondValue.BeginAnimation(System.Windows.Shapes.Rectangle.MarginProperty, A);
        }
    }

    internal class InteractionMenuObject : Canvas
    {
        System.Windows.Shapes.Rectangle UIObject;
        System.Windows.Media.Color Foreground, Background, Border;
        Label TextLabel;
        public int ID = 0;
        public int ClickID = 0;
        InteractionMenu P;
        Border B;
        internal InteractionMenuObject(string Text, int ID, InteractionMenu ParentMenu, int ClickID)
        {
            this.ID = ID;
            this.ClickID = ClickID;
            P = ParentMenu;
            UIObject = new System.Windows.Shapes.Rectangle();
            UIObject.Width = 200;
            UIObject.Height = 33;
            TextLabel = new Label();
            TextLabel.Content = Text;
            B = new System.Windows.Controls.Border();
            B.Child = UIObject;
            if (ID == 0)
            {
                B.BorderThickness = new Thickness(1, 1, 1, 1);
            }
            else
            {
                B.BorderThickness = new Thickness(1, 0, 1, 1);
            }
            Children.Add(B);
            Children.Add(TextLabel);
            MouseDown += InteractionMenuObject_MouseDown;
            TextLabel.MouseDown += InteractionMenuObject_MouseDown;
        }

        void InteractionMenuObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            P.SendClick(ClickID);
        }

        internal void UpdateText(string Text)
        {
            TextLabel.Content = Text;
        }

        internal void SetColors(System.Windows.Media.Color Foreground, System.Windows.Media.Color Background, System.Windows.Media.Color Border)
        {
            TextLabel.Foreground = new SolidColorBrush(Foreground);
            UIObject.Fill = new SolidColorBrush(Background);
            B.BorderBrush = new SolidColorBrush(Border);
        }
    }

    public class GameView : ElementHost
    {
        public Canvas Main;
        Canvas LoadingScreenCanvas;
        Boolean LoadingScreen;
        public System.Windows.Controls.Image Game;
        double Scale;
        public Canvas Loader;
        public GraphicsInfo Graphics;
        public PolygonClickHandler ClickEvents;
        public GameView(int SizeX, int SizeY, int Scale)
        {
            
            ClickEvents = new PolygonClickHandler();
            Graphics = new GraphicsInfo((int)Scale, this);
            Loader = LoaderSpinner();
            this.Scale = Scale;
            Main = new Canvas();
            Game = new System.Windows.Controls.Image();
            //BackColor = System.Drawing.Color.Pink;
            Game.RenderSize = new System.Windows.Size(SizeX, SizeY);
            Width = SizeX;
            Height = SizeY;
            Game.Width = SizeX;
            Game.Height = SizeY;
            //Main.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 100));
            Child = Main;
            Main.Width = Size.Width;
            Main.Height = Size.Height;
            LoadingScreenCanvas = new Canvas();
            Main.Children.Add(LoadingScreenCanvas);
            LoadingScreenCanvas.Width = Size.Width;
            LoadingScreenCanvas.Height = Size.Height;
            LoadingScreenCanvas.Children.Add(Loader);
            
            LoadingScreenCanvas.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
            Canvas.SetZIndex(LoadingScreenCanvas, 95);
            Canvas.SetTop(LoadingScreenCanvas, 0);
            Canvas.SetLeft(LoadingScreenCanvas, 0);
            Canvas.SetTop(Loader, SizeY / 2 - (Loader.Height / 2));
            Canvas.SetLeft(Loader, SizeX / 2 - (Loader.Width / 2));
            //Main.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 200, 200, 200));
            Main.Children.Add(Game);
            System.Windows.Shapes.Polygon A = new System.Windows.Shapes.Polygon();
            A.Points.Add(new System.Windows.Point(0, 0));
            A.Points.Add(new System.Windows.Point(0, 10));
            A.Points.Add(new System.Windows.Point(10, 10));
            A.Points.Add(new System.Windows.Point(10, 0));
            A.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 200, 200, 200));
            Canvas K = new Canvas();
            K.Height = 50;
            K.Width = 100;
            //Main.Children.Add(K);

            /*System.Windows.Shapes.Polygon Rec = new System.Windows.Shapes.Polygon();
            Rec.Width = 100;
            Rec.Height = 100;
            //Main.Children.Add(Rec);
            Rec.Points = new PointCollection() { new System.Windows.Point(0, 0), new System.Windows.Point(100, 0), new System.Windows.Point(100, 100), new System.Windows.Point(0, 100) };
            Canvas.SetZIndex(Rec, 100);
            Rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            Rec.MouseDown += GenericMouseDown;*/
            //Main.Children.Add(A);

            //InteractionMenu Temp = new InteractionMenu(System.Drawing.Color.FromArgb(64, 201, 124), System.Drawing.Color.FromArgb(255, 255, 255), System.Drawing.Color.FromArgb(64, 201, 124));
            //Temp.AddObject("Hai");
            //Main.Children.Add(Temp);
        }
        /// <summary>
        /// Shows a loading screen while the game renders the room
        /// </summary>
        /// <param name="Enabled">Turn the loading screen on or off</param>
        public void LoadingScreenToggle(Boolean Enabled)
        {
            this.LoadingScreen = Enabled;
            if (Enabled)
            {
                LoadingScreenCanvas.Opacity = 1;
            }
            else
            {
                LoadingScreenCanvas.Opacity = 0;
            }
        }

        void Game_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            //DoubleAnimationFrame FadeOutLoading = new DoubleAnimationFrame(LoadingScreen, 0.25, 0, 1, Canvas.OpacityProperty);
            DoubleAnimation FadeOutLoading = new DoubleAnimation();
            FadeOutLoading.From = 1;
            FadeOutLoading.To = 0;
            FadeOutLoading.Duration = TimeSpan.FromMilliseconds(250);
            Loader.Opacity = 0;
            //FadeOutLoading.BeginAnimation(Canvas.OpacityProperty, FadeOutLoading);
        }

        Canvas LoaderSpinner()
        {
            Canvas Spin = new Canvas();
            Spin.Width = 100;
            Spin.Height = 100;
            int Length = 40;
            System.Drawing.Point Starting = new System.Drawing.Point(50, 50);


            System.Windows.Shapes.Ellipse TL = new System.Windows.Shapes.Ellipse();
            TL.Width = 15;
            TL.Height = 15;
            TL.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 64, 201, 124));
            Spin.Children.Add(TL);
            System.Drawing.Point TLP = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(225), Length); //45, 90, 135, 180, 225, 270, 315
            Canvas.SetLeft(TL, TLP.X - 7.5);
            Canvas.SetTop(TL, TLP.Y - 7.5);
            DoubleAnimationFrame TLA = new DoubleAnimationFrame(TL, 0.8, 0, 1, System.Windows.Shapes.Ellipse.OpacityProperty);
            TLA.BeginTime(-0.1 * 7);
            TLA.Animation.RepeatBehavior = RepeatBehavior.Forever;
            TLA.Begin();

            System.Windows.Shapes.Ellipse TM = new System.Windows.Shapes.Ellipse();
            TM.Width = 15;
            TM.Height = 15;
            TM.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 64, 201, 124));
            Spin.Children.Add(TM);
            System.Drawing.Point TMP = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(270), Length);
            Canvas.SetLeft(TM, TMP.X - 7.5);
            Canvas.SetTop(TM, TMP.Y - 7.5);
            DoubleAnimationFrame TMA = new DoubleAnimationFrame(TM, 0.8, 0, 1, System.Windows.Shapes.Ellipse.OpacityProperty);
            TMA.BeginTime(-0.1 * 8);
            TMA.Animation.RepeatBehavior = RepeatBehavior.Forever;
            TMA.Begin();

            System.Windows.Shapes.Ellipse TR = new System.Windows.Shapes.Ellipse();
            TR.Width = 15;
            TR.Height = 15;
            TR.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 64, 201, 124));
            Spin.Children.Add(TR);
            System.Drawing.Point TRP = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(315), Length);
            Canvas.SetLeft(TR, TRP.X - 7.5);
            Canvas.SetTop(TR, TRP.Y - 7.5);
            DoubleAnimationFrame TRA = new DoubleAnimationFrame(TR, 0.8, 0, 1, System.Windows.Shapes.Ellipse.OpacityProperty);
            TRA.BeginTime(-0.1 * 1);
            TRA.Animation.RepeatBehavior = RepeatBehavior.Forever;
            TRA.Begin();

            System.Windows.Shapes.Ellipse ML = new System.Windows.Shapes.Ellipse();
            ML.Width = 15;
            ML.Height = 15;
            ML.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 64, 201, 124));
            Spin.Children.Add(ML);
            System.Drawing.Point MLP = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(180), Length);
            Canvas.SetLeft(ML, MLP.X - 7.5);
            Canvas.SetTop(ML, MLP.Y - 7.5);
            DoubleAnimationFrame MLA = new DoubleAnimationFrame(ML, 0.8, 0, 1, System.Windows.Shapes.Ellipse.OpacityProperty);
            MLA.BeginTime(-0.1 * 6);
            MLA.Animation.RepeatBehavior = RepeatBehavior.Forever;
            MLA.Begin();

            System.Windows.Shapes.Ellipse MR = new System.Windows.Shapes.Ellipse();
            MR.Width = 15;
            MR.Height = 15;
            MR.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 64, 201, 124));
            Spin.Children.Add(MR);
            System.Drawing.Point MMP = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(0), Length);
            Canvas.SetLeft(MR, MMP.X - 7.5);
            Canvas.SetTop(MR, MMP.Y - 7.5);
            DoubleAnimationFrame MRA = new DoubleAnimationFrame(MR, 0.8, 0, 1, System.Windows.Shapes.Ellipse.OpacityProperty);
            MRA.BeginTime(-0.1 * 2);
            MRA.Animation.RepeatBehavior = RepeatBehavior.Forever;
            MRA.Begin();

            System.Windows.Shapes.Ellipse BL = new System.Windows.Shapes.Ellipse();
            BL.Width = 15;
            BL.Height = 15;
            BL.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 64, 201, 124));
            Spin.Children.Add(BL);
            System.Drawing.Point BLP = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(135), Length);
            Canvas.SetLeft(BL, BLP.X - 7.5);
            Canvas.SetTop(BL, BLP.Y - 7.5);
            DoubleAnimationFrame BLA = new DoubleAnimationFrame(BL, 0.8, 0, 1, System.Windows.Shapes.Ellipse.OpacityProperty);
            BLA.BeginTime(-0.1 * 5);
            BLA.Animation.RepeatBehavior = RepeatBehavior.Forever;
            BLA.Begin();

            System.Windows.Shapes.Ellipse BM = new System.Windows.Shapes.Ellipse();
            BM.Width = 15;
            BM.Height = 15;
            BM.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 64, 201, 124));
            Spin.Children.Add(BM);
            System.Drawing.Point BMP = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(90), Length);
            Canvas.SetLeft(BM, BMP.X - 7.5);
            Canvas.SetTop(BM, BMP.Y - 7.5);
            DoubleAnimationFrame BMA = new DoubleAnimationFrame(BM, 0.8, 0, 1, System.Windows.Shapes.Ellipse.OpacityProperty);
            BMA.BeginTime(-0.1 * 4);
            BMA.Animation.RepeatBehavior = RepeatBehavior.Forever;
            BMA.Begin();

            System.Windows.Shapes.Ellipse BR = new System.Windows.Shapes.Ellipse();
            BR.Width = 15;
            BR.Height = 15;
            BR.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 64, 201, 124));
            Spin.Children.Add(BR);
            System.Drawing.Point BRP = ArtProcessing.ReturnAnglePoint(Starting, ArtProcessing.ToRadians(45), Length);
            Canvas.SetLeft(BR, BRP.X - 7.5);
            Canvas.SetTop(BR, BRP.Y - 7.5);
            DoubleAnimationFrame BRA = new DoubleAnimationFrame(BR, 0.8, 0, 1, System.Windows.Shapes.Ellipse.OpacityProperty);
            BRA.BeginTime(-0.1 * 3);
            BRA.Animation.RepeatBehavior = RepeatBehavior.Forever;
            BRA.Begin();
            //Spin.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            return Spin;
        }

        public void RenderBackground()
        {
            Graphics.Render();
        }
        public void FinishLoading()
        {
            Main.Dispatcher.Invoke(() =>
            {
            
            DoubleAnimation FadeOutLoading = new DoubleAnimation();
            FadeOutLoading.From = 1;
            FadeOutLoading.To = 0;
            FadeOutLoading.Duration = TimeSpan.FromMilliseconds(250);
            Loader.Opacity = 0;
            FadeOutLoading.Completed += FadeOutLoading_Completed;
            LoadingScreenCanvas.BeginAnimation(Canvas.OpacityProperty, FadeOutLoading);
            
            
            });
        }

        void FadeOutLoading_Completed(object sender, EventArgs e)
        {
            LoadingScreenCanvas.IsHitTestVisible = false;
            Main.Children.Remove(LoadingScreenCanvas);
        }

        public void AddBackgroundPoly(Polygon Polygon, int ZIndex)
        {
            Graphics.AddPolygon(Polygon, ZIndex);
        }

        public void AddPoly(System.Drawing.Point[] Points)
        {
            System.Windows.Shapes.Polygon A = new System.Windows.Shapes.Polygon();
            foreach (System.Drawing.Point a in Points)
            {
                A.Points.Add(new System.Windows.Point(a.X / 4, a.Y / 4));
            }
            A.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            Main.Children.Add(A);
        }

        void AddIsometricPolyOld(Polygon A, int ZIndex)
        {
            //Main.Dispatcher.Invoke(() =>
            //{
                Canvas B = new Canvas();
                B.Width = A.ContainerSize.Width / Scale;
                B.Height = A.ContainerSize.Height / Scale;
                
                B.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));

                System.Windows.Shapes.Polygon LeftP = new System.Windows.Shapes.Polygon();
                LeftP.Points = MoveUpRight(ConvertToWPFPoint(A.LeftFacePoints));
                if (A.LeftTBrush != null)
                {
                    LeftP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.LeftTBrush.Image)));
                }
                else
                {
                    LeftP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.LeftBrush.R, A.LeftBrush.G, A.LeftBrush.B));
                }
                B.Children.Add(LeftP);
                LeftP.MouseDown += GenericMouseDown;   //(s, e) => ClickEvents.Pass(A, 0);
                LeftP.MouseLeftButtonDown += GenericMouseDown;
                //Main.AddHandler(FrameworkElement.MouseDownEvent, new MouseButtonEventHandler(GenericMouseDown), true);

                System.Windows.Shapes.Polygon RightP = new System.Windows.Shapes.Polygon();
                RightP.Points = MoveUpLeft(ConvertToWPFPoint(A.RightFacePoints));
                if (A.RightTBrush != null)
                {
                    RightP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.RightTBrush.Image)));
                }
                else
                {
                    RightP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.RightBrush.R, A.RightBrush.G, A.RightBrush.B));
                }
                B.Children.Add(RightP);

                System.Windows.Shapes.Polygon TopP = new System.Windows.Shapes.Polygon();
                TopP.Points = ConvertToWPFPoint(A.TopFacePoints);
                if (A.TopTBrush != null)
                {
                    TopP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.TopTBrush.Image)));
                }
                else
                {
                    TopP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.TopBrush.R, A.TopBrush.G, A.TopBrush.B));
                }
                B.Children.Add(TopP);
                RenderOptions.SetEdgeMode(B, EdgeMode.Unspecified);
                //B.SnapsToDevicePixels = true;
                //B.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                //System.Drawing.Size LeftPSize = SizeFromPoints(LeftP.Points);
                //LeftP.Width = LeftPSize.Width;
                //LeftP.Height = LeftPSize.Height;

                Main.Children.Add(B);


                //B.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                //Main.Children.Add(LeftP);
                //B.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                Canvas.SetLeft(B, A.ReferencePoint.X / Scale);
                Canvas.SetTop(B, A.ReferencePoint.Y / Scale);
                Canvas.SetZIndex(B, ZIndex);
            //});
        }

        public void AddIsometricPoly(Polygon A, int ZIndex)
        {
            Main.Dispatcher.Invoke(() =>
            {
                PolygonClickObject ClickObject = new PolygonClickObject(A);
                System.Windows.Shapes.Polygon LeftP = new System.Windows.Shapes.Polygon();
                Main.Children.Add(LeftP);
                LeftP.Points = MoveUpRight(ConvertToWPFPoint(A.LeftFacePoints));
                //LeftP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                if (A.LeftTBrush != null)
                {
                    LeftP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.LeftTBrush.Image)));
                }
                else
                {
                    LeftP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.LeftBrush.R, A.LeftBrush.G, A.LeftBrush.B));
                }
                LeftP.MouseDown += (s, e) => ClickEvents.Pass(LeftP, 0);
                Canvas.SetLeft(LeftP, A.ReferencePoint.X / Scale);
                Canvas.SetTop(LeftP, A.ReferencePoint.Y / Scale);
                ClickObject.SetFace(LeftP, 0);

                System.Windows.Shapes.Polygon RightP = new System.Windows.Shapes.Polygon();
                RightP.Points = MoveUpLeft(ConvertToWPFPoint(A.RightFacePoints));
                if (A.RightTBrush != null)
                {
                    RightP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.RightTBrush.Image)));
                }
                else
                {
                    RightP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.RightBrush.R, A.RightBrush.G, A.RightBrush.B));
                }
                Canvas.SetLeft(RightP, A.ReferencePoint.X / Scale);
                Canvas.SetTop(RightP, A.ReferencePoint.Y / Scale);
                RightP.MouseDown += (s, e) => ClickEvents.Pass(RightP, 1);
                Main.Children.Add(RightP);
                ClickObject.SetFace(RightP, 1);

                System.Windows.Shapes.Polygon TopP = new System.Windows.Shapes.Polygon();
                TopP.Points = ConvertToWPFPoint(A.TopFacePoints);
                if (A.TopTBrush != null)
                {
                    TopP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.TopTBrush.Image)));
                }
                else
                {
                    TopP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.TopBrush.R, A.TopBrush.G, A.TopBrush.B));
                }
                Canvas.SetLeft(TopP, A.ReferencePoint.X / Scale);
                Canvas.SetTop(TopP, A.ReferencePoint.Y / Scale);
                Main.Children.Add(TopP);
                TopP.MouseDown += (s, e) => ClickEvents.Pass(TopP, 2);
                ClickObject.SetFace(TopP, 2);

                RenderOptions.SetEdgeMode(Main, EdgeMode.Unspecified);
                //B.SnapsToDevicePixels = true;
                //B.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                //System.Drawing.Size LeftPSize = SizeFromPoints(LeftP.Points);
                //LeftP.Width = LeftPSize.Width;
                //LeftP.Height = LeftPSize.Height;


                //B.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                //Main.Children.Add(LeftP);
                //B.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                Canvas.SetZIndex(TopP, ZIndex);
                Canvas.SetZIndex(LeftP, ZIndex);
                Canvas.SetZIndex(RightP, ZIndex);
                ClickEvents.AddPolygonClickObject(ClickObject);
            });
        }

        void GenericMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ClickEvents.Pass((System.Windows.Shapes.Polygon)sender, 0);
        }

        void Add3DObjectOld(Object3D Obj, int ZIndex)
        {
            Main.Dispatcher.Invoke(() =>
            {
                Canvas B = new Canvas();
                int SizeX = 0, SizeY = 0;
                int PosX = Width, PosY = Height;
                foreach (KeyValuePair<Polygon, int> a in Obj.Polygons)
                {
                    Polygon A = a.Key;

                    if (PosX > A.ReferencePoint.X / Scale)
                    {
                        PosX = Convert.ToInt32(A.ReferencePoint.X / Scale);
                    }
                    if (PosY > A.ReferencePoint.Y / Scale)
                    {
                        PosY = Convert.ToInt32(A.ReferencePoint.Y / Scale);
                    }
                }
                foreach (KeyValuePair<Polygon, int> a in Obj.Polygons)
                {
                    Polygon A = a.Key;


                    System.Windows.Shapes.Polygon LeftP = new System.Windows.Shapes.Polygon();
                    PointCollection LP = new PointCollection();
                    foreach (System.Drawing.Point p in A.LeftFacePoints)
                    {
                        LP.Add(new System.Windows.Point(p.X / Scale, p.Y / Scale));
                        if (p.X / Scale > SizeX)
                        {
                            SizeX = Convert.ToInt32(p.X / Scale);
                        }
                        if (p.Y / Scale > SizeY)
                        {
                            SizeY = Convert.ToInt32(p.Y / Scale);
                        }
                    }
                    LeftP.Points = MoveUpRight(LP);
                    if (A.LeftTBrush != null)
                    {
                        LeftP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.LeftTBrush.Image)));
                    }
                    else
                    {
                        LeftP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.LeftBrush.R, A.LeftBrush.G, A.LeftBrush.B));
                    }
                    B.Children.Add(LeftP);
                    Canvas.SetLeft(LeftP, A.ReferencePoint.X / Scale - PosX);
                    Canvas.SetTop(LeftP, A.ReferencePoint.Y / Scale - PosY);
                    Canvas.SetZIndex(LeftP, a.Value);

                    System.Windows.Shapes.Polygon RightP = new System.Windows.Shapes.Polygon();
                    PointCollection RP = new PointCollection();
                    foreach (System.Drawing.Point p in A.RightFacePoints)
                    {
                        RP.Add(new System.Windows.Point(p.X / Scale, p.Y / Scale));
                        if (p.X / Scale > SizeX)
                        {
                            SizeX = Convert.ToInt32(p.X / Scale);
                        }
                        if (p.Y / Scale > SizeY)
                        {
                            SizeY = Convert.ToInt32(p.Y / Scale);
                        }
                    }
                    RightP.Points = MoveUpLeft(RP);
                    if (A.RightTBrush != null)
                    {
                        RightP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.RightTBrush.Image))) { Stretch = Stretch.Uniform };
                    }
                    else
                    {
                        RightP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.RightBrush.R, A.RightBrush.G, A.RightBrush.B));
                    }
                    B.Children.Add(RightP);
                    Canvas.SetLeft(RightP, A.ReferencePoint.X / Scale - PosX);
                    Canvas.SetTop(RightP, A.ReferencePoint.Y / Scale - PosY);
                    Canvas.SetZIndex(RightP, a.Value);

                    System.Windows.Shapes.Polygon TopP = new System.Windows.Shapes.Polygon();
                    PointCollection TP = new PointCollection();
                    foreach (System.Drawing.Point p in A.TopFacePoints)
                    {
                        TP.Add(new System.Windows.Point(p.X / Scale, p.Y / Scale));
                        if (p.X / Scale > SizeX)
                        {
                            SizeX = Convert.ToInt32(p.X / Scale);
                        }
                        if (p.Y / Scale > SizeY)
                        {
                            SizeY = Convert.ToInt32(p.Y / Scale);
                        }
                    }
                    TopP.Points = TP;
                    if (A.TopTBrush != null)
                    {
                        TopP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.TopTBrush.Image)));
                    }
                    else
                    {
                        TopP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.TopBrush.R, A.TopBrush.G, A.TopBrush.B));
                    }
                    B.Children.Add(TopP);
                    Canvas.SetLeft(TopP, A.ReferencePoint.X / Scale - PosX);
                    Canvas.SetTop(TopP, A.ReferencePoint.Y / Scale - PosY);
                    Canvas.SetZIndex(TopP, a.Value);

                }
                B.Width = 1;
                B.Height = 1;
                Main.Children.Add(B);
                Canvas.SetLeft(B, PosX);
                Canvas.SetTop(B, PosY);
                Canvas.SetZIndex(B, ZIndex);
                //B.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            });
        }

        InteractionMenu LastInteractionMenu;
        public void AddInteractionMenu(InteractionMenu Menu, Boolean MoveToMouse)
        {
            if (LastInteractionMenu != null)
            {
                LastInteractionMenu.Can.Children.Clear();
                Main.Children.Remove(LastInteractionMenu.Can);
                LastInteractionMenu = null;
            }
            Menu.MoveToMouse(this);
            Menu.SetParent(Main);
            Main.Children.Add(Menu.Can);
            Menu.FadeInAnimation();
            LastInteractionMenu = Menu;
        }

        public int InteractionMenuID()
        {
            if (LastInteractionMenu != null && LastInteractionMenu.Can.Children.Count != 0)
            {
                return LastInteractionMenu.ID;
            }
            return -1;
        }

        public void RemoveInteraction()
        {
            if (LastInteractionMenu != null && LastInteractionMenu.Can.Children.Count != 0)
            {
                LastInteractionMenu.Can.Children.Clear();
                Main.Children.Remove(LastInteractionMenu.Can);
                LastInteractionMenu = null;
            }
        }

        public void AddItem(UIMenu A)
        {
            Main.Children.Add(A.Main);
        }

        public void Add3DObject(Object3D Obj, int ZIndex)
        {
            Main.Dispatcher.Invoke(() =>
            {
                PolygonClickObject3D ClickObject = new PolygonClickObject3D(Obj);
                //int SizeX = 0, SizeY = 0;
                int PosX = Width, PosY = Height;
                foreach (KeyValuePair<Polygon, int> a in Obj.Polygons)
                {
                    Polygon A = a.Key;

                    if (PosX > A.ReferencePoint.X / Scale)
                    {
                        PosX = Convert.ToInt32(A.ReferencePoint.X / Scale);
                    }
                    if (PosY > A.ReferencePoint.Y / Scale)
                    {
                        PosY = Convert.ToInt32(A.ReferencePoint.Y / Scale);
                    }
                }
                foreach (KeyValuePair<Polygon, int> a in Obj.Polygons)
                {
                    Polygon A = a.Key;
                    PolygonClickObject ClickObject2D = new PolygonClickObject(A);
                    System.Windows.Shapes.Polygon LeftP = new System.Windows.Shapes.Polygon();
                    PointCollection LP = new PointCollection();
                    foreach (System.Drawing.Point p in A.LeftFacePoints)
                    {
                        LP.Add(new System.Windows.Point(p.X / Scale, p.Y / Scale));
                    }
                    LeftP.Points = MoveUpRight(LP);
                    if (A.LeftTBrush != null)
                    {
                        LeftP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.LeftTBrush.Image)));
                    }
                    else
                    {
                        LeftP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.LeftBrush.R, A.LeftBrush.G, A.LeftBrush.B));
                    }
                    Main.Children.Add(LeftP);
                    Canvas.SetLeft(LeftP, A.ReferencePoint.X / Scale);
                    Canvas.SetTop(LeftP, A.ReferencePoint.Y / Scale);
                    Canvas.SetZIndex(LeftP, a.Value + ZIndex);
                    LeftP.MouseDown += (s, e) => ClickEvents.Pass(LeftP, 0);
                    ClickObject2D.SetFace(LeftP, 0);


                    System.Windows.Shapes.Polygon RightP = new System.Windows.Shapes.Polygon();
                    PointCollection RP = new PointCollection();
                    foreach (System.Drawing.Point p in A.RightFacePoints)
                    {
                        RP.Add(new System.Windows.Point(p.X / Scale, p.Y / Scale));
                    }
                    RightP.Points = MoveUpLeft(RP);
                    if (A.RightTBrush != null)
                    {
                        RightP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.RightTBrush.Image))) { Stretch = Stretch.Uniform };
                    }
                    else
                    {
                        RightP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.RightBrush.R, A.RightBrush.G, A.RightBrush.B));
                    }
                    Main.Children.Add(RightP);
                    Canvas.SetLeft(RightP, A.ReferencePoint.X / Scale);
                    Canvas.SetTop(RightP, A.ReferencePoint.Y / Scale);
                    Canvas.SetZIndex(RightP, a.Value + ZIndex);
                    RightP.MouseDown += (s, e) => ClickEvents.Pass(RightP, 1);
                    ClickObject2D.SetFace(RightP, 1);



                    System.Windows.Shapes.Polygon TopP = new System.Windows.Shapes.Polygon();
                    PointCollection TP = new PointCollection();
                    foreach (System.Drawing.Point p in A.TopFacePoints)
                    {
                        TP.Add(new System.Windows.Point(p.X / Scale, p.Y / Scale));
                    }
                    TopP.Points = TP;
                    if (A.TopTBrush != null)
                    {
                        TopP.Fill = new ImageBrush(ImageSourceForBitmap(new Bitmap(A.TopTBrush.Image)));
                    }
                    else
                    {
                        TopP.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, A.TopBrush.R, A.TopBrush.G, A.TopBrush.B));
                    }
                    Main.Children.Add(TopP);
                    Canvas.SetLeft(TopP, A.ReferencePoint.X / Scale);
                    Canvas.SetTop(TopP, A.ReferencePoint.Y / Scale);
                    Canvas.SetZIndex(TopP, a.Value + ZIndex);
                    TopP.MouseDown += (s, e) => ClickEvents.Pass(TopP, 2);
                    ClickObject2D.SetFace(TopP, 2);
                    ClickObject.AddPolygon(ClickObject2D);
                }
                ClickEvents.AddPolygonClickObject3D(ClickObject);
                //B.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            });
        }

        #region Functions

        PointCollection ConvertToWPFPoint(System.Drawing.Point[] P)
        {
            PointCollection Return = new PointCollection();
            foreach (System.Drawing.Point A in P)
            {
                Return.Add(new System.Windows.Point(A.X / Scale, A.Y / Scale));
            }
            return Return;
        }

        PointCollection MoveUpRight(PointCollection A)
        {
            PointCollection C = new PointCollection();
            foreach (System.Windows.Point B in A)
            {
                C.Add(new System.Windows.Point(B.X + 1, B.Y - 1));
            }
            return C;
        }
        PointCollection MoveUpLeft(PointCollection A)
        {
            PointCollection C = new PointCollection();
            foreach (System.Windows.Point B in A)
            {
                C.Add(new System.Windows.Point(B.X - 1, B.Y - 1));
            }
            return C;
        }

        System.Drawing.Size SizeFromPoints(PointCollection A)
        {
            double X = 0, Y = 0;
            foreach (System.Windows.Point B in A)
            {
                if (X < B.X / 4)
                {
                    X = B.X / 4;
                }
                if (Y < B.Y / 4)
                {
                    Y = B.Y / 4;
                }
            }
            return new System.Drawing.Size((int)X, (int)Y);
        }

        public void SetImage(Bitmap Image)
        {
            Game.Source = ImageSourceForBitmap(Image);
        }

        ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        #endregion
    }



    public class DoubleAnimatedObject
    {
        public System.Windows.Shapes.Shape Object;
        public System.Windows.Media.Animation.DoubleAnimation Animation;
        public DependencyProperty Target;
    }

    public class DoubleAnimationFrame : DoubleAnimatedObject
    {
        public System.Windows.Shapes.Shape Object;
        public DoubleAnimationFrame(System.Windows.Shapes.Shape Object, double Time, double To, double From, DependencyProperty Target)
        {
            this.Object = Object;
            Animation = new DoubleAnimation(From, To, new Duration(TimeSpan.FromSeconds(Time)));
            this.Target = Target;
        }

        public void Begin()
        {
            Object.BeginAnimation(Target, Animation);
        }

        public void BeginTime(double Seconds)
        {
            Animation.BeginTime = TimeSpan.FromSeconds(Seconds);
        }
    }

    class AnimatedObjectBubble : DoubleAnimatedObject
    {

        public AnimatedObjectBubble()
        {
            Object = new System.Windows.Shapes.Ellipse();
            Object.Width = 50;
            Object.Height = 50;
            Object.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 64, 201, 124));
            CreateAnimation();
        }

        void CreateAnimation()
        {
            Animation = new DoubleAnimation();
            Animation.To = 0;
            Animation.From = 1000;
            Animation.Duration = TimeSpan.FromMilliseconds(2000);
            Animation.AccelerationRatio = 0.50;
            Animation.DecelerationRatio = 0.50;
            Target = Canvas.LeftProperty;
        }

        public void BeginAnimation()
        {
            Object.BeginAnimation(Target, Animation);
        }
    }

    public class GUINeedsBar : System.Windows.Controls.Canvas
    {
        public WPFNeedsBar Bar1, Bar2, Bar3, Bar4;
        public GUINeedsBar(System.Drawing.Color FirstBar, System.Drawing.Color SecondBar, System.Drawing.Color ThirdBar, System.Drawing.Color FouthBar)
        {

            DrawBackground();

            ElementHost EleFirst = new ElementHost();
            ElementHost EleSecond = new ElementHost();
            ElementHost EleThird = new ElementHost();
            ElementHost EleFourth = new ElementHost();

            EleFirst.Location = new System.Drawing.Point(2, 2);
            EleSecond.Location = new System.Drawing.Point(10, 2);
            EleThird.Location = new System.Drawing.Point(18, 2);
            EleFourth.Location = new System.Drawing.Point(26, 2);

            EleFirst.Size = new System.Drawing.Size(6, 100);
            EleSecond.Size = new System.Drawing.Size(6, 100);
            EleThird.Size = new System.Drawing.Size(6, 100);
            EleFourth.Size = new System.Drawing.Size(6, 100);

            Bar1 = new WPFNeedsBar(FirstBar);
            Bar2 = new WPFNeedsBar(SecondBar);
            Bar3 = new WPFNeedsBar(ThirdBar);
            Bar4 = new WPFNeedsBar(FouthBar);

            Canvas Bar1Canvas = new Canvas();
            Canvas Bar2Canvas = new Canvas();
            Canvas Bar3Canvas = new Canvas();
            Canvas Bar4Canvas = new Canvas();

            Bar1Canvas.Width = 6;
            Bar2Canvas.Width = 6;
            Bar3Canvas.Width = 6;
            Bar4Canvas.Width = 6;

            Bar1Canvas.Height = 100;
            Bar2Canvas.Height = 100;
            Bar3Canvas.Height = 100;
            Bar4Canvas.Height = 100;

            Canvas.SetTop(Bar1Canvas, 2);
            Canvas.SetTop(Bar2Canvas, 2);
            Canvas.SetTop(Bar3Canvas, 2);
            Canvas.SetTop(Bar4Canvas, 2);

            Canvas.SetLeft(Bar1Canvas, 2);
            Canvas.SetLeft(Bar2Canvas, 10);
            Canvas.SetLeft(Bar3Canvas, 18);
            Canvas.SetLeft(Bar4Canvas, 26);

            Children.Add(Bar1Canvas);
            Children.Add(Bar2Canvas);
            Children.Add(Bar3Canvas);
            Children.Add(Bar4Canvas);

            Bar1Canvas.Children.Add(Bar1);
            Bar2Canvas.Children.Add(Bar2);
            Bar3Canvas.Children.Add(Bar3);
            Bar4Canvas.Children.Add(Bar4);
            Width = 34;
            Height = 104;
            Bar1Canvas.ClipToBounds = true;
            Bar2Canvas.ClipToBounds = true;
            Bar3Canvas.ClipToBounds = true;
            Bar4Canvas.ClipToBounds = true;
        }

        public void DrawBackground()
        {
            Bitmap I = new Bitmap(34, 104);
            Graphics g = Graphics.FromImage(I);
            System.Drawing.Pen Outline = new System.Drawing.Pen(System.Drawing.Color.Black, 2);
            g.DrawRectangle(Outline, 1, 1, 8, 102);
            g.DrawRectangle(Outline, 9, 1, 8, 102);
            g.DrawRectangle(Outline, 17, 1, 8, 102);
            g.DrawRectangle(Outline, 25, 1, 8, 102);
            //BackgroundImage = I;
            Background = new ImageBrush(ImageSourceForBitmap(I));
        }
        ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
    }

    public class WPFNeedsBar : System.Windows.Controls.StackPanel
    {
        public double Value;
        System.Windows.Shapes.Rectangle Bar = new System.Windows.Shapes.Rectangle();
        public WPFNeedsBar(System.Drawing.Color A)
        {
            System.Windows.Media.Color B = System.Windows.Media.Color.FromArgb(A.A, A.R, A.G, A.B);
            Bar.Width = 6;
            Bar.Height = 100;
            Bar.Fill = new System.Windows.Media.SolidColorBrush(B);
            Bar.Margin = new System.Windows.Thickness(0, 0, 0, -300);
            Children.Add(Bar);
        }

        public void SetBarValue(double Value)
        {
            this.Value = Value;
            double FlippedValue = 100 - Value;
            ThicknessAnimation A = new ThicknessAnimation();
            A.From = Bar.Margin;
            A.To = new System.Windows.Thickness(0, 0, 0, FlippedValue * 2 * -1 - 100);
            A.Duration = TimeSpan.FromMilliseconds(750);
            A.AccelerationRatio = 0.5;
            A.DecelerationRatio = 0.5;
            Bar.BeginAnimation(System.Windows.Shapes.Rectangle.MarginProperty, A);
        }

        public void SetBarValueInstant(double Value)
        {
            this.Value = Value;
            double FlippedValue = 100 - Value;
            ThicknessAnimation A = new ThicknessAnimation();
            A.From = Bar.Margin;
            A.To = new System.Windows.Thickness(0, 0, 0, FlippedValue * 2 * -1 - 100);
            A.Duration = TimeSpan.FromMilliseconds(1);
            Bar.BeginAnimation(System.Windows.Shapes.Rectangle.MarginProperty, A);
        }
    }
}
