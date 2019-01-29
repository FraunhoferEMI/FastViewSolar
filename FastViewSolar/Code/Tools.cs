/* View factor / area determination for solar flux computations.
 *
 * Misc.
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

#endregion

namespace Thermal
{
    public static class Tools
    {
        #region fields

        public static int errCount = 0;

        #endregion

        #region messages

        public static void ShowError(string text)
        {
            Console.WriteLine("Error " + errCount + ": " + text);
            errCount++;
        }

        public static void ShowMsg(string text)
        {
            Console.WriteLine(text);
        }

        #endregion

        #region colors

        public static Color GetColor(float v, float min, float max)
        {
            Vector3 cVec = Vector3.Zero;
            float dv;

            if (v < min)
                v = min;
            if (v > max)
                v = max;
            dv = max - min;

            if (v < (min + 0.25 * dv))
            {
                cVec.X = 0;
                cVec.Y = 4 * (v - min) / dv;
                cVec.Z = 1;
            }
            else if (v < (min + 0.5 * dv))
            {
                cVec.X = 0;
                cVec.Y = 1;
                cVec.Z = 1 + 4 * (min + 0.25f * dv - v) / dv;
            }
            else if (v < (min + 0.75 * dv))
            {
                cVec.X = 4 * (v - min - 0.5f * dv) / dv;
                cVec.Y = 1;
                cVec.Z = 0;
            }
            else
            {
                cVec.X = 1;
                cVec.Y = 1 + 4 * (min + 0.75f * dv - v) / dv;
                cVec.Z = 0;
            }

            return (new Color(cVec));
        }

        #endregion
    }
}
