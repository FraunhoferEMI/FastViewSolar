/* View factor / area determination for solar flux computations.
 *
 * Loads and displays *.obj files. Does not use the integrated wavefront loader provided by monogame.
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
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

#endregion

namespace Thermal
{
    public class SatModel: DrawableGameComponent
    {
        #region fields

        UserSettings Set;

        #region gfx

        List<VertexBuffer> VBPartial;
        List<List<VertexPositionNormalColor>> VXPartial;

        #endregion

        #region model

        public List<Color> ColorList { get; private set; }      // list of colors used
        public int[] ColorHist_Sun { get; set; }                // color histogram
        public int[] ColorHist_Earth { get; set; }
        public List<string> PartNames { get; private set; }     // Model part names
        public List<SCPart> SatParts { get; private set; }      // 3D model part vertex information
        public float TMax { get; set; }                         // Max and min temperatures in set
        public float TMin { get; set; }
        int tIndex;                                             // index of current t set
        TemperatureType tType;                                  // type of data displayed

        public int VertexNumber
        {
            get
            {
                if (VXPartial == null)
                {
                    return -1;
                }
                else
                {
                    int c = 0;
                    for (int i = 0; i < VXPartial.Count; i++)
                    {
                        c += VXPartial[i].Count;
                    }
                    return c;
                }
            }
        }

        #endregion

        #endregion

        #region init

        // drawable model class, derived from game
        public SatModel(Game Game, string Path, UserSettings set) : base(Game)
        {
            Set = set;

            tIndex = 0;
            tType = TemperatureType.Mean;

            VBPartial = new List<VertexBuffer>();

            // load model from file
            LoadModel(Path);
        }

        #endregion

        #region load

        #region temperature

        public void LoadTemperatures(UserSettings set)
        {
            Set = set;

            Tools.ShowMsg("Loading Temperatures for ...");
            
            // read all lines from file
            if (!File.Exists(Set.GetFileName(FileType.Temperatures)))
            {
                Tools.ShowError("SatModel:LoadTemperatures: File <" + Set.GetFileName(FileType.Temperatures) + "> not found.");
                return;
            }
            string[] lines = File.ReadAllLines(Set.GetFileName(FileType.Temperatures));

            // evaluate lines
            List<string> PartOrder = new List<string>();
            string[] s1, s2;
            string date;
            int idx;
            int tCounter = 0;
            TMax = 0;
            TMin = 0;
            List<float> TList = new List<float>();
            foreach (string line in lines)
            {
                // check if zero length or commented
                if (line.Trim().Length == 0)
                    continue;

                switch(line.Trim().Substring(0,2))
                {
                    case "%#":  // commented line, ignore
                        continue;
                    case "% ":  // commented line, hold model part information
                        // separate model name from rest
                        s1 = line.Split(' ');
                        if (s1.Length == 3)
                        {
                            string name = s1[2].Trim();
                            PartOrder.Add(name);
                            Tools.ShowMsg("\t " + name + " ...");
                            // check, if part contained in satellite model. if not, add
                            if (PartNames.IndexOf(name) < 0)
                            {
                                PartNames.Add(name);
                                SatParts.Add(new SCPart(name, null, null, null, null, new List<Temperature>()));
                            }
                        }
                        
                        break;
                    default:    // holds temperature information
                        // (1) time
                        s1 = line.Split(';');
                        date = s1[0];
                        // (2) temperatures sets
                        for (int i = 1; i < s1.Length; i++)
                        {
                            // find index of corresponding SatPart
                            idx = PartNames.IndexOf(PartOrder[i - 1]);
                            if (idx < 0)
                            {
                                Tools.ShowError("SatModel:LoadTemperatures: Model part <" + PartOrder[i - 1] + "> not found.");
                                continue;
                            }
                            // separate set
                            s2 = s1[i].Split(':');
                            // add temperatures
                            TList.Add(Convert.ToSingle(s2[0], Settings.culture));
                            TList.Add(Convert.ToSingle(s2[1], Settings.culture));
                            TList.Add(Convert.ToSingle(s2[2], Settings.culture));
                            // satellite part contained in model
                            SatParts[idx].Temperatures.Add(new Temperature(date, TList[TList.Count - 3], TList[TList.Count - 2], TList[TList.Count - 1]));
                        }
                        break;
                }
                tCounter++;
            }

            // find extrema or use pre-set color scale
            if (Settings.UseFullColorRange)
            {
                TMax = TList.Max();
                TMin = TList.Min();
                Settings.TMax = TMax;
                Settings.TMin = TMin;
            }
            else
            {
                TMax = Settings.TMax;
                TMin = Settings.TMin;
            }

            Tools.ShowMsg("Loaded " + tCounter + " temperature sets (" + TMin.ToString(Settings.culture) + " K -> " + TMax.ToString(Settings.culture) + " K).");
        }

        #endregion

        #region model

        // load model
        void LoadModel(string Path)
        {
            // vertex list
            VXPartial = new List<List<VertexPositionNormalColor>>();

            // new satellite
            SatParts = new List<SCPart>();

            // read in file
            if (!File.Exists(Path))
            {
                Tools.ShowError("SatModel:LoadModel: File <" + Path + "> not found.");
                return;
            }
            Tools.ShowMsg("Loading Model.");
            string[] lines = File.ReadAllLines(Path);

            // check number of objects in file
            int oCounter = 0;
            PartNames = new List<string>();
            foreach (string line in lines)
            {
                // check if zero length
                if (line.Trim().Length == 0)
                    continue;
                if (line.Trim()[0] == 'o')
                {
                    PartNames.Add(line.Trim().Substring(2, line.Trim().Length - 2));
                    oCounter++;
                }
            }
            Tools.ShowMsg("Loading " + oCounter + " parts.");
            // make color list
            MakeColorList(oCounter);

            // setup variables
            int vCount = 0;
            int nCount = 0;
            int pCount = 0;

            // the .obj file format is described here: https://en.wikipedia.org/wiki/Wavefront_.obj_file
            int colorCount = -1;
            foreach (string line in lines)
            {
                // trim line
                string l = line.Trim();

                // check if zero length
                if (l.Length == 0)
                {
                    continue;
                }

                // check for first two characters to identify type
                string[] s,f;
                switch (l.Substring(0, 2))
                {
                    // new object
                    case "o ":
                        // move to new ModelPart
                        SatParts.Add(new SCPart(PartNames[pCount++], null, null, null, null, null));
                        // add number of previously loaded vertices and normals to counter
                        if (SatParts.Count > 1)
                        {
                            vCount += SatParts[SatParts.Count - 2].Vertices.Count;
                            nCount += SatParts[SatParts.Count - 2].Normals.Count;
                        }
                        // change display color
                        colorCount++;
                        colorCount = colorCount >= ColorList.Count ? 0 : colorCount;
                        Tools.ShowMsg("Part " + colorCount + ": " + line.Substring(2, line.Length - 2) + " with color = " + ColorList[colorCount].ToString());
                        break;
                    // vertex
                    case "v ":
                        s = l.Split(' ');
                        if (s.Length >= 4)
                        {
                            SatParts[SatParts.Count - 1].Vertices.Add(new Vector3(Convert.ToSingle(s[1], Settings.culture), Convert.ToSingle(s[2], Settings.culture), Convert.ToSingle(s[3], Settings.culture)));
                            SatParts[SatParts.Count - 1].Vertices[SatParts[SatParts.Count - 1].Vertices.Count - 1] *= Set.ModelScale;
                            // color
                            SatParts[SatParts.Count - 1].VColors.Add(ColorList[colorCount]);
                        }
                        break;
                    // face
                    case "f ":
                        s = l.Split(' ');
                        if (s.Length >= 4)
                        {
                            int[] v = new int[s.Length - 1];
                            int[] n = new int[s.Length - 1];
                            // extract vertex and normal indices
                            for (int i = 1; i < s.Length; i++)
                            {
                                f = s[i].Split('/');
                                v[i - 1] = Convert.ToInt32(f[0]) - vCount;
                                //n[i - 1] = Convert.ToInt32(f[2]) - nCount;
                                n[i - 1] = -1;
                            }
                            // add to face list
                            SatParts[SatParts.Count - 1].Faces.Add(new Face(v, n));
                        }
                        break;
                }
            }

            // update vertex colors without temperature data (-1)
            UpdateVertexColors(-1);

            // make vertex buffer
            UpdateVertexBuffer();
        }

        #endregion

        #endregion

        #region update

        // update vertex colors
        public void UpdateVertexColors(int dataSetIdx)
        {
            // write to vertex list
            VXPartial.Clear();
            int oCount = 0;
            foreach (SCPart o in SatParts)
            {
                VXPartial.Add(new List<VertexPositionNormalColor>());
                Color c = Color.White;
                // temperature to color
                if (!Settings.UseModelColors && dataSetIdx >= 0)
                {
                    switch (tType)
                    {
                        case TemperatureType.CoolCase:
                            c = Tools.GetColor(o.Temperatures[dataSetIdx].Cool, Settings.TMin, Settings.TMax);
                            break;
                        case TemperatureType.HotCase:
                            c = Tools.GetColor(o.Temperatures[dataSetIdx].Hot, Settings.TMin, Settings.TMax);
                            break;
                        case TemperatureType.Mean:
                            c = Tools.GetColor(o.Temperatures[dataSetIdx].Mean, Settings.TMin, Settings.TMax);
                            break;
                    }
                }
                else // use model colors
                {
                    c = ColorList[oCount];
                }

                // faces to vertexlist
                foreach (Face f in o.Faces)
                {
                    VXPartial[oCount].Add(new VertexPositionNormalColor(o.Vertices[f.vertexIdx[0]], new Vector3(1, 0, 0), c));
                    VXPartial[oCount].Add(new VertexPositionNormalColor(o.Vertices[f.vertexIdx[1]], new Vector3(0, 1, 0), c));
                    VXPartial[oCount].Add(new VertexPositionNormalColor(o.Vertices[f.vertexIdx[2]], new Vector3(0, 0, 1), c));
                }
                oCount++;
            }
            // update vertex buffer
            UpdateVertexBuffer();
        }

        // update vertex buffer
        void UpdateVertexBuffer()
        {
            VBPartial.Clear();
            for (int i = 0; i < VXPartial.Count; i++)
            {
                if (VXPartial[i].Count > 0)
                {
                    VBPartial.Add(new VertexBuffer(GraphicsDevice, VertexPositionNormalColor.VertexDeclaration, VXPartial[i].Count, BufferUsage.WriteOnly));
                    VBPartial[i].SetData(VXPartial[i].ToArray());
                }
            }
        }

    #endregion

        #region histogram

        void MakeColorList(int numColor)
        {
            // new list and histogram
            ColorList = new List<Color>();

            // make ColorList
            if (Settings.UseGrayScale)
            {
                Settings.StepGray = (float)1.0 / (numColor + 1);
                for (float g = Settings.StepGray; g < 1.0; g += Settings.StepGray)
                {
                    ColorList.Add(new Color(g, g, g, 1));
                }
            }
            else
            {
                Settings.StepColor = (float)3.0 / numColor;
                for (float r = Settings.StepColor; r < 1.0; r += Settings.StepColor)
                {
                    for (float g = Settings.StepColor; g < 1.0; g += Settings.StepColor)
                    {
                        for (float b = Settings.StepColor; b < 1.0; b += Settings.StepColor)
                        {
                            ColorList.Add(new Color(r, g, b, 1));
                        }
                    }
                }
            }

            // reset the color histogram
            ResetHistogram();
        }

        public void ResetHistogram()
        {
            ColorHist_Sun = new int[ColorList.Count];
            ColorHist_Earth = new int[ColorList.Count];

            for (int i = 0; i < ColorList.Count; i++)
            {
                ColorHist_Sun[i] = 0;
                ColorHist_Earth[i] = 0;
            }
        }

        #endregion

        #region draw

        // draw model part
        public void DrawModelPart(Effect Eff, Camera Cam, float LightIntensity, int ObjID)
        {
            // effect
            Eff.CurrentTechnique = !Settings.UseWireframe ? Eff.Techniques["BasicColorDrawing"] : Eff.Techniques["Wireframe"];
            Eff.Parameters["WorldViewProjection"].SetValue(Cam.World * Cam.View * Cam.Projection);
            Eff.Parameters["LightIntensity"].SetValue(LightIntensity);

            // draw
            if (ObjID < VBPartial.Count)
            {
                foreach (EffectPass pass in Eff.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.SetVertexBuffer(VBPartial[ObjID]);
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, VXPartial[ObjID].Count);
                }
            }
        }

        #endregion

        #region control

        public void NextSet(int step)
        {

            tIndex += step;
            tIndex = tIndex >= NumTSets ? 0 : tIndex;
        }

        public void PreviousSet(int step)
        {

            tIndex -= step;
            tIndex = tIndex < 0 ? NumTSets - 1 : tIndex;
        }

        public void ChangeType()
        {
            if (tType == TemperatureType.CoolCase)
            {
                tType = TemperatureType.HotCase;
                return;
            }
            if (tType == TemperatureType.HotCase)
            {
                tType = TemperatureType.Mean;
                return;
            }
            if (tType == TemperatureType.Mean)
            {
                tType = TemperatureType.CoolCase;
                return;
            }
        }

        #endregion

        #region access

        public int NumTSets
        {
            get
            {
                if (TSetsLoaded)
                {
                    return SatParts[0].Temperatures.Count;
                }
                return -1;
            }
        }

        public bool TSetsLoaded
        {
            get
            {
                return (SatParts != null && SatParts.Count > 0 && SatParts[0].Temperatures != null && SatParts[0].Temperatures.Count > 0);
            }
        }

        public string CurrentDateString
        {
            get
            {
                if (TSetsLoaded)
                {
                    return SatParts[0].Temperatures[tIndex].DateString;
                }
                return "NA";
            }
        }

        public DateTime CurrentDate
        {
            get
            {
                if (TSetsLoaded)
                {
                    return SatParts[0].Temperatures[tIndex].Date;
                }
                return DateTime.Now;
            }
        }

        public int CurrentIndex
        {
            get
            {
                return tIndex;
            }
        }

        public TemperatureType CurrentType
        {
            get
            {
                return tType;
            }
        }

        #endregion
    }
}
