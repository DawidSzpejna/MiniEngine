using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using MiniEngine.EntityComponetSystem;
using MiniEngine.BasicComponents;
using MiniEngine.RenderingSystems;
using MiniEngine.CameraSystem;
using MiniEngine.Hero;

namespace MiniEngine
{
    public partial class Engine : GameWindow
    {
        Coordinator coordinator;
        RenderingSys renderingbgSys;
        CameraManager cameraSys;
        FlukCokSys flukCokSys;

        protected Color4 _clearColor = new Color4(0.2f, 0.3f, 0.3f, 1f);
        public Engine(int widht, int height, string title = "MyWindow") :
            base(
                GameWindowSettings.Default, 
                new NativeWindowSettings() 
                {
                    Title = title,
                    Size = new Vector2i(widht, height),
                    WindowBorder = WindowBorder.Fixed,
                    StartVisible = false,
                    API = ContextAPI.OpenGL,
                    Profile = ContextProfile.Core,
                    APIVersion = new Version(3, 3)
                }
                ) 
        {
            coordinator = new Coordinator();
        }

        #region Load/UnLoad
        protected override void OnLoad()
        {
            // basic settings
            // --------------
            {
                base.OnLoad();
                this.IsVisible = true;
                GL.ClearColor(_clearColor);
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
            }

            // code...
            // components
            // ----------
            coordinator.RegisterSimpleComponent<    MeshInfo>();
            coordinator.RegisterSimpleComponent<    VerticesData>();
            coordinator.RegisterDisposableComponent<ObjectsGL>();
            coordinator.RegisterSimpleComponent<    Transformates>();
            coordinator.RegisterSimpleComponent<    CameraInfo>();
            coordinator.RegisterSimpleComponent<    LightInfo>();

            // system rendering
            // ----------------
            renderingbgSys = coordinator.RegisterSystem<RenderingSys>();
            renderingbgSys.AddGBuffer(Size.X, Size.Y);

            // system camera
            // -------------
            cameraSys = coordinator.RegisterSystem<CameraManager>();

            // signature for system
            // --------------------
            Transformates transformates = new Transformates(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY, 0.6f);
            List<Loader.Mesh> meshes = Loader.ColladaLoad.Load("floating_island.dae", "Sources");
            AddModel(transformates, meshes);

            transformates = new Transformates(new Vector3(-8, 1, -4), -Vector3.UnitZ, Vector3.UnitY, 0.6f);
            meshes = Loader.ColladaLoad.Load("floating_island.dae", "Sources");
            AddModel(transformates, meshes);

            transformates = new Transformates(new Vector3(0, -3, 0), -Vector3.UnitZ, Vector3.UnitY, 7f);
            meshes = Loader.ColladaLoad.Load("bigPlane.dae", "Sources/BigPlane");
            AddModel(transformates, meshes);

            // hero
            // ----
            flukCokSys = coordinator.RegisterSystem<FlukCokSys>();
            Transformates tranHero = new Transformates(new Vector3(4, 0, -5), -Vector3.UnitZ, Vector3.UnitY, 0.4f);
            flukCokSys.AddEntity(coordinator, tranHero, "myflyfly.dae", "Hero/Sources");
            flukCokSys.AddReflector(coordinator, renderingbgSys.rLightSubSys);
           
            List<Vector3> path = new List<Vector3>() { new Vector3(4, 0, -5), new Vector3(-4, 0, -5), new Vector3(-4, 0, 5), new Vector3(4, 0, 5) };
            flukCokSys.AddPath(coordinator, path);


            CameraInfo cameraInfo = new CameraInfo(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
            cameraSys.AddFreeCamera(coordinator, cameraInfo);

            cameraInfo = new CameraInfo(Vector3.UnitZ * 5f + Vector3.UnitY * 6f, Size.X / (float)Size.Y);
            cameraSys.AddStaticCamera(coordinator, cameraInfo);

            cameraInfo = new CameraInfo(Vector3.UnitZ * 0.2f + Vector3.UnitY * 6f, Size.X / (float)Size.Y);
            cameraSys.AddFollowingCamera(coordinator, cameraInfo, flukCokSys.EntityHero);

            cameraInfo = new CameraInfo(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
            cameraSys.AddTPPCamera(coordinator, cameraInfo, flukCokSys.EntityHero);
        }

        internal void AddModel(Transformates transformates, List<Loader.Mesh> meshes)
        {
            Entity entity = coordinator.CreateEntity();

            foreach (var mesh in meshes)
            {
                AddObject(entity, mesh);
            }

            coordinator.AddComponent(entity, transformates);
        }

        private void AddObject(Entity entity, Loader.Mesh mesh)
        {
            MeshInfo meshInfo = new MeshInfo(mesh.Model, mesh.MeshTextures);
            coordinator.AddComponent(entity, meshInfo);

            VerticesData verticesData = new VerticesData(mesh.Vertices, mesh.Indices);
            coordinator.AddComponent(entity, verticesData);

            ObjectsGL objectsGL = new ObjectsGL(mesh.Vertices, mesh.Indices);
            coordinator.AddComponent(entity, objectsGL);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            // unbinding GL-structures
            // -----------------------
            UnBindShader();
            UnBindObjectsGL();
            UnBindTextures();
            UnBindFrameBuffer();
            UnBindRenderBuffer();

            // disposing C# objects
            // --------------------
            // code .....
        }

        private void UnBindShader() => GL.UseProgram(0);
        private void UnBindObjectsGL()
        {
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        private void UnBindTextures()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        private void UnBindFrameBuffer() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        private void UnBindRenderBuffer() => GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        #endregion

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderingbgSys.RenderWorld(coordinator, cameraSys);

            this.Context.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var input = KeyboardState;
            cameraSys.CameraAction(coordinator, input, (float)args.Time);

            flukCokSys.ControlRelfectors(coordinator, input, (float)args.Time);
            flukCokSys.Moving(coordinator, (float)args.Time);
            flukCokSys.MakeNoise(coordinator, input);

            renderingbgSys.rLightSubSys.CheckIfDayNight(input);
            renderingbgSys.rLightSubSys.MakeDayNight((float)args.Time);
            cameraSys.ChangeCamera(input);
        }

        #region Something XD
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            cameraSys.UpdateFov(coordinator, e.OffsetY);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // resing the viewport
            // -------------------
            GL.Viewport(0, 0, e.Width, e.Height);
            cameraSys.UpdateAspectRatio(coordinator, Size.X / (float)Size.Y);
        }
        #endregion
    }
}
