using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MiniEngine.Graphics
{
    public class Shader : IDisposable
    {
        private int ProgramHandle { get; set; }

        private bool _disposedValue;

        private readonly Dictionary<string, int> _uniformLocation;


        #region Constructors
        public Shader(string vertexShaderPath, string fragmentShaderPath)
        {
            string vertexShaderSource = File.ReadAllText(vertexShaderPath);
            string fragmentShaderSource = File.ReadAllText(fragmentShaderPath);

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);

            // Compiling - vertexShader
            {
                GL.CompileShader(vertexShader);
                GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
                if (success == 0)
                {
                    string infoLog = GL.GetShaderInfoLog(vertexShader);
                    Console.WriteLine(infoLog);
                }
            }

            // Compiling - fragmentShader
            {
                GL.CompileShader(fragmentShader);
                GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int success);
                if (success == 0)
                {
                    string infoLog = GL.GetShaderInfoLog(fragmentShader);
                    Console.WriteLine(infoLog);
                }
            }

            // Creating the final program
            {
                this.ProgramHandle = GL.CreateProgram();
                GL.AttachShader(ProgramHandle, vertexShader);
                GL.AttachShader(ProgramHandle, fragmentShader);
                GL.LinkProgram(ProgramHandle);

                GL.GetProgram(ProgramHandle, GetProgramParameterName.LinkStatus, out int success);
                if (success == 0)
                {
                    string infoLog = GL.GetProgramInfoLog(ProgramHandle);
                    Console.WriteLine(infoLog);
                }
            }

            // Detaching and deleting
            GL.DetachShader(ProgramHandle, vertexShader);
            GL.DetachShader(ProgramHandle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Gathering info about Uniforms in shader
            GL.GetProgram(ProgramHandle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);
            _uniformLocation = new Dictionary<string, int>();

            for (int i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(ProgramHandle, i, out _, out _);
                var location = GL.GetUniformLocation(ProgramHandle, key);

                _uniformLocation.Add(key, location);
            }
        }
        #endregion


        #region Basic Features
        public void UseShader()
        {
            GL.UseProgram(ProgramHandle);
        }
        #endregion


        #region Uniform Setters
        public void SetMatrix4(string name, Matrix4 mtx)
        {
            if (!_uniformLocation.ContainsKey(name))
            {
                Console.WriteLine("SetMatrix4: {0}", name);
                return;
            }

            GL.UseProgram(ProgramHandle);
            GL.UniformMatrix4(_uniformLocation[name], false, ref mtx);
        }


        public void SetVector3(string name, Vector3 vec)
        {
            if (!_uniformLocation.ContainsKey(name))
            {
                Console.WriteLine("SetVector3: {0}", name);
                return;
            }

            GL.UseProgram(ProgramHandle);
            GL.Uniform3(_uniformLocation[name], vec);
        }

        public void SetVector4(string name, Vector4 vec)
        {
            if (!_uniformLocation.ContainsKey(name))
            {
                Console.WriteLine("SetVector4: {0}", name);
                return;
            }

            GL.UseProgram(ProgramHandle);
            GL.Uniform4(_uniformLocation[name], vec);
        }


        public void SetColor4(string name, Color4 col)
        {
            if (!_uniformLocation.ContainsKey(name))
            {
                Console.WriteLine("SetVector4: {0}", name);
                return;
            }

            GL.UseProgram(ProgramHandle);
            GL.Uniform4(_uniformLocation[name], col);
        }


        public void SetFloat(string name, float val)
        {
            if (!_uniformLocation.ContainsKey(name))
            {
                Console.WriteLine("SetFloat: {0}", name);
                return;
            }

            GL.UseProgram(ProgramHandle);
            GL.Uniform1(_uniformLocation[name], val);
        }


        public void SetInt(string name, int val)
        {
            if (!_uniformLocation.ContainsKey(name))
            {
                Console.WriteLine("SetInt: {0}", name);
                return;
            };

            GL.UseProgram(ProgramHandle);
            GL.Uniform1(_uniformLocation[name], val);
        }

        public void SetBool(string name, bool val)
        {
            if (!_uniformLocation.ContainsKey(name))
            {
                Console.WriteLine("SetBool: {0}", name);
                return;
            };

            GL.UseProgram(ProgramHandle);
            GL.Uniform1(_uniformLocation[name], val ? 1 : 0);
        }
        #endregion


        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing) { }

                GL.DeleteProgram(ProgramHandle);

                ProgramHandle = 0;
                _disposedValue = true;
            }
        }


        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources - what???
        ~Shader()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
