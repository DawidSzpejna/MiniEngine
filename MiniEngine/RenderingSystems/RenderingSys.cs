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
using MiniEngine.CameraSystem;
using MiniEngine.DeferredShading;
using MiniEngine.Lights;

namespace MiniEngine.RenderingSystems
{
    internal class RenderingSys : ECSystem, IDisposable
    {
        private bool _disposedValue;

        private Shader _geometryShader;

        private Shader _lightShader;

        private GBuffer _gBuffer;

        private QuadSubSys _quadSubSys;

        public LightSubSys rLightSubSys { get; private set; }

        public Shader _lightObjectShader;

        public RenderingSys()
        {
            _geometryShader = new Shader(
                @"DeferredShading/Shader/VGeometryPassShr.glsl",
                @"DeferredShading/Shader/FGeometryPassShr.glsl"
            );

            _lightShader = new Shader(
                @"RenderingSystems/ShaderDeffered/VLightPassShr.glsl",
                @"RenderingSystems/ShaderDeffered/FLightPassShr.glsl"
            );

            _lightObjectShader = new Shader(
                @"RenderingSystems/ObjectLight/VLightObjectShr.glsl",
                @"RenderingSystems/ObjectLight/FLightObjectShr.glsl"
            );
        }

        public override void Init(Coordinator coordinator)
        {
            Signature signature = new Signature();
            signature.Set(coordinator.GetComponentType<MeshInfo>(), true);
            signature.Set(coordinator.GetComponentType<VerticesData>(), true);
            signature.Set(coordinator.GetComponentType<ObjectsGL>(), true);
            signature.Set(coordinator.GetComponentType<Transformates>(), true);
            coordinator.SetSystemSignature<RenderingSys>(signature);

            _quadSubSys = coordinator.RegisterSystem<QuadSubSys>();
            _quadSubSys.AddEntity(coordinator);

            rLightSubSys = coordinator.RegisterSystem<LightSubSys>();
            rLightSubSys.AddDirLight(
                new LightInfo(new Vector3(-2, -3, -2), new Vector3(0.2f), new Vector3(1.0f), new Vector3(1.0f))
            );

            LightInfo lightInfo = new LightInfo(1, 0.09f, 0.032f, new Vector3(0.2f), new Vector3(1.0f), new Vector3(1.0f));

            Transformates transformates = new Transformates(new Vector3(4, 2, -5), Vector3.Zero, Vector3.Zero, 0.1f);
            rLightSubSys.AddPointLight(coordinator, lightInfo, transformates);

            transformates = new Transformates(new Vector3(0, 0.2f, 0.3f), Vector3.Zero, Vector3.Zero, 0.1f);
            //rLightSubSys.AddPointLight(coordinator, lightInfo, transformates);
        }

        public void AddGBuffer(int width, int height)
            => _gBuffer = new GBuffer(width, height);


        #region Draw model's meshes
        public void DrawMeshes(Coordinator coordinator, CameraManager camera, Shader shader)
        {
            shader.SetMatrix4("projection", camera.GetProjectionMatrix(coordinator));
            shader.SetMatrix4("view", camera.GetViewMatrix(coordinator));

            foreach (var entity in Entities)
            {
                coordinator.GetComponents(entity, out List<MeshInfo> meshInfoList);
                coordinator.GetComponents(entity, out List<VerticesData> verticesDataList);
                coordinator.GetComponents(entity, out List<ObjectsGL> objectsGLList);
                coordinator.GetComponents(entity, out List<Transformates> transformatesList);

                for (int i = 0; i < objectsGLList.Count; i++)
                {
                    DrawMesh(shader, meshInfoList[i], transformatesList[0], objectsGLList[i], verticesDataList[i]);
                }
            }

        }

        private void DrawMesh(
            Shader shader, MeshInfo meshInfo, Transformates transformates, 
            ObjectsGL objectsGL, VerticesData verticesData)
        {

            InitMesh(shader, meshInfo, transformates);
            Draw(objectsGL, verticesData);
        }

        private void InitMesh(Shader shader,
            MeshInfo meshInfo, Transformates transformates)
        {
            Matrix4 mat = Matrix4.CreateTranslation(transformates.Position);
            mat = Matrix4.Mult(Matrix4.CreateScale(transformates.Scale), mat);
            mat = Matrix4.Mult(transformates.RotateMatrix, mat);
            mat = Matrix4.Mult(transformates.NoiseMatrix, mat);

            mat = Matrix4.Mult(meshInfo.Model, mat);

            shader.SetMatrix4("model", mat);
            shader.SetMatrix4("modelRT", Matrix4.Transpose(Matrix4.Invert(mat)));

            shader.SetInt("texture_diffuse1", 3);
            meshInfo.MeshTexture.UseTexture(TextureUnit.Texture3);
        }

        private void Draw(ObjectsGL objectsGL, VerticesData verticesData)
        {
            GL.BindVertexArray(objectsGL.VAO);
            GL.DrawElements(BeginMode.Triangles, verticesData.Indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
        #endregion


        #region Rendering
        public void RenderWorld(Coordinator coordinator, CameraManager camera)
        {
            DeferredPart(coordinator, camera);

            CopyDepthFromGBuffer();

            ForwardPart(coordinator, camera);
        }

        private void DeferredPart(Coordinator coordinator, CameraManager camera)
        {
            GeometryPass(coordinator, camera);
            ClearCreen();
            LightingPass(coordinator, camera);
        }

        private void GeometryPass(Coordinator coordinator, CameraManager camera)
        {
            _gBuffer.UseGBuffer();

            ClearCreen();
            InitGeometryPass(coordinator, camera);
            DrawMeshes(coordinator, camera, _geometryShader);

            _gBuffer.UnUseGBuffer();
        }

        private void ClearCreen() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        private void InitGeometryPass(Coordinator coordinator, CameraManager camera)
        {
            _geometryShader.UseShader();
            _geometryShader.SetMatrix4("projection", camera.GetProjectionMatrix(coordinator));
            _geometryShader.SetMatrix4("view", camera.GetViewMatrix(coordinator));
        }

        private void LightingPass(Coordinator coordinator, CameraManager camera)
        {
            InitLightPass(coordinator, camera);
            rLightSubSys.UseDirLight(_lightShader);
            rLightSubSys.UsePointLight(coordinator, _lightShader);
            rLightSubSys.UseSpotLight(coordinator, _lightShader);

            _gBuffer.UseGTextures();
            _quadSubSys.RenderQuad(coordinator);
        }

        private void InitLightPass(Coordinator coordinator, CameraManager camera)
        {
            coordinator.GetComponents(camera.CurrentEntity, out List<CameraInfo> cameraInfoList);

            _lightShader.UseShader();
            _lightShader.SetInt("gPosition", 0);
            _lightShader.SetInt("gNormal", 1);
            _lightShader.SetInt("gAlbedoSpec", 2);

            _lightShader.SetVector3("viewPos", cameraInfoList[0].Position);
        }

        private void CopyDepthFromGBuffer()
            => _gBuffer.CopyDepthFromGBuffer();

        private void ForwardPart(Coordinator coordinator, CameraManager camera)
        {
            _lightObjectShader.UseShader();
            rLightSubSys.RenderLights(coordinator, camera, _lightObjectShader);
        }

        #endregion


        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RenderingSystem()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
