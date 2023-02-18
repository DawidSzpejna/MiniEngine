using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.EntityComponetSystem;
using OpenTK.Mathematics;

namespace MiniEngine.CameraSystem
{
    public interface ICameraSubSystem
    {
        public Entity CurrentEntity { get; set; }

        public List<Entity> CameraEntities { get; set; }

        public Matrix4 GetViewMatrix(Coordinator coordinator, Entity entity);

        public Matrix4 GetProjectionMatrix(Coordinator coordinator, Entity entity);

        public void CameraAction(Coordinator coordinator, Entity entity, params Object[] values);

        public void CameraAction(Coordinator coordinator, params Object[] values);
    }
}
