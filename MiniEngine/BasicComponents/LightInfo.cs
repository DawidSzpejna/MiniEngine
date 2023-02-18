using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace MiniEngine.BasicComponents
{
    internal class LightInfo
    {
        private Vector3 _direction;

        public ref Vector3 Direction { get => ref _direction; }

        public Vector3 vector3 { set => _direction = value; }

        private float _cutOff;

        private float _outerCutOff;

        public float CutOff
        {
            get { return _cutOff; }
            set
            {
                _cutOff = value;
                CosCutOff = MathF.Cos(MathHelper.DegreesToRadians(_cutOff));
            }
        }

        public float OuterCutOff
        {
            get { return _outerCutOff; }
            set
            {
                _outerCutOff = value;
                CosOuterCutOff = MathF.Cos(MathHelper.DegreesToRadians(_outerCutOff));
            }
        }

        public float CosCutOff { get; private set; }

        public float CosOuterCutOff { get; private set; }

        public float Constant { get; set; }

        public float Linear { get; set; }

        public float Quadratic { get; set; }

        public Vector3 Ambient { get; set; }

        public Vector3 Diffuse { get; set; }

        public Vector3 Specular { get; set; }


        #region Constructors
        public LightInfo(
            Vector3 direction, Vector3 ambient, Vector3 diffuse, Vector3 specular
            )
        {
            Direction = direction;
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
        }

        public LightInfo(
            float constant, float linear, float quadratic, 
            Vector3 ambient, Vector3 diffuse, Vector3 specular
            )
        {
            Constant = constant;
            Linear = linear;
            Quadratic = quadratic;
            Ambient= ambient;
            Diffuse = diffuse;
            Specular = specular;
        }

        public LightInfo(
            Vector3 dir, float cutOff, float outerCutOff, float constant,
            float linear, float quadratic, Vector3 ambient, Vector3 diffuse, Vector3 specular
            )
        {
            Direction = dir;
            CutOff = cutOff;
            OuterCutOff = outerCutOff;
            Constant = constant;
            Linear = linear;
            Quadratic = quadratic;
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
        }
        #endregion
    }
}
