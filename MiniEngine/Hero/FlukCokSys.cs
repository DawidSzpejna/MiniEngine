using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.EntityComponetSystem;
using MiniEngine.BasicComponents;
using MiniEngine.Loader;
using MiniEngine.Lights;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;

namespace MiniEngine.Hero
{
    internal class FlukCokSys : ECSystem
    {
        private Entity _entityHero = -1;
        
        public Entity EntityHero => _entityHero;
        
        private Rotator _heroRotator;
        
        private Rotator _reflector1Rotator;
        
        private Rotator _reflector2Rotator;
        
        private Path _heroPath;
        
        private Entity _reflector1;
        
        private Entity _reflector2;
        
        private bool _CreazyMoving;


        public override void Init(Coordinator coordinator)
        {
            Signature signature = new Signature();
            signature.Set(coordinator.GetComponentType<MeshInfo>(), true);
            signature.Set(coordinator.GetComponentType<VerticesData>(), true);
            signature.Set(coordinator.GetComponentType<ObjectsGL>(), true);
            signature.Set(coordinator.GetComponentType<Transformates>(), true);
            coordinator.SetSystemSignature<FlukCokSys>(signature);

            _heroPath = new Path(null);
        }

        public void AddEntity(Coordinator coordinator, Transformates transformates, string name, string dir)
        {
            _entityHero = coordinator.CreateEntity();

            var meshes = ColladaLoad.Load(name, dir);
            foreach (var mesh in meshes)
            {
                coordinator.AddComponent(_entityHero, new MeshInfo(mesh.Model, mesh.MeshTextures));
                coordinator.AddComponent(_entityHero, new VerticesData(mesh.Vertices, mesh.Indices));
                coordinator.AddComponent(_entityHero, new ObjectsGL(mesh.Vertices, mesh.Indices));
            }
            coordinator.AddComponent(_entityHero, transformates);

            _heroRotator = new Rotator();
            _reflector1Rotator = new Rotator();
            _reflector2Rotator = new Rotator();
        }

        public void AddPath(Coordinator coordinator, List<Vector3> path)
        {
            if (path.Count == 0) return;

            coordinator.GetComponent(_entityHero, 0, out Transformates transformates);
            transformates.Position = path[0];
            coordinator.SetComponent(_entityHero, 0, transformates);

            _heroPath = new Path(path);
        }


        #region Movements
        public void Moving(Coordinator coordinator, float time)
        {
            if (!_heroPath.IsPathToDo) return;

            coordinator.GetComponent(_entityHero, 0, out Transformates heroTransformates);
            coordinator.GetComponent(_reflector1, 0, out Transformates refl1Transformates);
            coordinator.GetComponent(_reflector2, 0, out Transformates refl2Transformates);
            coordinator.GetComponent(_reflector1, 0, out LightInfo refl1LightInfo);
            coordinator.GetComponent(_reflector2, 0, out LightInfo refl2LightInfo);

            if (!RotateObject(
                ref heroTransformates, 
                ref refl1Transformates, ref refl1LightInfo,
                ref refl2Transformates, ref refl2LightInfo,
                time))
            {
                _heroPath.MovingInLines(ref heroTransformates, time);
            }

            MovingForwardReflectors(ref refl1Transformates, ref refl2Transformates, heroTransformates);

            coordinator.SetComponent(_reflector2, 0, refl2Transformates);
            coordinator.SetComponent(_reflector1, 0, refl1Transformates);
            coordinator.SetComponent(_entityHero, 0, heroTransformates);
            coordinator.SetComponent(_reflector2, 0, refl2LightInfo);
            coordinator.SetComponent(_reflector1, 0, refl1LightInfo);
        }

        private bool RotateObject(
            ref Transformates heroTransformates, 
            ref Transformates refl1Transformates, ref LightInfo refl1LightInfo,
            ref Transformates refl2Transformates, ref LightInfo refl2LightInfo,
            float time
            )
        {
            if (_heroRotator.IsRotating)
            {
                _heroRotator.Rotate(ref heroTransformates, time);
                _reflector1Rotator.Rotate(ref refl1Transformates, ref refl1LightInfo.Direction, time);
                _reflector2Rotator.Rotate(ref refl2Transformates, ref refl2LightInfo.Direction, time);
                return true;
            }

            if (Rotator.TryToRotate(_heroPath.SourceToDirection, heroTransformates.SlopeVector))
            {
                _heroRotator.Restart(heroTransformates.RotateMatrix, heroTransformates.SlopeVector, _heroPath.SourceToDirection);
                _reflector1Rotator.Restart(refl1Transformates.RotateMatrix, refl1Transformates.SlopeVector, refl1LightInfo.Direction, _heroPath.SourceToDirection);
                _reflector2Rotator.Restart(refl2Transformates.RotateMatrix, refl2Transformates.SlopeVector, refl2LightInfo.Direction, _heroPath.SourceToDirection);
                return true;
            }

            return false;
        }
        #endregion


        #region Reflectors
        public void AddReflector(Coordinator coordinator, LightSubSys lightSubSystem)
        {
            coordinator.GetComponents(_entityHero, out List<Transformates> transformatesList);
            Transformates transformatesHero = transformatesList[0];

            AddLight1(coordinator, lightSubSystem, transformatesHero);
            AddLight2(coordinator, lightSubSystem, transformatesHero);
        }

