using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace MiniEngine.BasicComponents
{
    internal struct Transformates
    {
        public Vector3 Position { get; set; }

        public Vector3 SlopeVector { get; set; }

        public Matrix4 RotateMatrix { get; set; }

        public Matrix4 NoiseMatrix { get; set; } // <- tego nie powinno tutaj być, ale jest
                                                    // ze względu na drgania

        public Vector3 UpVector { get; set; }

        public float Scale { get; set; }


        #region Constructors
        public Transformates(Vector3 pos, Vector3 slVec, Matrix4 rotateM, Vector3 upVec, float scale)
        {
            Position = pos;
            SlopeVector = slVec;
            RotateMatrix = rotateM;
            UpVector = upVec;
            Scale = scale;
            NoiseMatrix = Matrix4.Identity;
        }

        public Transformates(Vector3 pos, Vector3 slVec, Vector3 upVec, float scale )
        {
            Position = pos;
            SlopeVector = slVec;
            RotateMatrix = Matrix4.Identity;
            UpVector = upVec;
            Scale = scale;
            NoiseMatrix = Matrix4.Identity;
        }
        #endregion
    }
}
