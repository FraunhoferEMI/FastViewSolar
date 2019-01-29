/* View factor / area determination for solar flux computations.
 *
 * Employs orthographic projections. Uses the monogame framework for display.
 * 
 * Requires the following inputs
 *  1. Wavefront file (*.obj, ASCII) of the satellite. Each object within the file
 *      will be treated as a unique surface.
 *  2. Attitude evolution file. Create this file using the orekit software
 *      "AttitudePropagator".
 *  3. A settings file in *.xml format. Here, you need to specify the satellite
 *      name, input and output directories. The settings file must be located in
 *      the same folder as the executable.
 *      
 *  The current software requires a lot of refactoring and is part of a larger package
 *  to also a) compute thermal energy transfer and b) display the results.
 * 
 * The Fraunhofer-Gesellschaft zur Foerderung der angewandten Forschung e.V.,
 * Hansastrasse 27c, 80686 Munich, Germany (further: Fraunhofer) is the holder
 * of all proprietary rights on this computer program. You can only use this
 * computer program if you have closed a license agreement with Fraunhofer or
 * you get the right to use the computer program from someone who is authorized
 * to grant you that right. Any use of the computer program without a valid
 * license is prohibited and liable to prosecution.
 * 
 * The use of this software is only allowed under the terms and condition of the
 * General Public License version 2.0 (GPL 2.0).
 * 
 * Copyright©2019 Gesellschaft zur Foerderung der angewandten Forschung e.V. acting
 * on behalf of its Fraunhofer Institut für  Kurzzeitdynamik. All rights reserved.
 * 
 * Contact: max.gulde@emi.fraunhofer.de
 * 
 */

