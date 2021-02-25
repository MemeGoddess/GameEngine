using System;
using System.Drawing;
using GSEngine;
using GSEngine.OpenGL;

namespace OpenGLTest2
{
    class MainClass
    {
        //static GameWindow game;
        static System.Boolean Orthographic = true;
        public static void Main(string[] args)
        {
            GameView game = new GameView(1280, 720, 2, 4, "Rawr");
            game.WindowClosed += (sender) => { Environment.Exit(0); };
            

            Polygon3D Floor = new Polygon3D(
                LengthX: 2.5,
                LengthY: 0.1,
                LengthZ: 2.5,
                Scale: 2,
                PosX: 0,
                PosY: 0,
                PosZ: 0,
                LeftFace: new SolidBrush(Color.FromArgb(255, 255, 0, 0)),
                RightFace: new SolidBrush(Color.FromArgb(255, 0, 255, 0)),
                TopFace: new SolidBrush(Color.FromArgb(255, 0, 0, 255)),
                renderStyle: RenderStyle.Isometric);
            Floor.MouseLeftClick += B_MouseLeftClick;
            //Floor.Y -= 100;

            Shader shader = new Shader(Color.FromArgb(240, 240, 240));
            Shader shader2 = new Shader(Color.FromArgb(64, 201, 124));

            GSEngine.PolygonRenderer box = new GSEngine.PolygonRenderer(
                //Reference: Floor.polygonRenderer, 
                LengthX: 1,
                LengthY: 1,
                LengthZ: 1,
                Scale: 2,
                PosX: 0,
                PosY: 0,
                PosZ: 0,
                LeftFace: shader.ShadeBrush(1),
                RightFace: shader.ShadeBrush(0.8),
                TopFace: shader.ShadeBrush(0.9),
                Render: false
                );
            Polygon3D box3D = new Polygon3D(box, RenderStyle.Isometric);
            game.Children.Add(box3D);



            GSEngine.PolygonRenderer box2 = new GSEngine.PolygonRenderer(
                Reference: box,
                LengthX: 0.5,
                LengthY: 0.5,
                LengthZ: 0.5,
                Scale: 2,
                PosX: 0,
                PosY: 0,
                PosZ: 0,
                LeftFace: shader2.ShadeBrush(1),
                RightFace: shader2.ShadeBrush(0.8),
                TopFace: shader2.ShadeBrush(0.9),
                Render: false
                );
            Polygon3D box3D2 = new Polygon3D(box2, RenderStyle.Isometric);
            game.Children.Add(box3D2);

            //game.Children.Add(Floor);
            Polygon P = new Polygon(new PointD[]
            {
                new PointD(50, 50),
                new PointD(50, 150),
                new PointD(150, 180),
                new PointD(150, 80)
            }, Color.FromArgb(255, 255, 0, 0));
            //game.Children.Add(P);

            Button But = new Button();
            //game.Children.Add(But);
            But.MouseLeftClick += B_MouseLeftClick;
            But.BackColor = Color.FromArgb(239, 239, 239);
            game.BackColor = Color.FromArgb(190, 190, 190);

            while (true)
            {
                Console.ReadKey(true);
            }
        }

        static void B_MouseLeftClick(object Sender, OpenTK.Input.MouseEventArgs e)
        {
            Console.WriteLine("Clicked!");
        }
    }
}
