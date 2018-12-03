/* View factor / area determination for solar flux computations.
 *
 * Loads satellite attitude evolution generated with AttitudePropagator.
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
 * Copyright©2018 Gesellschaft zur Foerderung der angewandten Forschung e.V. acting
 * on behalf of its Fraunhofer Institut für  Kurzzeitdynamik. All rights reserved.
 * 
 * Contact: max.gulde@emi.fraunhofer.de
 * 
 */

#region using

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace Thermal
{
    public class Data
    {
        #region fields

        UserSettings Set;

        List<Orientation> SunAngles;
        public int aIndex;

        List<string> AreatoWriteSun, PowerToWrite;
        StreamWriter sw;

        #endregion

        #region init

        public Data(UserSettings set)
        {
            Set = set;

            aIndex = Set.StartIndex;

            // sun angles
            Tools.ShowMsg("Reading in orientation data:");
            SunAngles = LoadAngles(Set.GetFileName(FileType.SolarAngles), Views.Sun);
            if (SunAngles != null)
            {
                Tools.ShowMsg(SunAngles.Count + " orientations relative to the sun.");
            }

            // init write block lists
            AreatoWriteSun = new List<string>();
            PowerToWrite = new List<string>();
        }

        #endregion

        #region load

        List<Orientation> LoadAngles(string path, Views view)
        {
            // check if exists
            if (!File.Exists(path))
            {
                Tools.ShowError("Data:LoadAngles: File <" + path + "> not found.");
                return null;
            }

            // read in file
            string[] lines = File.ReadAllLines(path);

            // clear current list
            List<Orientation> list = new List<Orientation>();

            // evaluate
            int lCount = 0;
            float Az, Az2, El, Ec;
            float diff, sum, az, el;
            foreach (string line in lines)
            {
                // ignore empty lines
                if (line.Length == 0)
                {
                    continue;
                }
                // ignore header
                if (line[0] == '"')
                {
                    continue;
                }
                // split
                string[] s = line.Split(',');
                // check for correct format
                switch (view)
                {
                    case Views.Sun:
                        if (s.Length != 4)
                        {
                            Tools.ShowError("Data:LoadAngles: wrong format.");
                            return list;
                        }

                        // Parse incoming data
                        Az = Convert.ToSingle(s[1], Settings.culture);
                        El = Convert.ToSingle(s[2], Settings.culture);
                        Ec = Convert.ToSingle(s[3], Settings.culture);

                        // add new entry
                        list.Add(new Orientation(s[0], Az, El, Ec));

                        // counter
                        lCount++;
                        break;
                }
            }
            return list;
        }

        #endregion

        #region update

        public void Next()
        {
            aIndex++;
        }

        public void Previous()
        {
            if (aIndex > 0)
            {
                aIndex--;
            }
        }

        public void SetIndex(int idx)
        {
            aIndex = idx;
        }

        public int GetIndex(DateTime date)
        {
            TimeSpan diff = date - SunAngles[0].Date;
            return (int)Math.Floor(diff.TotalSeconds / Set.TempResolution);
        }

        #endregion

        #region write

        // add line to lists
        public void AddLine(string line, Views view, DataType type, bool append, bool writenow)
        {
            // area list
            if (type == DataType.Area)
            {
                AreatoWriteSun.Add(line);
                // check if data should be written
                if (writenow || AreatoWriteSun.Count >= Settings.WriteBlockSize)
                {
                    WriteToFile(type, append);
                }
            }
            // power list
            else
            {
                PowerToWrite.Add(line);
                // check if data should be written
                if (writenow || PowerToWrite.Count >= Settings.WriteBlockSize)
                {
                    WriteToFile(type, append);
                }
            }
        }

        // write lists to file
        public void WriteToFile(DataType type, bool append)
        {
            string path = string.Empty;
            List<string> data;
            // decide on path
            if (type == DataType.Area)
            {
                path = Set.GetFileName(FileType.SolarSurfaces);
                data = AreatoWriteSun;
            }
            else
            {
                path = Set.GetFileName(FileType.SolarPower);
                data = PowerToWrite;
            }

            // write to file
            if (Set.WriteData)
            {
                sw = new StreamWriter(path, append);
                foreach (string line in data)
                {
                    sw.WriteLine(line);
                }
                sw.Close();
                data.Clear();
            }
        }

        #endregion

        #region access

        public float AzSun
        {
            get
            {
                // return 0 if not existing
                return (aIndex < SunAngles.Count) ? SunAngles[aIndex].Azimuth : 0;
            }
        }

        public float ElSun
        {
            get
            {
                float el = aIndex < SunAngles.Count ? SunAngles[aIndex].Elevation : 0;
                return el;
            }
        }

        public float IsInView
        {
            get
            {
                return aIndex < SunAngles.Count ? SunAngles[aIndex].Visibility : -1;
            }
        }

        public float Subsolar
        {
            get
            {
                return aIndex < SunAngles.Count ? SunAngles[aIndex].Eclipse : 0;
            }
        }

        public string DateSun
        {
            get
            {
                return aIndex < SunAngles.Count ? SunAngles[aIndex].Date.ToString() : string.Empty;
            }
        }

        public string DateSunString
        {
            get
            {
                return aIndex < SunAngles.Count ? SunAngles[aIndex].DateString : string.Empty;
            }
        }

        public bool Finished
        {
            get
            {
                return aIndex >= SunAngles.Count;
            }
        }

        public int SunAngleNum
        {
            get
            {
                return SunAngles.Count;
            }
        }

        #endregion
    }
}
