using GSEngine;
using Project_Game.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Game
{
    class Desk : IsometricObject
    {
        public Desk(int Scale, double PosX, double PosY, double PosZ, Polygon Reference)
        {
            //Polygon TopLeftLeg = new Polygon(Reference, 0.25, 1, 0.25, Scale, 0 + PosX, 1.75 + PosY, PosZ, new SolidBrush(Color.FromArgb(172,132,104)));
            Polygon TopRightLeg = new Polygon(Reference, 0.15, 1, 0.15, Scale, 0 + PosX, 0 + PosY, PosZ, new SolidBrush(Color.FromArgb(160, 123, 97)), false);
            Polygon TopLeftLeg = new Polygon(Reference, 0.15, 1, 0.15, Scale, 0 + PosX, 1.75 + PosY, PosZ, new SolidBrush(Color.FromArgb(160, 123, 97)), false);
            Polygon BottomRightLeg = new Polygon(Reference, 0.15, 1, 0.15, Scale, 0.75 + PosX, 0 + PosY, PosZ, new SolidBrush(Color.FromArgb(160, 123, 97)), false);
            Polygon BottomLeftLeg = new Polygon(Reference, 0.15, 1, 0.15, Scale, 0.75 + PosX, 1.75 + PosY, PosZ, new SolidBrush(Color.FromArgb(160, 123, 97)), false);
            Polygon TableTop = new Polygon(TopRightLeg, 1, 0.1, 2, Scale, 0, 0, 0, new SolidBrush(Color.FromArgb(172, 132, 104)), false);

            Obj3D.AddPolygon(TopLeftLeg, 0);
            Obj3D.AddPolygon(TopRightLeg, 0);
            Obj3D.AddPolygon(BottomLeftLeg, 1);
            Obj3D.AddPolygon(BottomRightLeg, 1);
            Obj3D.AddPolygon(TableTop, 2);
        }
    }

    class DeskPCA : IsometricObject
    {
        public DeskPCA(int Scale, Polygon Reference)
        {
            Polygon MonitorStand = new Polygon(Reference, 0.25, 0.05, 0.25, Scale, 0.15, 0.8, 0, new SolidBrush(Color.FromArgb(230, 230, 230)), false);
            Polygon MonitorArm = new Polygon(MonitorStand, 0.1, 0.5, 0.1, Scale, 0.05, 0.1, 0, new SolidBrush(Color.FromArgb(230, 230, 230)), false);
            //Polygon Monitor = new Polygon(MonitorStand, 0.1, 0.5, 1, Scale, 0.15, -.225, 0.25, new SolidBrush(Color.FromArgb(240, 240, 240)));
            //Polygon Monitor = new Polygon(MonitorArm, 0.1, 0.5625, 1, Scale, 0.15, -.45, -0.2825, new SolidBrush(Color.FromArgb(240, 240, 240)), false);
            Shader Mon = new Shader(Color.FromArgb(240,240,240));
            Polygon Monitor = new Polygon(MonitorArm, 0.1, 0.5625, 1, Scale, 0.15, -.45, -0.2825, Mon.ShadeBrush(0.9), ArtProcessing.TileBrush(new Bitmap(Image.FromFile(@"E:\Project Designs\RPG Game\Art\TestPCImage.png")), 1, 0.5625, Scale), Mon.ShadeBrush(1.0), false);
            ReferencerPoly = Monitor;
            //Polygon Monitor = new Polygon(MonitorArm, 0.1, 0.5, 1, Scale, 0.15, -.45, -0.25, new SolidBrush(Color.FromArgb(240, 240, 240)));

            Polygon Calculator = new Polygon(Reference, 0.2, 0.03, 0.15, Scale, 0.5, 1.5, 0, new SolidBrush(Color.FromArgb(230, 230, 230)), false);

            Polygon Keyboard = new Polygon(Reference, 0.25, 0.05, 0.75, Scale, 0.5, 0.5, 0, new SolidBrush(Color.FromArgb(230, 230, 230)), false);

            Polygon Mouse = new Polygon(Reference, 0.2, 0.075, 0.15, Scale, 0.6, 0.25, 0, new SolidBrush(Color.FromArgb(230, 230, 230)), false);
            Polygon MouseLeft = new Polygon(Mouse, 0.1, 0.01, 0.05, Scale, 0.01, 0.09, 0, new SolidBrush(Color.FromArgb(50,50, 50)), false);
            Polygon MouseRight = new Polygon(Mouse, 0.1, 0.01, 0.05, Scale, 0.01, 0.025, 0, new SolidBrush(Color.FromArgb(50,50, 50)), false);

            Obj3D.AddPolygon(MonitorStand, 0);
            Obj3D.AddPolygon(MonitorArm, 1);
            Obj3D.AddPolygon(Monitor, 2);
            Obj3D.AddPolygon(Calculator, 0);
            Obj3D.AddPolygon(Keyboard, 0);
            Obj3D.AddPolygon(Mouse, 0);
            Obj3D.AddPolygon(MouseLeft, 1);
            Obj3D.AddPolygon(MouseRight, 1);
        }
    }

    class Chair : IsometricObject
    {
        public Chair(int Scale, double PosX, double PosY, double PosZ, Polygon Reference)
        {
            SolidBrush C = new SolidBrush(Color.FromArgb(230, 230, 230));
            SolidBrush Gray = new SolidBrush(Color.FromArgb(50, 50, 50));
            SolidBrush Gray2 = new SolidBrush(Color.FromArgb(60, 60, 60));
            SolidBrush Gray1 = new SolidBrush(Color.FromArgb(40, 40, 40));

            Bitmap Logo = new Bitmap(100 * Scale, 100 * Scale);
            Graphics g = Graphics.FromImage(Logo);
            g.Clear(Color.FromArgb(60, 60, 60));
            g.DrawImage(Resources.Logo3, 0, 0, Logo.Width, Logo.Height);

            Polygon TopRightWheel = new Polygon(Reference, 0.1, 0.1, 0.1, Scale, 0.45 + PosX, 0.1 + PosY, 0 + PosZ, Gray1, false);
            Polygon TopLeftWheel = new Polygon(Reference, 0.1, 0.1, 0.1, Scale, 0.1 + PosX, 0.45 + PosY, 0 + PosZ, Gray1, false);
            Polygon BottomLeftWheel = new Polygon(Reference, 0.1, 0.1, 0.1, Scale, 0.45 + PosX, 0.8 + PosY, 0 + PosZ, Gray1, false);
            Polygon BottomRighttWheel = new Polygon(Reference, 0.1, 0.1, 0.1, Scale, 0.8 + PosX, 0.45 + PosY, 0 + PosZ, Gray1, false);

            Polygon TopRightBar = new Polygon(TopRightWheel, 0.1, 0.1, 0.37, Scale, 0, 0, 0, Gray, false);
            Polygon TopLeftBar = new Polygon(TopLeftWheel, 0.37, 0.1, 0.1, Scale, 0, 0, 0, Gray, false);
            Polygon BottomLeftBar = new Polygon(BottomLeftWheel, 0.1, 0.1, 0.37, Scale, 0, -0.27, 0, Gray, false);
            Polygon BottomRightBar = new Polygon(BottomRighttWheel, 0.37, 0.1, 0.1, Scale, -0.27, 0, 0, Gray, false);

            Polygon MainBar = new Polygon(Reference, 0.1, 0.6, 0.1, Scale, 0.45 + PosX, 0.45 + PosY, 0.1 + PosZ, Gray, false);

            Polygon Seat = new Polygon(MainBar, 0.75, 0.1, 0.75, Scale, -0.375, -0.375, 0, Gray2, false);

            Shader GrayShade = new Shader(Color.FromArgb(60, 60, 60));
            Polygon Back = new Polygon(Seat, 0.1, 0.85, 0.75, Scale, 0.72, 0, -0.1, GrayShade.ShadeBrush(0.9), new TextureBrush(Logo), GrayShade.ShadeBrush(1), false);

            Obj3D.AddPolygon(TopRightWheel, 0);
            Obj3D.AddPolygon(TopLeftWheel, 0);
            Obj3D.AddPolygon(BottomLeftWheel, 0);
            Obj3D.AddPolygon(BottomRighttWheel, 0);
            Obj3D.AddPolygon(TopRightBar, 1);
            Obj3D.AddPolygon(TopLeftBar, 1);
            Obj3D.AddPolygon(BottomLeftBar, 2);
            Obj3D.AddPolygon(BottomRightBar, 2);
            Obj3D.AddPolygon(MainBar, 1);
            Obj3D.AddPolygon(Seat, 1);
            Obj3D.AddPolygon(Back, 4);

            
        }
    }
}
