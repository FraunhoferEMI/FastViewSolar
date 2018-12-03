/* View factor / area determination for solar flux computations.
 *
 * Custom structs for 3D display and data mangement.
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Thermal
{
    // new vertex definition for position, normal, and color
    public struct VertexPositionNormalColor : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;

        public readonly static VertexDeclaration VertexDeclaration
        = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );

        public VertexPositionNormalColor(Vector3 Position, Vector3 Normal, Color Color)
        {
            this.Position = Position;
            this.Normal = Normal;
            this.Color = Color;
           
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    // struct to hold face information from object file
    public struct Face
    {
        public List<int> vertexIdx;
        public List<int> normalIdx;

        public Face(int[] vert, int[] norm)
        {
            vertexIdx = new List<int>();
            normalIdx = new List<int>();

            if (vert == null || vert.Length == 0)
            {
                Tools.ShowError("face: no vertices given.");
            }

            if (vert.Length != norm.Length)
            {
                Tools.ShowError("face: " + vert.Length + " vert indices, but " + norm.Length + " normal indices.");
            }

            // add indices (obj files start counting at 1)
            for (int i = 0; i < vert.Length; i++)
            {
                vertexIdx.Add(vert[i] - 1);
            }
            for (int i = 0; i < norm.Length; i++)
            {
                normalIdx.Add(norm[i] - 1);
            }
        }
    }

    // enumerator to decide between viewports
    public enum Views
    {
        Sun,
        All
    }

    // enumerator to decide for temperature type
    public enum TemperatureType
    {
        CoolCase,
        HotCase,
        Mean
    }

    // enumerator for data type
    public enum DataType
    {
        Area,
        Power
    }

    // enumerator for file types
    public enum FileType
    {
        SolarAngles,
        SolarSurfaces,
        Temperatures,
        SolarPower,
        SatModel
    }

    // struct to hold time and angle data
    public struct Orientation
    {
        public DateTime Date;       // date
        public string DateString;   // date as string
        public float Azimuth;       // azimuth
        public float Elevation;     // elevation
        public float Eclipse;       // angle between earth center -> sun and earth center -> satellite
        public float Visibility;    // is in view of sun or not?

        public Orientation(string date, float azimuth, float elevation, float eclipse)
        {
            Eclipse = eclipse;
            DateString = date;
            Azimuth = azimuth;
            Elevation = elevation;
            Date = DateTime.Parse(date, Settings.culture, System.Globalization.DateTimeStyles.None);
            Visibility = Math.Abs(Eclipse) < (Settings.EclipseAngle) ? 0 : 1;
        }

        public Orientation(float azimuth, float elevation)
        {
            Eclipse = 0;
            Azimuth = azimuth;
            Elevation = elevation;
            DateString = string.Empty;
            Date = DateTime.Now;
            Visibility = 1;
        }
    }

    // struct to hold temperature data
    public struct Temperature
    {
        public DateTime Date;       // date
        public string DateString;   // date as string
        public float Cool;          // cool case
        public float Hot;           // hot case
        public float Mean;          // mean

        public Temperature(string date, float cool, float hot, float mean)
        {
            DateString = date;
            Date = DateTime.Parse(date, Settings.culture, System.Globalization.DateTimeStyles.None);
            Cool = cool;
            Hot = hot;
            Mean = mean;
        }
    }

    // struct to hold model part and temperatures
    public struct SCPart
    {
        public string Name { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Face> Faces { get; set; }
        public List<Color> VColors { get; set; }
        public List<Temperature> Temperatures { get; set; }

        public SCPart(string name, List<Vector3> vertices, List<Vector3> normals, List<Face> faces, List<Color> colors, List<Temperature> temperatures)
        {
            Name = name;
            Vertices = vertices == null ? new List<Vector3>() : vertices;
            Normals = normals == null ? new List<Vector3>() : vertices;
            Faces = faces == null ? new List<Face>() : faces;
            VColors = colors == null ? new List<Color>() : colors;
            Temperatures = temperatures == null ? new List<Temperature>() : temperatures;
        }
    }
}
