using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.BasicComponents
{
    internal class VerticesData
    {
        public float[] Vertices { get; private set; }

        public int[] Indices { get; private set; }

        public int Count => LengthInBytes / Stride;

        public int LengthInBytes => Vertices.Length * sizeof(float);

        public static readonly int Elements = 3;

        public static readonly int SizeOfAttribute1 = 3 * sizeof(float);

        public static readonly int SizeOfAttribute2 = 3 * sizeof(float);

        public static readonly int SizeOfAttribute3 = 2 * sizeof(float);

        public static readonly int Stride = SizeOfAttribute1 + SizeOfAttribute2 + SizeOfAttribute3;


        #region Constructors
        public VerticesData(float[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
        #endregion
    }
}
