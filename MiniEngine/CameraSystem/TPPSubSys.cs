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
    internal class TPPSubSys : ECSystem, ICameraSubSystem
    {
        public Entity CurrentEntity { get; set; }
        public List<Entity> CameraEntities { get; set; }

        private Dictionary<Entity, Entity> _cameraToOwnerObject;

        public override void Init(Coordinator coordinator)
        {
            Signature signature = new Signature();
            signature.Set(coordinator.GetComponentType<CameraInfo>(), true);
            coordinator.SetSystemSignature<TPPSubSys>(signature);

            CurrentEntity = -1;
            CameraEntities = new List<Entity>();
            _cameraToOwnerObject = new Dictionary<Entity, Entity>();
        }

        public void AddEntity(Coordinator coordinator, CameraInfo cameraInfo, Entity ownerObject)
        {
            Entity entity = coordinator.CreateEntity();
            coordinator.AddComponent(entity, cameraInfo);
            CameraEntities.Add(entity);
            _cameraToOwnerObject[entity] = ownerObject;
            if (CurrentEntity == -1) CurrentEntity = entity;
        }

        public void ChagneTrackingObject(Entity camera, Entity trackingObject)
        {
            if (!_cameraToOwnerObject.ContainsKey(camera)) throw new Exception("Given entity is not following camera.");
            _cameraToOwnerObject[camera] = trackingObject;
        }

        public void CameraAction(Coordinator coordinator, int entity, params object[] values) { }

        public void CameraAction(Coordinator coordinator, params object[] values) { }

        public Matrix4 GetProjectionMatrix(Coordinator coordinator, int entity)
        {
            coordinator.GetComponents<CameraInfo>(entity, out List<CameraInfo> cameraInfo);

            return Matrix4.CreatePerspectiveFieldOfView(
                cameraInfo[0].FovRad, cameraInfo[0].AspectRatio, cameraInfo[0].DepthNear, cameraInfo[0].DepthFar);
        }

        public Matrix4 GetViewMatrix(Coordinator coordinator, int entity)
        {
            coordinator.GetComponents<CameraInfo>(entity, out List<CameraInfo> cameraInfo);
            coordinator.GetComponents<Transformates>(_cameraToOwnerObject[entity], out List<Transformates> transformatesList);

            return Matrix4.LookAt(
                transformatesList[0].Position - transformatesList[0].SlopeVector * 1.6f +  Vector3.UnitY * 1.2f,
                transformatesList[0].Position + transformatesList[0].SlopeVector, 
                cameraInfo[0].Up
            );
        }
    }
}
