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
    internal class FreeCameraSubSys : ECSystem, ICameraSubSystem
    {
        private float _speed = 1.5f;
        public Entity CurrentEntity { get; set; }
        public List<Entity> CameraEntities { get; set; }


        public override void Init(Coordinator coordinator)
        {
            Signature signature = new Signature();
            signature.Set(coordinator.GetComponentType<CameraInfo>(), true);
            coordinator.SetSystemSignature<FreeCameraSubSys>(signature);

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

        #region ICameraSystem

        public Matrix4 GetViewMatrix(Coordinator coordinator, Entity entity)
        {
            coordinator.GetComponents<CameraInfo>(entity, out List<CameraInfo> cameraInfo);

            return Matrix4.LookAt(
                cameraInfo[0].Position, cameraInfo[0].Position + cameraInfo[0].Front, cameraInfo[0].Up);
        }

        public Matrix4 GetProjectionMatrix(Coordinator coordinator, Entity entity)
        {
            coordinator.GetComponents<CameraInfo>(entity, out List<CameraInfo> cameraInfo);

            return Matrix4.CreatePerspectiveFieldOfView(
                cameraInfo[0].FovRad, cameraInfo[0].AspectRatio, cameraInfo[0].DepthNear, cameraInfo[0].DepthFar);
        }

        public void CameraAction(Coordinator coordinator, Entity entity, params Object[] values)
        {
            var input = values[0] as KeyboardState;
            var time = (float)values[1];

            coordinator.GetComponents<CameraInfo>(entity, out List<CameraInfo> cameraInfoList);
            CameraInfo cameraInfo = cameraInfoList[0];

            if (input.IsKeyDown(Keys.W))
            {
                cameraInfo.Position += cameraInfo.Front * _speed * time;
            }

            if (input.IsKeyDown(Keys.S))
            {
                cameraInfo.Position -= cameraInfo.Front * _speed * time;
            }

            if (input.IsKeyDown(Keys.A))
            {
                cameraInfo.Position -= Right(cameraInfo.Front, cameraInfo.Up) * _speed * time;
            }

            if (input.IsKeyDown(Keys.D))
            {
                cameraInfo.Position += Right(cameraInfo.Front, cameraInfo.Up) * _speed * time;
            }

            if (input.IsKeyDown(Keys.Space))
            {
                cameraInfo.Position += cameraInfo.Up * _speed * time;
            }

            if (input.IsKeyDown(Keys.LeftShift))
            {
                cameraInfo.Position -= cameraInfo.Up * _speed * time;
            }

            cameraInfoList[0] = cameraInfo;
            coordinator.SetComponents<CameraInfo>(entity, cameraInfoList);
        }

        public void CameraAction(Coordinator coordinator, params Object[] values) => CameraAction(coordinator, CurrentEntity, values);
        #endregion

        private Vector3 Right(Vector3 front, Vector3 up) => Vector3.Normalize(Vector3.Cross(front, up));
    }
}
