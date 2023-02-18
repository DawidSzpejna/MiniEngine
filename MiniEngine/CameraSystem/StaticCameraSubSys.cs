using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using MiniEngine.EntityComponetSystem;
using MiniEngine.Graphics;
using MiniEngine.BasicComponents;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiniEngine.CameraSystem
{
    internal class StaticCameraSubSys : ECSystem, ICameraSubSystem
    {
        public Entity CurrentEntity { get; set; }
        public List<Entity> CameraEntities { get; set; }

        public override void Init(Coordinator coordinator)
        {
            Signature signature = new Signature();
            signature.Set(coordinator.GetComponentType<CameraInfo>(), true);
            coordinator.SetSystemSignature<StaticCameraSubSys>(signature);

            CurrentEntity = -1;
            CameraEntities = new List<Entity>();
        }

        public void AddEntity(Coordinator coordinator, CameraInfo cameraInfo)
        {
            Entity entity = coordinator.CreateEntity();
            coordinator.AddComponent(entity, cameraInfo);
            CameraEntities.Add(entity);
            if (CurrentEntity == -1) CurrentEntity = entity;
        }

        public void CameraAction(Coordinator coordinator, int entity, params object[] values)  {}

        public void CameraAction(Coordinator coordinator, params object[] values) {}

        public Matrix4 GetProjectionMatrix(Coordinator coordinator, int entity)
        {
            coordinator.GetComponents<CameraInfo>(entity, out List<CameraInfo> cameraInfo);

            return Matrix4.CreatePerspectiveFieldOfView(
                cameraInfo[0].FovRad, cameraInfo[0].AspectRatio, cameraInfo[0].DepthNear, cameraInfo[0].DepthFar);
        }

        public Matrix4 GetViewMatrix(Coordinator coordinator, int entity)
        {
            coordinator.GetComponents<CameraInfo>(entity, out List<CameraInfo> cameraInfo);

            return Matrix4.LookAt(
                cameraInfo[0].Position, Vector3.Zero, cameraInfo[0].Up);
        }
    }
}
