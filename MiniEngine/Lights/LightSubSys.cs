using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.EntityComponetSystem;
using MiniEngine.BasicComponents;
using MiniEngine.Graphics;
using MiniEngine.CameraSystem;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiniEngine.Lights
{
    internal partial class LightSubSys : ECSystem
    {
        private List<Entity> _pointLights;
        private List<Entity> _spotLights;


        private VerticesData _verticesData;
        private ObjectsGL _objectsGL;

        public LightSubSys()
        {
            _pointLights = new List<Entity>();
            _spotLights = new List<Entity>();

            // .....
            float[] vertices = new float[] {
                 1,  1,  1, -0,  1, -0, 0, 0,
                -1,  1, -1, -0,  1, -0, 0, 0,
                -1,  1,  1, -0,  1, -0, 0, 0,
                -1,  1, -1, -0, -0, -1, 0, 0,
                 1, -1, -1, -0, -0, -1, 0, 0,
                -1, -1, -1, -0, -0, -1, 0, 0,
                 1,  1, -1,  1, -0, -0, 0, 0,
                 1, -1,  1,  1, -0, -0, 0, 0,
                 1, -1, -1,  1, -0, -0, 0, 0,
                -1, -1,  1, -0, -1, -0, 0, 0,
                 1, -1, -1, -0, -1, -0, 0, 0,
                 1, -1,  1, -0, -1, -0, 0, 0,
                -1,  1,  1, -1, -0, -0, 0, 0,
                -1, -1, -1, -1, -0, -0, 0, 0,
                -1, -1,  1, -1, -0, -0, 0, 0,
                 1,  1,  1, -0, -0,  1, 0, 0,
                -1, -1,  1, -0, -0,  1, 0, 0,
                 1, -1,  1, -0, -0,  1, 0, 0,
                 1,  1,  1, -0,  1, -0, 0, 0,
                 1,  1, -1, -0,  1, -0, 0, 0,
                -1,  1, -1, -0,  1, -0, 0, 0,
                -1,  1, -1, -0, -0, -1, 0, 0,
                 1,  1, -1, -0, -0, -1, 0, 0,
                 1, -1, -1, -0, -0, -1, 0, 0,
                 1,  1, -1,  1, -0, -0, 0, 0,
                 1,  1,  1,  1, -0, -0, 0, 0,
                 1, -1,  1,  1, -0, -0, 0, 0,
                -1, -1,  1, -0, -1, -0, 0, 0,
                -1, -1, -1, -0, -1, -0, 0, 0,
                 1, -1, -1, -0, -1, -0, 0, 0,
                -1,  1,  1, -1, -0, -0, 0, 0,
                -1,  1, -1, -1, -0, -0, 0, 0,
                -1, -1, -1, -1, -0, -0, 0, 0,
                 1,  1,  1, -0, -0,  1, 0, 0,
                -1,  1,  1, -0, -0,  1, 0, 0,
                -1, -1,  1, -0, -0,  1, 0, 0
            };
            int[] indices = new int[] {
                 0,  1,  2,  3,  4,  5, 
                 6,  7,  8,  9, 10, 11,
                12, 13, 14, 15, 16, 17, 
                18, 19, 20, 21, 22, 23,
                24, 25, 26, 27, 28, 29,
                30, 31, 32, 33, 34, 35,
            };

            _verticesData = new VerticesData(vertices, indices);
            _objectsGL = new ObjectsGL(vertices, indices);
        }

        public override void Init(Coordinator coordinator)
        {
            Signature signature = new Signature();
            signature.Set(coordinator.GetComponentType<LightInfo>(), true);
            signature.Set(coordinator.GetComponentType<Transformates>(), true);
            coordinator.SetSystemSignature<LightSubSys>(signature);
        }

        #region Directional Lights
        private bool _dayNight;
        private LightInfo _dirLightInfo;
        private static readonly string _dirLightName = "dirLight";

        public void AddDirLight(LightInfo lightInfo)
        {
            _dirLightInfo = lightInfo;
            _dayNight = false;
        }

        public void UseDirLight(Shader shader)
        {
            shader.SetVector3($"{_dirLightName}.direction", _dirLightInfo.Direction);
            shader.SetVector3($"{_dirLightName}.ambient", _dirLightInfo.Ambient);
            shader.SetVector3($"{_dirLightName}.diffuse", _dirLightInfo.Diffuse);
            shader.SetVector3($"{_dirLightName}.specular", _dirLightInfo.Specular);
        }


        float _sunRadius;
        float _sunAngle;
        public void CheckIfDayNight(KeyboardState input)
        {
            if (input.IsKeyPressed(Keys.N))
            {
                _dayNight = !_dayNight;
                _sunRadius = MathF.Sqrt(_dirLightInfo.Direction.X * _dirLightInfo.Direction.X + _dirLightInfo.Direction.Y * _dirLightInfo.Direction.Y);
                _sunAngle = MathF.Atan2(-_dirLightInfo.Direction.Y, -_dirLightInfo.Direction.X);
                _sunAngle = MathHelper.RadiansToDegrees(_sunAngle);
            }
        }

        public void MakeDayNight(float time)
        {
            if (_dayNight)
            {
                float x = _sunRadius * (float)Math.Cos(_sunAngle / 180f * Math.PI);
                float y = _sunRadius * (float)Math.Sin(_sunAngle / 180f * Math.PI);

                Vector3 newDirection = new Vector3(-x, -y, _dirLightInfo.Direction.Z);
                _dirLightInfo.Direction = newDirection;

                _sunAngle += 9 * time;
                if (_sunAngle >= 360)
                {
                    _sunAngle = 0;
                }
            }
        }
        #endregion

        #region Point lights
        private static readonly string _pointLightName = "pointLights";

        public void AddPointLight(Coordinator coordinator, LightInfo lightInfo, Transformates transformates)
        {
            Entity entity = coordinator.CreateEntity();
            coordinator.AddComponent(entity, lightInfo);
            coordinator.AddComponent(entity, transformates);
            _pointLights.Add(entity);
        }

        public void UsePointLight(Coordinator coordinator, Shader shader)
        {
            for (int i = 0; i < _pointLights.Count; i++)
            {
                coordinator.GetComponents(_pointLights[i], out List<LightInfo> lightInfosList);
                coordinator.GetComponents(_pointLights[i], out List<Transformates> transformatesList);

                shader.SetVector3($"{_pointLightName}[{i}].position", transformatesList[0].Position);

                shader.SetFloat($"{_pointLightName}[{i}].constant", lightInfosList[0].Constant);
                shader.SetFloat($"{_pointLightName}[{i}].linear", lightInfosList[0].Linear);
                shader.SetFloat($"{_pointLightName}[{i}].quadratic", lightInfosList[0].Quadratic);

                shader.SetVector3($"{_pointLightName}[{i}].ambient", lightInfosList[0].Ambient);
                shader.SetVector3($"{_pointLightName}[{i}].diffuse", lightInfosList[0].Diffuse);
                shader.SetVector3($"{_pointLightName}[{i}].specular", lightInfosList[0].Specular);
            }

            shader.SetInt("nrPointLights", _pointLights.Count);
        }
        #endregion

        #region Spot lights
        private static readonly string _spotLightName = "spotLights";

        public Entity AddSpotLight(Coordinator coordinator, LightInfo lightInfo, Transformates transformates)
        {
            Entity entity = coordinator.CreateEntity();
            coordinator.AddComponent(entity, lightInfo);
            coordinator.AddComponent(entity, transformates);
            _spotLights.Add(entity);

            return entity;
        }

        public void UseSpotLight(Coordinator coordinator, Shader shader)
        {
            for (int i = 0; i < _spotLights.Count; i++)
            {
                coordinator.GetComponents(_spotLights[i], out List<LightInfo> lightInfosList);
                coordinator.GetComponents(_spotLights[i], out List<Transformates> transformatesList);

                shader.SetVector3($"{_spotLightName}[{i}].position", transformatesList[0].Position);
                shader.SetVector3($"{_spotLightName}[{i}].direction", lightInfosList[0].Direction);

                shader.SetFloat($"{_spotLightName}[{i}].cutOff", lightInfosList[0].CosCutOff);
                shader.SetFloat($"{_spotLightName}[{i}].outerCutOff", lightInfosList[0].CosOuterCutOff);

                shader.SetFloat($"{_spotLightName}[{i}].constant", lightInfosList[0].Constant);
                shader.SetFloat($"{_spotLightName}[{i}].linear", lightInfosList[0].Linear);
                shader.SetFloat($"{_spotLightName}[{i}].quadratic", lightInfosList[0].Quadratic);

                shader.SetVector3($"{_spotLightName}[{i}].ambient", lightInfosList[0].Ambient);
                shader.SetVector3($"{_spotLightName}[{i}].diffuse", lightInfosList[0].Diffuse);
                shader.SetVector3($"{_spotLightName}[{i}].specular", lightInfosList[0].Specular);
            }

            shader.SetInt("nrSpotlights", _spotLights.Count);
        }
        #endregion


        public void RenderLights(Coordinator coordinator, CameraManager camera, Shader shader)
        {
            shader.SetMatrix4("projection", camera.GetProjectionMatrix(coordinator));
            shader.SetMatrix4("view", camera.GetViewMatrix(coordinator));

            foreach (var entity in Entities)
            {
                coordinator.GetComponents(entity, out List<Transformates> transformatesList);

                InitLight(coordinator, shader, entity, transformatesList[0]);
                DrawMesh();
            }
        }

        private void InitLight(Coordinator coordinator, Shader shader, Entity entity, Transformates transformates)
        {
            coordinator.GetComponents(entity, out List<LightInfo> lightInfosList);

            Matrix4 mat = Matrix4.CreateTranslation(transformates.Position);
            mat = Matrix4.Mult(Matrix4.CreateScale(transformates.Scale), mat);
            mat = Matrix4.Mult(transformates.RotateMatrix, mat);
            mat = Matrix4.Mult(transformates.NoiseMatrix, mat);

            shader.SetMatrix4("model", mat);
            shader.SetVector3("lightColor", lightInfosList[0].Diffuse);
        }

        private void DrawMesh()
        {
            GL.BindVertexArray(_objectsGL.VAO);
            GL.DrawElements(BeginMode.Triangles, _verticesData.Indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}