        private void AddLight1(Coordinator coordinator, LightSubSys lightSubSystem, Transformates transformatesHero)
        {
            LightInfo lightInfo = new LightInfo(-Vector3.UnitY, 12.5f, 17.5f, 1, 0.09f, 0.032f, new Vector3(0.2f), new Vector3(1.0f), new Vector3(1.0f));
            Transformates transformates1 = new Transformates(Vector3.Zero, transformatesHero.SlopeVector, Vector3.UnitY, 1);

            PlaceReflector(ref transformates1, transformatesHero);
            _reflector1 = lightSubSystem.AddSpotLight(coordinator, lightInfo, transformates1);
        }

        private void AddLight2(Coordinator coordinator, LightSubSys lightSubSystem, Transformates transformatesHero)
        {
            LightInfo lightInfo = new LightInfo(-Vector3.UnitY, 12.5f, 17.5f, 1, 0.09f, 0.032f, new Vector3(0.2f), new Vector3(1.0f), new Vector3(1.0f));
            Transformates transformates2 = new Transformates(Vector3.Zero, transformatesHero.SlopeVector, Vector3.UnitY, 1);

            PlaceReflector(ref transformates2, transformatesHero, true);
            _reflector2 = lightSubSystem.AddSpotLight(coordinator, lightInfo, transformates2);
        }

        private void PlaceReflector(ref Transformates transformates1, Transformates transformatesHero, bool isLeft = false)
        {
            Vector3 right = Vector3.Normalize(Vector3.Cross(transformatesHero.SlopeVector, Vector3.UnitY));
            if (isLeft) right *= -1;

            transformates1 = new Transformates(transformatesHero.Position - transformatesHero.SlopeVector * 0.28f + right, transformates1.SlopeVector, transformates1.RotateMatrix, Vector3.UnitY, 0.05f);
        }

        private void MovingForwardReflectors(ref Transformates transformates1, ref Transformates transformates2, Transformates transformatesHero)
        {
            PlaceReflector(ref transformates1, transformatesHero);
            PlaceReflector(ref transformates2, transformatesHero, true);
        }

        public void ControlRelfectors(Coordinator coordinator, KeyboardState input, float time)
        {
            float speedOfMoving = 1.5f;

            coordinator.GetComponent(_entityHero, 0, out Transformates heroTransformates);
            coordinator.GetComponent(_reflector1, 0, out LightInfo refl1LightInfo);
            coordinator.GetComponent(_reflector2, 0, out LightInfo refl2LightInfo);

            if (input.IsKeyDown(Keys.KeyPad8))
            {
                refl1LightInfo.Direction += heroTransformates.SlopeVector * speedOfMoving * time;
                refl2LightInfo.Direction += heroTransformates.SlopeVector * speedOfMoving * time;
            }

            if (input.IsKeyDown(Keys.KeyPad2))
            {
                refl1LightInfo.Direction -= heroTransformates.SlopeVector * speedOfMoving * time;
                refl2LightInfo.Direction -= heroTransformates.SlopeVector * speedOfMoving * time;
            }

            Vector3 right = Vector3.Normalize(Vector3.Cross(heroTransformates.SlopeVector, Vector3.UnitY));

            if (input.IsKeyDown(Keys.KeyPad4))
            {
                refl1LightInfo.Direction -= right * speedOfMoving * time;
                refl2LightInfo.Direction -= right * speedOfMoving * time;
            }

            if (input.IsKeyDown(Keys.KeyPad6))
            {
                refl1LightInfo.Direction += right * speedOfMoving * time;
                refl2LightInfo.Direction += right * speedOfMoving * time;
            }

            coordinator.SetComponent(_reflector2, 0, refl2LightInfo);
            coordinator.SetComponent(_reflector1, 0, refl1LightInfo);
        }
        #endregion


        public void MakeNoise(Coordinator coordinator, KeyboardState input)
        {

            if (input.IsKeyPressed(Keys.B))
            {
                _CreazyMoving = !_CreazyMoving;
                coordinator.GetComponent(_entityHero, 0, out Transformates heroTransformates);
                heroTransformates.NoiseMatrix = Matrix4.Identity;
                coordinator.SetComponent(_entityHero, 0, heroTransformates);
            }

            if (_CreazyMoving)
            {
                coordinator.GetComponent(_entityHero, 0, out Transformates heroTransformates);

                Random rnd = new Random();
                int axisOfNoise = rnd.Next(0, 3);
                float angleOfNoise = (float)rnd.NextDouble() * 0.05f;
                switch(axisOfNoise)
                {
                    case 0:
                        heroTransformates.NoiseMatrix  = Matrix4.CreateRotationX(angleOfNoise);
                        break;
                    case 1:
                        heroTransformates.NoiseMatrix  = Matrix4.CreateRotationY(angleOfNoise); 
                        break;
                    case 2:
                        heroTransformates.NoiseMatrix  = Matrix4.CreateRotationZ(angleOfNoise); 
                        break;
                }

                coordinator.SetComponent(_entityHero, 0, heroTransformates);
            }

        }
    }
}
