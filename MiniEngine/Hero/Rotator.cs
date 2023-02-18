using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.BasicComponents;
using OpenTK.Mathematics;

namespace MiniEngine.Hero
{
    internal class Rotator
    {
        private float _factorRotation = 0;
        private Quaternion _startOfRotation;
        private Quaternion _endOfRotation;
        private Matrix4 _oldRotateMatrix;
        private Vector3 _oldSlopeVector;
        private Vector3 _odlVectorToRotate;
        private Vector3 _endVector;

        public static readonly float FactorRotationConstant = 0.007812500f;

        public bool IsRotating;

        public void Restart(Matrix4 oldRotateMatrix, Vector3 oldSlopeVector, Vector3 endVector)
        {
            (_startOfRotation, _endOfRotation) = RotationBetweenVectors(oldSlopeVector, endVector);
            _oldRotateMatrix = oldRotateMatrix;
            _oldSlopeVector = oldSlopeVector;
            _endVector = endVector;
            _factorRotation = 0;
            IsRotating = true;
        }

        public void Restart(Matrix4 oldRotateMatrix, Vector3 oldSlopeVector, Vector3 vectorToRotate, Vector3 endVector)
        {
            (_startOfRotation, _endOfRotation) = RotationBetweenVectors(oldSlopeVector, endVector);
            _oldRotateMatrix = oldRotateMatrix;
            _oldSlopeVector = oldSlopeVector;
            _odlVectorToRotate = vectorToRotate;
            _endVector = endVector;
            _factorRotation = 0;
            IsRotating = true;
        }

        public Matrix4 Rotate(ref Transformates transformates, float time)
        {
            _factorRotation += FactorRotationConstant;
            Quaternion interpolation = Quaternion.Slerp(_startOfRotation, _endOfRotation, _factorRotation);
            Matrix4 quaterionMatrix = Matrix4.CreateFromQuaternion(interpolation);
            transformates.RotateMatrix = Matrix4.Mult(quaterionMatrix, _oldRotateMatrix);

            if (_factorRotation >= 1)
            {
                transformates.SlopeVector = _endVector;
                IsRotating = false;
                return quaterionMatrix;
            }

            transformates.SlopeVector = Vector3.Normalize(Vector3.TransformVector(_oldSlopeVector, quaterionMatrix));
            return quaterionMatrix;
        }

        public Matrix4 Rotate(ref Transformates transformates, ref Vector3 vectorToRotate, float time)
        {
            _factorRotation += FactorRotationConstant;
            Quaternion interpolation = Quaternion.Slerp(_startOfRotation, _endOfRotation, _factorRotation);
            Matrix4 quaterionMatrix = Matrix4.CreateFromQuaternion(interpolation);
            transformates.RotateMatrix = Matrix4.Mult(quaterionMatrix, _oldRotateMatrix);

            vectorToRotate = Vector3.Normalize(Vector3.TransformVector(_odlVectorToRotate, quaterionMatrix));

            if (_factorRotation >= 1)
            {
                transformates.SlopeVector = _endVector;
                IsRotating = false;
                return quaterionMatrix;
            }

            transformates.SlopeVector = Vector3.Normalize(Vector3.TransformVector(_oldSlopeVector, quaterionMatrix));
            return quaterionMatrix;
        }

        public static bool TryToRotate(Vector3 slopeVector, Vector3 srcDest) => Vector3.Dot(srcDest, slopeVector) != 1;

        // ---> http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-17-quaternions/
        private (Quaternion, Quaternion) RotationBetweenVectors(Vector3 start, Vector3 dest)
        {
            start = Vector3.Normalize(start);
            dest = Vector3.Normalize(dest);

            float cosTheta = Vector3.Dot(start, dest);
            Vector3 rotationAxis;

            if (cosTheta < -1 + 0.001f)
            {
                rotationAxis = Vector3.Cross(Vector3.UnitZ, start);
                if (rotationAxis.Length < 0.001)
                {
                    rotationAxis = Vector3.Cross(Vector3.UnitX, start);
                }

                rotationAxis = Vector3.Normalize(rotationAxis);
                return (Quaternion.FromAxisAngle(rotationAxis, MathHelper.DegreesToRadians(0)), Quaternion.FromAxisAngle(rotationAxis, MathHelper.DegreesToRadians(180)));
            }

            rotationAxis = Vector3.Cross(start, dest);

            float s = MathF.Sqrt((1 + cosTheta) * 2);
            float invs = 1 / s;

            return (Quaternion.FromAxisAngle(rotationAxis, MathHelper.DegreesToRadians(0)), new Quaternion(rotationAxis.X * invs, rotationAxis.Y * invs, rotationAxis.Z * invs, s * 0.5f));
        }
    }
}
