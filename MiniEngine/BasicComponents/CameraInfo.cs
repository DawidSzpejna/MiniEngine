using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace MiniEngine.BasicComponents
{
    internal struct CameraInfo
    {
        public Vector3 Position { get; set; }

        public readonly float DepthNear;

        public readonly float DepthFar;

        public float AspectRatio;

        public float FovRad;

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(FovRad);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                FovRad = MathHelper.DegreesToRadians(angle);
            }
        }

        public Vector3 Front { get; set; }
        public Vector3 Up { get; set; }

        #region Constructors
        public CameraInfo(Vector3 positin, float aspectRatio, float depthnear = 0.1f, float depthfar = 100.0f)
        {
            Position = positin;
            AspectRatio = aspectRatio;
            DepthNear = depthnear;
            DepthFar = depthfar;

            FovRad = MathHelper.PiOver2;
            Front = -Vector3.UnitZ;
            Up = Vector3.UnitY;
        }
        #endregion
    }
}
