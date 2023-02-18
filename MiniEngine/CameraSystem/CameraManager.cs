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
    public enum CameraTypes
    {
        StaticCamera,
        FollowingCamera,
        TPPCamera,
        FreeCamera
    }

    internal class CameraManager : ECSystem
    {
        public ICameraSubSystem CurrentCameraSubSys { get; private set; }

        public Entity CurrentEntity => CurrentCameraSubSys.CurrentEntity;

        private FreeCameraSubSys _freeCameraSys;

        private StaticCameraSubSys _staticCameraSys;

        private FollowingCameraSubSys _followingCameraSys;

        private TPPSubSys _tppSys;


        public override void Init(Coordinator coordinator)
        {
            Signature signature = new Signature();
            signature.Set(coordinator.GetComponentType<CameraInfo>(), true);
            coordinator.SetSystemSignature<CameraManager>(signature);

            _staticCameraSys     = coordinator.RegisterSystem<StaticCameraSubSys>();
            _followingCameraSys  = coordinator.RegisterSystem<FollowingCameraSubSys>();
            _tppSys              = coordinator.RegisterSystem<TPPSubSys>();
            _freeCameraSys       = coordinator.RegisterSystem<FreeCameraSubSys>();
            CurrentCameraSubSys = _staticCameraSys;
        }

        public void ChangeCamera(KeyboardState input)
        {
            if (input.IsKeyPressed(Keys.F1))
            {
                CurrentCameraSubSys = _staticCameraSys;
            }

            if (input.IsKeyPressed(Keys.F2))
            {
                CurrentCameraSubSys = _followingCameraSys;
            }

            if (input.IsKeyPressed(Keys.F3))
            {
                CurrentCameraSubSys = _tppSys;
            }

            if (input.IsKeyPressed(Keys.F4))
            {
                CurrentCameraSubSys = _freeCameraSys;
            }
        }

        public void UpdateAspectRatio(Coordinator coordinator, float aspectRatio)
        {
            foreach (var entity in Entities)
            {
                coordinator.GetComponents(entity, out List<CameraInfo> cameraInfosList);
                CameraInfo cameraInfo = cameraInfosList[0];
                cameraInfo.AspectRatio = aspectRatio;
                cameraInfosList[0] = cameraInfo;
                coordinator.SetComponents(entity, cameraInfosList);
            }
        }

        public void UpdateFov(Coordinator coordinator, float OffsetY)
        {
            foreach (var entity in Entities)
            {
                coordinator.GetComponents(entity, out List<CameraInfo> cameraInfosList);
                CameraInfo cameraInfo = cameraInfosList[0];
                cameraInfo.Fov -= OffsetY;
                cameraInfosList[0] = cameraInfo;
                coordinator.SetComponents(entity, cameraInfosList);
            }
        }

        #region Adding cameras
        public void AddStaticCamera(Coordinator coordinator, CameraInfo cameraInfo)
            => _staticCameraSys.AddEntity(coordinator, cameraInfo);
        
        public void AddFollowingCamera(Coordinator coordinator, CameraInfo cameraInfo, Entity trackingObject)
            => _followingCameraSys.AddEntity(coordinator, cameraInfo, trackingObject);

        public void AddTPPCamera(Coordinator coordinator, CameraInfo cameraInfo, Entity ownerObject)
            => _tppSys.AddEntity(coordinator, cameraInfo, ownerObject);

        public void AddFreeCamera(Coordinator coordinator, CameraInfo cameraInfo)
            => _freeCameraSys.AddEntity(coordinator, cameraInfo);
        #endregion


        #region Switch of ICameraSubSystem
        public Matrix4 GetViewMatrix(Coordinator coordinator)
            => CurrentCameraSubSys.GetViewMatrix(coordinator, CurrentCameraSubSys.CurrentEntity);

        public Matrix4 GetViewMatrix(Coordinator coordinator, Entity entity)
            => CurrentCameraSubSys.GetViewMatrix(coordinator, entity);

        public Matrix4 GetProjectionMatrix(Coordinator coordinator, Entity entity)
            => CurrentCameraSubSys.GetProjectionMatrix(coordinator, entity);

        public Matrix4 GetProjectionMatrix(Coordinator coordinator)
            => CurrentCameraSubSys.GetProjectionMatrix(coordinator, CurrentCameraSubSys.CurrentEntity);

        public void CameraAction(Coordinator coordinator, Entity entity, params Object[] values)
            => CurrentCameraSubSys.CameraAction(coordinator, entity, values);
        
        public void CameraAction(Coordinator coordinator, params Object[] values)
            => CurrentCameraSubSys.CameraAction(coordinator, values);
        #endregion
    }
}
