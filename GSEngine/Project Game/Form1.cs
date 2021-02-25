using GSLibrary;
using GSEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Project_Game.Properties;

namespace Project_Game
{
    public partial class Form1 : Form
    {
        Memory Mem = new Memory();
        GameView A;
        int Scale = 4;
        MenuTemplate Template = new MenuTemplate(Color.FromArgb(64, 201, 124), Color.FromArgb(255, 255, 255), Color.FromArgb(64, 201, 124), Color.FromArgb(158, 216, 183));
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            Controls.Add(new TitleBar("GSEngine Demo"));

            A = new GameView(1200, Height - 32, Scale);
            A.Size = new System.Drawing.Size(1200, Height - 32);
            A.Location = new Point(0, 32);
            Controls.Add(A);
            A.LoadingScreenToggle(true);
            
            Load += Form1_Load;
            A.ClickEvents.Clicked += ClickEvents_Clicked;
            A.ClickEvents.Clicked3D += ClickEvents_Clicked3D;
            GV.Mem = Mem;
            GV.Mem.CodingSkills.Add(new Skill(-1) { Name = "GCode", ID = 1, SkillType = 0, Boosts = { new SkillBoost() { BoostAmount = 0, BoostReason = "" } }, SkillLevel = 75 });
            GV.Mem.CodingSkills.Add(new Skill(1) { Name = "NCode", ID = 2, SkillType = 0, Boosts = { new SkillBoost() { BoostAmount = 0, BoostReason = "" } } });
            GV.Mem.CodingSkills.Add(new Skill(2) { Name = "BCode", ID = 3, SkillType = 0, Boosts = { new SkillBoost() { BoostAmount = 0, BoostReason = "" } } });
            GV.Mem.CodingSkills.Add(new Skill(-1) { Name = "QCode", ID = 4, SkillType = 0, Boosts = { new SkillBoost() { BoostAmount = 0, BoostReason = "" } } });

            GV.Mem.ArtSkills.Add(new Skill(-1) { Name = "GArt", ID = 5, SkillType = 1, SkillLevel = 50 });

            Icon = Resources.Logo;
            Text = "Project Game";
        }

        

        void Form1_Load(object sender, EventArgs e)
        {
            if (true)
            {
                new Task(() =>
                {
                    CreateView();
                    A.RenderBackground();
                    try
                    {
                        System.Threading.Thread.Sleep(500);
                        A.FinishLoading();
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show(e1.Message);
                    }


                }).Start();
            }
            else
            {
                CreateView();
                A.RenderBackground();
                A.FinishLoading();
            }
        }

        void ClickEvents_Clicked(PolygonClickObject Object, int FaceClicked)
        {
            
        }
        void ClickEvents_Clicked3D(PolygonClickObject3D Object, Polygon Poly, int FaceClicked)
        {
            if (Object.Main == Desk3D.Obj3D)
            {
                if (Poly == Desk3D.GetTopPolygon() && FaceClicked == 2)
                {
                    MessageBox.Show("Hai2");
                }
            }
            else if (Object.Main == Computer.Obj3D)
            {
                if (Poly == Computer.ReferencerPoly)
                {
                    if (A.InteractionMenuID() == 1)
                    {
                        A.RemoveInteraction();
                    }
                    else
                    {
                        InteractionMenu Menu = new InteractionMenu(Template, 1);
                        Menu.Clicked += Menu_Clicked;
                        Menu.AddObject("Create Software", 101);
                        Menu.AddObject("Shop", 102);
                        Menu.AddObject("Skills", 103);
                        Menu.AddObject("Software", 107);
                        A.AddInteractionMenu(Menu, true);
                    }
                }
            }
        }

