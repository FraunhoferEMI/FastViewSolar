/* View factor / area determination for solar flux computations.
 *
 * Orthographic camera for 3D-display.
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
using Microsoft.Xna.Framework;

#endregion

namespace Thermal
{
    public class Camera
    {
        #region fields

        public Matrix Projection { get; private set; }
        public Matrix View { get; private set; }
        public Matrix World { get; private set; }

        Vector3 Target;

        public float Az { get; private set; }
        public float El { get; private set; }
        const float maxAz = 360;
        const float maxEl = 89.999f;

        float Size;

        UserSettings Set;

        #endregion

        public Camera(float size, UserSettings set)
        {
            // settings
            Set = set;

            // set view and projection matrices
            Projection = Matrix.Identity;
            View = Matrix.Identity;
            World = Matrix.Identity;

            // Target is always center
            Target = Vector3.Zero;

            // set elevation and azimuth
            Az = 0;
            El = 0;

            Size = size;

            // update
            Update();
        }

        public void Update()
        {
            // compute position from angles
            Vector3 Position;
            Position.X = (float)(Math.Cos(MathHelper.ToRadians(El + Set.ElevationOffset)) * Math.Sin(MathHelper.ToRadians(-Az + Set.AzimuthOffset)));
            Position.Y = (float)(Math.Cos(MathHelper.ToRadians(El + Set.ElevationOffset)) * Math.Cos(MathHelper.ToRadians(-Az + Set.AzimuthOffset)));
            Position.Z = (float)Math.Sin(MathHelper.ToRadians(El + Set.ElevationOffset));

            // set view and projection matrices
            View = Matrix.CreateLookAt(Position, Target, Vector3.Forward);
            Projection = Matrix.CreateOrthographic(Size, Size, Settings.NearPlane, Settings.FarPlane);
        }

        public void Rotate(float dAz, float dEl)
        {
            Az += dAz;
            El += dEl;
            // clamp
            Az = Az > maxAz ? Az - 360 : Az;
            Az = Az < -maxAz ? Az + 360 : Az;
            El = El > maxEl ? maxEl : El;
            El = El < -maxEl ? -maxEl : El;

            Update();
        }

        public void Orient(float az, float el)
        {
            Az = az;
            El = el;

            Update();
        }
    }
}
