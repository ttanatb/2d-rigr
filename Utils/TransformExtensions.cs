using UnityEngine;

namespace Rigr.Utils
{
    public static class TransformExtensions
    {
        public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.rotation = matrix.ExtractRotation();
            transform.position = matrix.ExtractPosition();
        }

        public static void FromMatrixWithScale(this Transform transform, Matrix4x4 matrix)
        {
            transform.localScale = matrix.ExtractScale();
            FromMatrix(transform, matrix);
        }
    }

// From https://forum.unity.com/threads/how-to-assign-matrix4x4-to-transform.121966/
}