        void Menu_Clicked(int ClickID)
        {
            switch (ClickID)
            {
                #region Skills Button on PC
                case 103:
                    UIMenu MenuSkills = new UIMenu(A, Color.FromArgb(255, 255, 255), Color.FromArgb(64, 201, 124));
                    MenuSkills.Clicked+=Menu_Clicked;
                    //NewMenu.AddButton(100, 100, 100, 23, Template, "Hai", 104);
                    MenuSkills.AddText(0, 0, 700, 50, Template, "Skills", 1, 30);
                    int Loop = 0;
                    MenuSkills.AddText(0, 50, 350, 50, Template, "Coding", 1, 25);
                    MenuSkills.AddText(350, 50, 350, 50, Template, "Art", 1, 25);

                    foreach (Skill AB in Mem.CodingSkills)
                    {
                        MenuSkills.AddText(0, 100 + (Loop * 35), 85, 25, Template, AB.Name, 1, 15);
                        SkillBoost AC = AB.GetBoost();
                        MultiProgressBar Bar = MenuSkills.AddProgress(85, 100 + (Loop * 35), 160, 25, new MenuTemplate(Color.FromArgb(123, 217, 165), Color.White, Color.FromArgb(64, 201, 124), Color.White));
                        
                        MenuSkills.AddTextHint(255, 100 + (Loop * 35) + 3, 200, 125, Template
                            , "There are 2 bars, the stronger bar is the current skill level and the lighter bar is the prior-experience.\n\n" + AC.BoostReason
                            , 13 , false);

                        Bar.SetFirstValue(AB.SkillLevel);
                        
                        if (AC.BoostAmount != 0)
                        {
                            Bar.SetSecondValue(AC.BoostAmount);
                        }
                        
                        Loop++;
                    }
                    Loop = 0;
                    foreach (Skill AB in Mem.ArtSkills)
                    {
                        MenuSkills.AddText(600, 100 + (Loop * 35), 85, 25, Template, AB.Name, 1, 15);
                        SkillBoost AC = AB.GetBoost();
                        MultiProgressBar Bar = MenuSkills.AddProgress(440, 100 + (Loop * 35), 160, 25, new MenuTemplate(Color.FromArgb(123, 217, 165), Color.White, Color.FromArgb(64, 201, 124), Color.White));
                        MenuSkills.AddTextHint(410, 100 + (Loop * 35) + 3, 200, 125, Template, "There are 2 bars, the stronger bar is the current skill level and the lighter bar is the prior-experience.\n\n" + AC.BoostReason, 13, true);
                        Bar.SetFirstValue(AB.SkillLevel);

                        if (AC.BoostAmount != 0)
                        {
                            Bar.SetSecondValue(AC.BoostAmount);
                        }
                        Loop++;
                    }



                    A.AddItem(MenuSkills);
                    break;
                #endregion

                #region Create Software
                case 101:
                    UIMenu MenuCreateSoftware = new UIMenu(A, Color.FromArgb(255, 255, 255), Color.FromArgb(64, 201, 124));
                    MenuCreateSoftware.Clicked += Menu_Clicked;
                    MenuCreateSoftware.AddText(0, 0, 700, 50, Template, "Create Software", 1, 30);
                    //MenuCreateSoftware.AddButton(200, 100, 300, 25, Template, "Game Engine", 105, true, true);
                    //MenuCreateSoftware.AddButton(200, 130, 300, 25, Template, "Game", 106, true, true);
                    //MenuCreateSoftware.AddShape(190, 175, 320, 320, Template, 5, 3, -1);

                    //MenuCreateSoftware.AddShape(325, 100, 150, 150, Template, 4, 3);
                    MenuCreateSoftware.AddShape(275, 40, 150, 150, Template, 6, 3, 106);
                    MenuCreateSoftware.AddShape(61, 191, 150, 150, Template, 6, 3, 105);
                    A.AddItem(MenuCreateSoftware);
                    break;
                #endregion
                case 105:
                    UIMenu MenuCreateEngine = new UIMenu(A, Color.FromArgb(255, 255, 255), Color.FromArgb(64, 201, 124));
                    MenuCreateEngine.Clicked += Menu_Clicked;
                    MenuCreateEngine.AddText(0, 0, 700, 50, Template, "Create Game Engine", 1, 30);

                    MenuCreateEngine.AddText(50, 150, 200, 32, Template, "Game", 1, 20);
                    MenuCreateEngine.AddText(250, 150, 200, 32, Template, "Support", 1, 20);
                    MenuCreateEngine.AddText(450, 150, 200, 32, Template, "Extra", 1, 20);

                    MenuCreateEngine.AddText(50, 350, 200, 32, Template, "Engine", 1, 20);
                    MenuCreateEngine.AddText(250, 350, 200, 32, Template, "Story", 1, 20);
                    MenuCreateEngine.AddText(450, 350, 200, 32, Template, "Tools", 1, 20);

                    MenuCreateEngine.AddButton(55, 185, 190, 25, Template, "Skill Tree", 200, false, true);
                    MenuCreateEngine.AddButton(255, 185, 190, 25, Template, "Steering Wheel", 201, false, true);


                    A.AddItem(MenuCreateEngine);
                    break;

                case 107:
                    UIMenu MenuSoftware = new UIMenu(A, Color.FromArgb(255, 255, 255), Color.FromArgb(64, 201, 124));

                    A.AddItem(MenuSoftware);
                    break;
            }
        }
        Polygon WallLeft;
        Polygon Server1;
        Desk Desk3D;
        DeskPCA Computer;
        void CreateView()
        {
            //C is Background Render, A is Object Render
            Shader Wall = new Shader(Color.FromArgb(150, 150, 150));
            Polygon WallRight = new Polygon(5, 3, 0.25, Scale, 3.5, -1.24, 0.22, Wall.ShadeBrush(1), Wall.ShadeBrush(0.5), Wall.ShadeBrush(0.6), true);
            Polygon WallRightA = new Polygon(5, 3, 0.25, 3.25, -.99, Scale);
            A.AddBackgroundPoly(WallRight, 0);

            WallLeft = new Polygon(0.25, 3, 5, Scale, 3.5, -0.755, 0.45, Wall.ShadeBrush(0.5), Wall.ShadeBrush(.9), Wall.ShadeBrush(0.6), true);
            A.AddBackgroundPoly(WallLeft, 0);
            Image Temp = Resources.WoodtileGSRotated;
            Polygon Floor = new Polygon(5, 0.25, 5, Scale, 3.25, -1.25, 0, new SolidBrush(Color.FromArgb(120, 72, 0)), new SolidBrush(Color.FromArgb(109, 65, 0)), ArtProcessing.TileBrush(new Bitmap(Temp), 5, 5, Scale), true);
            A.AddBackgroundPoly(Floor, 1);


            Server1 = new Polygon(Floor, 0.75, 0.25, 1, Scale, 1.25, 0, 0, new SolidBrush(Color.FromArgb(64, 201, 124)), false);
            A.AddIsometricPoly(Server1 ,2);

            Polygon Server2 = new Polygon(Server1, 0.75, 0.25, 1, Scale, 0, 0, 0.1, new SolidBrush(Color.FromArgb(64, 201, 124)), false);
            A.AddIsometricPoly(Server2, 2);

            //Polygon Server3 = new Polygon(Floor, 1, 1, 1, Scale, 2, 2, 0, new SolidBrush(Color.FromArgb(64, 201, 124)), false);
            //A.AddIsometricPoly(Server3, 10);

            //Polygon Cube = new Polygon(Floor, 1, 1, 1, Scale, 2, 2, 0, ArtProcessing.TileBrush(new Bitmap(Temp), 1, 1, Scale), new TextureBrush(Resources.WoodtileShade), Wall.ShadeBrush(1), false);
            //A.AddIsometricPoly(Cube, 5);

            Desk3D = new Desk(Scale, 0.1, 1.5, 0, Floor);
            A.Add3DObject(Desk3D.Obj3D, 3);

            Computer = new DeskPCA(Scale, Desk3D.GetTopPolygon());
            A.Add3DObject(Computer.Obj3D, 6);

            Chair Chair = new Chair(Scale, 0.4, 2, 0, Floor);
            A.Add3DObject(Chair.Obj3D, 3);
        }

        
    }

    public class Skill
    {
        public string Name;
        public int ForkOf {
            get
            {
                return forkid;
            }
        }
        int forkid;
        public int ID;
        public double SkillLevel;
        public double BoostLevel;
        public int SkillType; //0 = Coding, 1 = Art
        public List<SkillBoost> Boosts = new List<SkillBoost>();
        public Skill(int ForkOf)
        {
            this.forkid = ForkOf;
        }

        public SkillBoost GetBoost()
        {

            Skill ForkParent = GV.Mem.GetSkillByID(forkid);


            double BoostsLevel = 0;
            string BoostsReason = "";
            if (ForkParent != null)
            {
                BoostsLevel = ForkParent.SkillLevel;
                BoostsReason = String.Format("Faster learning to {0}% provided by {1} due to being a fork", BoostsLevel, ForkParent.Name);
            }
            foreach (SkillBoost A in Boosts)
            {
                if (A.BoostAmount > BoostsLevel)
                {
                    BoostsLevel = A.BoostAmount;
                    BoostsReason = String.Format("Faster learning to {0}% provided by {1}", BoostsLevel, A.BoostReason);
                }
            }
            return new SkillBoost() { BoostAmount = BoostsLevel, BoostReason = BoostsReason };
        }
    }

    public class SkillBoost
    {
        public double BoostAmount;
        public string BoostReason;
    }

    public static class GV
    {
        public static Memory Mem;
    }

    public class Game
    {
        public int GameID;
        public List<int> Skills = new List<int>();
    }

    public class GameEngine
    {

    }

    public class Attribute
    {
        public string Name;
        public int Shooter, Platformer, Survival, Strategy, Racing, RPG, Story;
        public double Affect, Cost;
        public int Level;
        public int Research;
    }

    public class Theme
    {
        public int Shooter, Platformer, Survival, Strategy, Racing, RPG, Story;
        public string Name;
        public int Realistic, Arcade, Competitive, Casual;
    }

    public class GameCombos
    {
        List<Theme> Themes = new List<Theme>()
        {
            new Theme(){Name = "Future", Shooter = 3, Platformer = -3, Survival = 0, Strategy = 0, Racing = 2, RPG = 2, Story = 1, Realistic = 3, Arcade = 1, Competitive = -1, Casual= 1},
            new Theme(){Name = "Post Apocalyptic", Shooter = 2, Platformer = -3, Survival = 3, Strategy = 1, Racing = 1, RPG = 3, Story = 2, Realistic = 3, Arcade = 2, Competitive = -2, Casual = -3},
            new Theme(){Name = "Loot", Shooter = 2, Platformer = 2, Survival = -3, Strategy = -3, Racing = 1, RPG = 3, Story = -2, Realistic = -3, Arcade = 3, Competitive = 0, Casual = -3},
            new Theme(){Name = "Fantasy", Shooter = 1, Platformer = -3, Survival = -1, Strategy = 2, Racing = -1, RPG = 3, Story = 2, Realistic = -2, Arcade = 2, Competitive = 0, Casual = 0},
            new Theme(){Name = "Stealth", Shooter = 1, Platformer = 2, Survival = -3, Strategy = 0, Racing = -3, RPG = -2, Story = 2, Realistic = 2, Arcade = 2, Competitive = 1, Casual = -2},
            new Theme(){Name = "Zombies", Shooter = 3, Platformer = 1, Survival = 3, Strategy = -1, Racing = 1, RPG = -3, Story = -1, Realistic = 2, Arcade = 3, Competitive = 1, Casual = -3},
            new Theme(){Name = "Crime", Shooter = 3, Platformer = -1, Survival = 1, Strategy = 3, Racing = 1, RPG = -1, Story = 2, Realistic = 3, Arcade = 2, Competitive = 1, Casual = 0},
            new Theme(){Name = "City Building", Shooter = -3, Platformer = 0, Survival = -3, Strategy = 3, Racing = 0, RPG = 1, Story = 1, Realistic = 3, Arcade = 2, Competitive = -3, Casual = 3},
            new Theme(){Name = "Dungeon", Shooter = -2, Platformer = 3, Survival = -1, Strategy = -3, Racing = -3, RPG = 2, Story = 0, Realistic = 1, Arcade = 2, Competitive = 2, Casual = 2},
            new Theme(){Name = "War", Shooter = -2, Platformer = -2, Survival = 1, Strategy = 3, Racing = -1, RPG = 1, Story = 3, Realistic = 3, Arcade = 1, Competitive = 1, Casual = -2},
            new Theme(){Name = "Space", Shooter = -1, Platformer = -2, Survival = 2, Strategy = 1, Racing = 3, RPG = 1, Story = 0, Realistic = 2, Arcade = 1, Competitive = -3, Casual = 2},
        };

        List<Attribute> Attributes = new List<Attribute>()
        {
            new Attribute(){Name = "Skill tree", Shooter = 1, Platformer = 1, Survival = 1, Strategy = 1, Racing = 1, RPG = 1, Story = 0, Affect = 0.5, Cost = 0.75},
            new Attribute(){Name = "Steering Wheel", Shooter = -1, Platformer = -1, Survival = -1, Strategy = -1, Racing = 2, RPG = -1, Story = -1, Affect = 0.25, Cost = 0.25},
            new Attribute(){Name = "Controller", Shooter = 0, Platformer = 1, Survival = 0, Strategy = 0, Racing = 1, RPG = 1, Story = 1, Affect = 0.5, Cost = 0.5},
            new Attribute(){Name = "Cloud Saving", Shooter = 1, Platformer = 1, Survival = 1, Strategy = 1, Racing = 1, RPG = 1, Story = 1, Affect = 0.5, Cost = 0.25},
            new Attribute(){Name = "Microtransations", Shooter = -1, Platformer = -1, Survival = -1, Strategy = -1, Racing = -1, RPG = -1, Story = -1, Affect = 0, Cost = 0.25},
            new Attribute(){Name = "Multiplayer", Shooter = 1, Platformer = 1, Survival = 1, Strategy = -1, Racing = 1, RPG = 0, Story = -1, Affect = 0.5, Cost = 1},
            new Attribute(){Name = "Online", Shooter = 2, Platformer = -1, Survival = 2, Strategy = 1, Racing = 1, RPG = 1, Story = 0, Affect = 1.5, Cost = 2},
            new Attribute(){Name = "Physics", Shooter = 1, Platformer = 1, Survival = 1, Strategy = 0, Racing = 1, RPG = 1, Story = 1, Affect = 0, Cost = 1},
            new Attribute(){Name = "Cutscenes", Shooter = 1, Platformer = 0, Survival = -1, Strategy = 0, Racing = 0, RPG = 1, Story = 1, Affect = 0, Cost = 1},
            new Attribute(){Name = "Story Tree", Shooter = -1, Platformer = 2, Survival = -1, Strategy = -1, Racing = 0, RPG = 2, Story = 2, Affect = 0, Cost = 2.5},
            new Attribute(){Name = "Motion Capture", Shooter = 1, Platformer = -2, Survival = 1, Strategy = -1, Racing = -1, RPG = 1, Story = 2, Affect = 1.5, Cost = 2},
            new Attribute(){Name = "Mods", Shooter = 1, Platformer = 1, Survival = 1, Strategy = 2, Racing = 1, RPG = 1, Story = 1, Affect = 2, Cost = 3.5},
            new Attribute(){Name = "Level Editor", Shooter = 1, Platformer = 1, Survival = 1, Strategy = 1, Racing = 1, RPG = 1, Story = 1, Affect = 1.5, Cost = 3},
        };
    }


    public class Memory : GSMemory
    {
        public List<Skill> CodingSkills = new List<Skill>();
        public List<Skill> ArtSkills = new List<Skill>();
        public GameCombos Combos = new GameCombos();

        public Skill GetSkillByID(int ID)
        {
            foreach (Skill A in CodingSkills)
            {
                if (A.ID == ID)
                {
                    return A;
                }
            }
            foreach (Skill A in ArtSkills)
            {
                if (A.ID == ID)
                {
                    return A;
                }
            }
            return null;
        }
    }
}
