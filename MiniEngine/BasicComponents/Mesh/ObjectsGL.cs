using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace MiniEngine.BasicComponents
{
    internal class ObjectsGL : IDisposable
    {
        public int VAO;

        private int _VBO;

        private int _EBO;

        private bool _disposedValue;


        #region Constructor
        public ObjectsGL(float[] verticesData, int[] indices)
        {
            GenerateObjects();

            BindGeneralObject();
            {
                BindAndFillVBO(verticesData);
                BindAndFillEBO(indices);

                int stride = 3 * sizeof(float) + 3 * sizeof(float) + 2 * sizeof(float);
                AddAttributeForPosition(stride);
                AddAttributeForNormal(stride);
                AddAttributeForTexCoords(stride);
            }
            UnBindGeneralObject();
        }

        private void GenerateObjects()
        {
            VAO = GL.GenVertexArray();
            _VBO = GL.GenBuffer();
            _EBO = GL.GenBuffer();
        }

        private void BindGeneralObject() => GL.BindVertexArray(VAO);

        private void UnBindGeneralObject() => GL.BindVertexArray(0);

        private void BindAndFillVBO(float[] verticesData)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, verticesData.Length * sizeof(float), verticesData, BufferUsageHint.StaticDraw);
        }

        private void BindAndFillEBO(int[] indices)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.StaticDraw);
        }

        private void AddAttributeForPosition(int stride)
        {
            // vertex position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
        }

        private void AddAttributeForNormal(int stride)
        {
            // normal position
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        private void AddAttributeForTexCoords(int stride)
        {

            // texture coordinate
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float) + 3 * sizeof(float));
            GL.EnableVertexAttribArray(2);
        }
        #endregion


        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing) { /* dispose managed state (managed objects) */ }

                /* free unmanaged resources (unmanaged objects) and override finalizer */
                GL.DeleteBuffer(_VBO);
                GL.DeleteBuffer(_EBO);
                GL.DeleteVertexArray(VAO);
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ObjectsGL()
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
