using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using MiniEngine.Graphics;

namespace MiniEngine.BasicComponents
{
    internal class MeshInfo
    {
        public Matrix4 Model { get; set; }

        public Texture MeshTexture { get; set; }


        public MeshInfo(Matrix4 model, Texture texture)
        {
            Model = model;
            MeshTexture = texture;
        }
    }
}
