using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.EntityComponetSystem;
using MiniEngine.BasicComponents;
using OpenTK.Graphics.OpenGL4;

namespace MiniEngine.DeferredShading
{
    internal class QuadSubSys : ECSystem
    {
        Entity _quatEntity = -1;
        float[] _vertices = {
			    // positions        // normals        // texture Coords
			    -1.0f,  1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                 1.0f,  1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f,
                 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            };
        int[] _indices = {
                0, 1, 3,
                3, 2, 0
            };

        public override void Init(Coordinator coordinator)
        {
            Signature signature = new Signature();
            signature.Set(coordinator.GetComponentType<ObjectsGL>(), true);
            coordinator.SetSystemSignature<QuadSubSys>(signature);
        }

        public void AddEntity(Coordinator coordinator)
        {
            _quatEntity = coordinator.CreateEntity();
            coordinator.AddComponent(_quatEntity, new ObjectsGL(_vertices, _indices));
        }

        public void RenderQuad(Coordinator coordinator)
        {
            coordinator.GetComponents(_quatEntity, out List<ObjectsGL> ObjectsGLList);

            GL.BindVertexArray(ObjectsGLList[0].VAO);
            GL.DrawElements(BeginMode.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}
