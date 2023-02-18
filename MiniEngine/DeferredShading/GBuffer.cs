using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace MiniEngine.DeferredShading
{
    internal class GBuffer : IDisposable
    {
        private bool _disposedValue;

        public readonly int Width;
        public readonly int Height;

        private int _gBuffer;
        private int _gPosition;
        private int _gNormal;
        private int _gAlbedoSpec;
        private int _rboDepth;

        public GBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            SetupGBuffer(width, height);
        }

        private void SetupGBuffer(int width, int height)
        {
            AddGFrameBuffer();
            InitGFrameBuffer(width, height);
        }

        private void AddGFrameBuffer()
        {
            GL.GenFramebuffers(1, out _gBuffer);
            UseGBuffer();
        }

        public void UseGBuffer() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, _gBuffer);

        public void UnUseGBuffer() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        private void InitGFrameBuffer(int width, int height)
        {
            AddPositionTexture(width, height);
            AddNormalsTexture(width, height);
            AddAlbedoSpecTexture(width, height);
            AttachmentsForRendering();
            AddDepthTexture(width, height);

            if (!CheckIfComplete())
                Console.WriteLine("Framebuffer not complete!");

            UnUseGBuffer();
        }

        private void AddPositionTexture(int width, int height)
        {
            GL.GenTextures(1, out _gPosition);
            GL.BindTexture(TextureTarget.Texture2D, _gPosition);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _gPosition, 0);
        }

        private void AddNormalsTexture(int width, int height)
        {
            GL.GenTextures(1, out _gNormal);
            GL.BindTexture(TextureTarget.Texture2D, _gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, _gNormal, 0);
        }

        private void AddAlbedoSpecTexture(int width, int height)
        {
            GL.GenTextures(1, out _gAlbedoSpec);
            GL.BindTexture(TextureTarget.Texture2D, _gAlbedoSpec);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, _gAlbedoSpec, 0);
        }

        private void AttachmentsForRendering()
        {
            DrawBuffersEnum[] attachments = new DrawBuffersEnum[] {
                    DrawBuffersEnum.ColorAttachment0,
                    DrawBuffersEnum.ColorAttachment1,
                    DrawBuffersEnum.ColorAttachment2
                };
            GL.DrawBuffers(3, attachments);
        }

        private void AddDepthTexture(int width, int height)
        {
            GL.GenRenderbuffers(1, out _rboDepth);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rboDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _rboDepth);
        }

        private bool CheckIfComplete() => GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete;

        public void UseGTextures()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _gPosition);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _gNormal);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, _gAlbedoSpec);
        }

        public void CopyDepthFromGBuffer()
        {
            UseReadGBuffer();
            UnUseDrawGBuffer();
            BlitGBufferForDepth();
            UnUseGBuffer();
        }

        public void UseReadGBuffer() 
            => GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _gBuffer);

        public void UnUseDrawGBuffer()
            => GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

        public void BlitGBufferForDepth() 
            => GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
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
        // ~GBuffer()
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
