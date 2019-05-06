/* View factor / area determination for solar flux computations.
 *
 * General Settings as well as User Settings.
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
using Microsoft.Xna.Framework;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

#endregion

namespace Thermal
{
    #region settings

    public static class Settings
    {

        #region screen

        public static int SeparatorWidth = 2;
        public static int InsetSize = 250;
        public static int InsetPadding = 20;

        #endregion

        #region display

        public static bool ShowMouse = true;
        public static bool ShowText = true;
        public static bool UseGrayScale = true;
        public static bool ShowFrameRate = true;
        public static bool ShowInset = true;
        public static bool UseFullColorRange = true;
        public static float AzRate = 0.1f;
        public static float ElRate = 0.037f;
        public static float StepRate = 10f;
        public static bool UseModelColors = false;
        public static bool UseWireframe = false;

        #endregion

        #region camera

        public static float NearPlane = 0.001f;
        public static float FarPlane = 10f;
        public static string OutputResolution = "F6";

        #endregion

        #region color

        public static float StepColor = 0.30f;
        public static float StepGray = 0.07f;
        public static float TMax = 300;
        public static float TMin = 250;
        public static int ColorBarHeight = 250;
        public static int ColorBarWidth = 40;
        public static int ColorBarPadding = 20;
        public static int ColorBarSteps = 5;        // number of colorbar scale values displayed
        public static Color BackgroundColor = Color.White;

        #endregion

        #region text

        public static Vector2 TextPosition = new Vector2(30, 20);
        public static Vector2 TextSpacing = new Vector2(0, 20);
        public static Color TextColor = Color.Black;
        public static int WriteBlockSize = 10000; // data block size (in lines) until written into file

        #endregion

        #region control

        public static float RotSpeedAz = 1f;
        public static float RotSpeedEl = 1f;
        public static float KeyDelay = 200;         // [ms] minimum time between key presses
        public static int FastScrollSpeed = 25;     // number of steps in fast forward or fastbackward mode
        public static int FasterScrollSpeed = 250;  // number of steps in fast forward or fastbackward mode
        public static float SimDelay = 100;
        public static float TZoomSpeed = 2f/120f;   // colorbar zoom speed

        #endregion

        #region constants

        public static float EarthRadius = 6371;     // [km] earth radius
        public static float SunIntensity = 1367;    // [W/m^2] solar intensity
        public static float EclipseAngle = 0;

        #endregion

        #region regional

        public static CultureInfo culture = new CultureInfo("en-US");

        #endregion
    }

    public class UserSettings
    {
        #region satellite

        public string Name = "ERNST";                       // satellite name
        public float CellEfficiency = 0.3f;                 // efficiency of solar cells
        public int[] SolarCellPartIndex = new int[1] { 0 }; // part index in model
        public float FixedSunAz = 90;                       // (deg) Fixed sun azimuth
        public float SensorElevationOffset = 0;            // (deg) Offset of the sensor wrt nadir

        #endregion

        #region simulation

        public float TempResolution = 1;    // [s] temporal resolution of STK report
        public int StartIndex = 0;          // starting index of simulation data
        public bool WriteData = false;      // write simulation results to file
        public int ScreenSizePixel = 800;   // height of window
        public float ScreenSizeMeter = 1;   // [m] screen size
        float PixelPerViewport;
        float PixelArea;
        public float ModelScale = 1;        // scale of model

        #endregion

        #region orbit

        public float Altitude = 500f;

        #endregion

        #region files

        string fileSuffix = string.Empty;               // Suffix for data files
        public string BaseFolder = @"C:/";              // Base folder
        public string InFolder = @"data/";              // Input data directory
        public string OutFolder = @"results/";          // Results data folder
        public string SatelliteModel = "dummy.obj";     // *.obj file of satellite
        public string Suffix = "_i98_a500";             // Preferred suffix
        string In_SunAngles = string.Empty;             // Solar angles
        string In_TempSets = string.Empty;              // Temperatures (visualization only)
        string Out_AreaSunView = string.Empty;          // Solar view factors / areas
        string Out_Power = string.Empty;                // Solar panel surface * efficiency

        #endregion

        public UserSettings()
        {
            Update();
        }

        public void Update()
        {
            // set eclipse angle
            Settings.EclipseAngle = 90 + MathHelper.ToDegrees((float)Math.Acos(Settings.EarthRadius / (Settings.EarthRadius + Altitude)));

            In_SunAngles = BaseFolder + InFolder + Name + Suffix + "_SunAngles.csv";
            In_TempSets = BaseFolder + InFolder + "Temperatur" + Suffix + ".txt";

            Out_AreaSunView = BaseFolder + OutFolder + "Out_AreaSunView" + Suffix + ".txt";
            Out_Power = BaseFolder + OutFolder + "Out_Power" + Suffix + ".txt";

            PixelPerViewport = ScreenSizePixel * ScreenSizePixel;
            PixelArea = ScreenSizeMeter * ScreenSizeMeter / PixelPerViewport;
        }

        public string GetFileName(FileType type)
        {
            switch (type)
            {
                case FileType.SolarAngles:
                    return In_SunAngles;
                case FileType.SolarSurfaces:
                    return Out_AreaSunView;
                case FileType.SolarPower:
                    return Out_Power;
                case FileType.Temperatures:
                    return In_TempSets;
                case FileType.SatModel:
                    return SatelliteModel;
            }
            return string.Empty;
        }

        public float GetPixelPerViewport()
        {
            return PixelPerViewport;
        }

        public float GetPixelArea()
        {
            return PixelArea;
        }

        public string DataFolder
        {
            get
            {
                return BaseFolder + InFolder;
            }
        }
    }

    #endregion

    #region save & load

    // class for handling save and load operations of settings
    public static class SettingsRW
    {
        public static void SaveXML<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError("SettingsRW:SaveXML: Could not save object: " + ex.ToString());
            }

            Tools.ShowMsg("Saved settings to file.");

        }

        public static T LoadXML<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError("SettingsRW:LoadXML: Could not load object: " + ex.ToString());
            }

            Tools.ShowMsg("Loaded settings from file.");

            return objectOut;
        }

    }

    #endregion
}