#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Thermal
{
    public class Main : Game
    {
        #region fields

        readonly string SettingsFile = @"set.xml";

        // gfx
        GraphicsDeviceManager GDM;
        Camera CamSun;
        Effect Eff;
        Viewport ViewSun;
        SpriteBatch SB;

        // model
        SatModel Sat;

        // io
        Input IN;
        Text TXT;
        Point MouseOldPosition;
        bool SimIsRunning = false;
        bool SimIsOver = false;
        bool OrientationChanged = false;

        // data
        Data Dat;
        UserSettings Set;

        // performance
        int StepsLast = 0;
        int StepsNow = 0;

        // timing
        Stopwatch sw_Step = new Stopwatch();

        #endregion

        #region init

        public Main()
        {
            GDM = new GraphicsDeviceManager(this);
            // set graphics profile
            GDM.GraphicsProfile = GraphicsProfile.HiDef;
            // set content directory
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // load settings
            Set = SettingsRW.LoadXML<UserSettings>(SettingsFile);
            if (Set == null)
            {
                Set = new UserSettings();
                SettingsRW.SaveXML(Set, SettingsFile);
               
                Tools.ShowMsg("Did not find settings file, create file with default settings.");
            }
            Set.Update();
            
            // set screen resolution
            Tools.ShowMsg("Initializing.");
            Tools.ShowMsg("Pixel size is <" + Set.GetPixelArea() + "> m^2.");
            GDM.PreferredBackBufferWidth = Set.ScreenSizePixel;
            GDM.PreferredBackBufferHeight = Set.ScreenSizePixel;
            GDM.SynchronizeWithVerticalRetrace = false; // for higher frame rates
            GDM.ApplyChanges();

            PrintSettings();

            // setup viewports
            ViewSun = new Viewport(0, 0, Set.ScreenSizePixel, Set.ScreenSizePixel);

            // setup spritebatch
            SB = new SpriteBatch(GraphicsDevice);

            // display mouse
            IsMouseVisible = Settings.ShowMouse;

            // allow framerate > 60
            IsFixedTimeStep = false;

            base.Initialize();
        }

        void PrintSettings()
        {
            if (Set == null)
            {
                return;
            }
            Tools.ShowMsg("# Satellite parameters");
            Tools.ShowMsg("\t Altitude = " + Set.Altitude.ToString("F2", Settings.culture) + " km");
            Tools.ShowMsg("\t Eclipe angle = " + Settings.EclipseAngle.ToString("F2", Settings.culture) + " deg");
        }

        #endregion

        #region load

        protected override void LoadContent()
        {
            Tools.ShowMsg("Loading Content:");

            // io
            IN = new Input();
            TXT = new Text(this, SB, Set);
            MouseOldPosition = IN.MousePosition;

            // // // gfx
            // camera 1
            CamSun = new Camera(Set.ScreenSizeMeter, Set);

            // settings and data as in settings
            Reload();

            // load effects
            Eff = Content.Load<Effect>("Default");

            // timing
            sw_Step.Start();

            // set first camera position
            CamSun.Orient(Dat.AzSun, Dat.ElSun);
        }

        void Reload()
        {
            // load satellite model
            Sat = new SatModel(this, Set.DataFolder + Set.GetFileName(FileType.SatModel), Set);

            // load angular data
            Dat = new Data(Set);

            WriteHeader();
        }

        void WriteHeader()
        {
            if (Set.WriteData)
            {
                // // // write out file headers
                // area
                List<string> Header_Area = new List<string>();
                Header_Area.Add("% File generated on " + DateTime.Now.ToString());
                Header_Area.Add("% 1: time [s]");
                int c = 2;
                foreach (string line in Sat.PartNames)
                {
                    Header_Area.Add("% " + c + ": " + line + " [m^2]");
                    c++;
                }
                // power
                List<string> Header_Power = new List<string>();
                Header_Power.Add("% File generated on " + DateTime.Now.ToString());
                Header_Power.Add("% 1: time [s]");
                Header_Power.Add("% 2: power [W]");
                // write to file - create files
                Dat.AddLine(Header_Area[0], Views.Sun, DataType.Area, false, true);
                Dat.AddLine(Header_Power[0], Views.Sun, DataType.Power, false, true);
                // write remaining data
                for (int i = 1; i < Header_Area.Count; i++)
                {
                    Dat.AddLine(Header_Area[i], Views.Sun, DataType.Area, true, true);
                }
                for (int i = 1; i < Header_Power.Count; i++)
                {
                    Dat.AddLine(Header_Power[i], Views.Sun, DataType.Power, true, true);
                }
            }
            // check if daten to be written to file
            else
            {
                Tools.ShowMsg("Attention: Simulation data will not be saved to file!");
            }
        }

        #endregion

        #region update

        protected override void Update(GameTime gameTime)
        {
            // text update
            TXT.Update();

            // histogram update
            Sat.ResetHistogram();

            // performance
            if (sw_Step.ElapsedMilliseconds >= 1000)
            {
                StepsNow = Dat.aIndex - StepsLast;
                StepsLast = Dat.aIndex;
                sw_Step.Restart();
            }

            #region general

            // exit
            if (IN.Exit)
            {
                Exit();
            }

            // load settings
            if (IN.LoadSettings)
            {
                Reload();
            }

            // save settings
            if (IN.SaveSettings)
            {
                SettingsRW.SaveXML(Set, SettingsFile);
            }

            if (IN.ToggleWrite)
            {
                Set.WriteData = !Set.WriteData;
                Tools.ShowMsg("Set WriteData to <" + Set.WriteData.ToString() + ">.");
                WriteHeader();
            }

            #endregion

            #region camera mouse control

            // mouse must be within bounds of viewport
            if (ViewSun.Bounds.Contains(IN.MousePosition))
            {
                // start simulation
                if (IN.Start && !SimIsOver)
                {
                    SimIsRunning = true;
                }

                if (!SimIsRunning)
                {
                    // camera control
                    if (IN.ClickL && MouseOldPosition != IN.MousePosition)
                    {
                        Point dRot = MouseOldPosition - IN.MousePosition;
                        if (ViewSun.Bounds.Contains(IN.MousePosition))
                        {
                            CamSun.Rotate(-dRot.X * Settings.RotSpeedAz, dRot.Y * Settings.RotSpeedEl);
                        }
                        OrientationChanged = true;
                    }
                }
            }

            #endregion

            #region simulation control

            // next data set
            if (SimIsRunning)
            {
                CamSun.Orient(Dat.AzSun, Dat.ElSun);

                // pause
                if (IN.Pause)
                {
                    SimIsRunning = false;
                }
            }
            else
            {
                if (IN.Next)
                {
                    Dat.Next();
                    OrientationChanged = true;
                    // orient camera
                    CamSun.Orient(Dat.AzSun, Dat.ElSun);
                }
                if (IN.Previous)
                {
                    Dat.Previous();
                    OrientationChanged = true;
                    // orient camera
                    CamSun.Orient(Dat.AzSun, Dat.ElSun);
                }
            }

            #endregion

            #region text display

            // write Infos
            TXT.WriteLine("Sun View", Views.Sun);
            TXT.WriteLine("Azimuth = " + CamSun.Az.ToString("F1", Settings.culture), Views.Sun);
            TXT.WriteLine("Elevation = " + CamSun.El.ToString("F1", Settings.culture), Views.Sun);
            TXT.WriteLine("Subsolar = " + Dat.Subsolar.ToString("F1", Settings.culture), Views.Sun);

            TXT.WriteLine(Sat.VertexNumber / 3 + " faces | write data = " + Set.WriteData.ToString() + " <F12>", Views.All);
            TXT.WriteLine("Date = " + Dat.DateSun, Views.All);
            TXT.WriteLine("Step " + Dat.aIndex + " / " + Dat.SunAngleNum + " (" + StepsNow + "/s - " + ((float)Dat.aIndex / Dat.SunAngleNum * 100).ToString("F2", Settings.culture) + " %)", Views.All);
            TXT.WriteLine("Exit <Esc>, Reload settings <F9>, Save settings <F5>", Views.All);

            #endregion

            // set new position
            MouseOldPosition = IN.MousePosition;

            base.Update(gameTime);
        }

        #endregion

        #region draw

        protected override void Draw(GameTime gameTime)
        {
            // clear
            GraphicsDevice.Clear(Settings.BackgroundColor);

            #region draw models and compute areas

            List<float> AreasS;

            // sun view
            AreasS = DrawModel(ViewSun, CamSun, SimIsRunning || OrientationChanged, Dat.IsInView);

            #endregion

            #region generate output

            if (SimIsRunning || OrientationChanged)
            {
                // (1) compute areas from histogram to string
                List<string> sAreasS = new List<string>();
                for (int i = 0; i < Sat.PartNames.Count; i++)
                {
                    sAreasS.Add(AreasS[i].ToString(Settings.OutputResolution, Settings.culture));
                }
                // (2) compute power from solar cell area
                float Power = 0;
                foreach (int i in Set.SolarCellPartIndex)
                {
                    Power += AreasS[i] * Set.CellEfficiency * Settings.SunIntensity;
                    //Console.WriteLine("Power at index <" + i + "> is " + Power + " W");
                }

                // (3a) write data into file
                if (SimIsRunning)
                {
                    // write areas
                    Dat.AddLine(Dat.DateSunString + ";" + String.Join(";", sAreasS.ToArray()), Views.Sun, DataType.Area, true, false);

                    // write power
                    Dat.AddLine(Dat.DateSunString + ";" + Power.ToString(Settings.OutputResolution, Settings.culture), Views.Sun, DataType.Power, true, false);

                    // next angular data set
                    Dat.Next();

                    // check if end
                    if (Dat.Finished)
                    {
                        SimIsRunning = false;
                        SimIsOver = true;
                        // write remaining data
                        Dat.WriteToFile(DataType.Area, true);
                        Dat.WriteToFile(DataType.Power, true);
                    }
                }
                // (3b) otherwise display in console
                if (OrientationChanged)
                {
                    Tools.ShowMsg("- - - - - Sun - - - - -");
                    for (int i = 0; i < AreasS.Count; i++)
                    {
                        Tools.ShowMsg(i + ": Area of <" + Sat.PartNames[i] + "> = " + sAreasS[i] + " m^2.");
                    }
                    Tools.ShowMsg("Generated power = " + Power.ToString("F2",Settings.culture) + " W.");
                }
            }

            OrientationChanged = false;

            #endregion

            #region draw text

            if (Settings.ShowText)
            {
                GraphicsDevice.Viewport = ViewSun;
                SB.Begin();
                TXT.Draw(true);
                SB.End();
            }

            #endregion

            base.Draw(gameTime);
        }

        public List<float> DrawModel(Viewport view, Camera cam, bool computeArea, float modelFractionIlluminated)
        {
            // set viewport
            GraphicsDevice.Viewport = view;
            // new area list
            List<float> Areas = new List<float>();
            // draw model
            for (int i = 0; i < Sat.PartNames.Count; i++)
            {
                Sat.DrawModelPart(Eff, cam, 1.0f, i);
            }
            if (computeArea)
            {
                // new occlusion query
                OcclusionQuery occQuery = new OcclusionQuery(GraphicsDevice);
                // compute areas
                for (int i = 0; i < Sat.PartNames.Count; i++)
                {
                    // occlusion query
                    occQuery.Begin();

                    Sat.DrawModelPart(Eff, cam, 1.0f, i);

                    occQuery.End();

                    while (!occQuery.IsComplete)
                    {
                        // do nothing until query is complete
                    }

                    float Area = Math.Abs(occQuery.PixelCount * Set.GetPixelArea() * modelFractionIlluminated);
                    Areas.Add(Area);
                }
                // dispose
                occQuery.Dispose();
            }
            return Areas;
        }


        #endregion

    }
}
