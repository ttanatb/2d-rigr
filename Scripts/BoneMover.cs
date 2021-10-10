using UnityEngine;

[System.Serializable]
public class BoneMover
{
    [SerializeField] private Transform m_bone = null;
    [SerializeField] private Bounds m_bounds = new Bounds();
    [SerializeField] private Quaternion m_baseQuat = Quaternion.identity;
    // protected readonly Matrix4x4 m_baseTransform;

    [SerializeField] private Vector3 m_basePosition;
    [SerializeField] private Quaternion m_baseLocQuat = Quaternion.identity;

    public enum LerpMethod
    {
        Invalid,
        Normal,
        Inverse,
        NegativeOnly,
        InverseNegativeOnly,
        PositiveOnly,
        InversePositiveOnly,
        Zero,
        One
    }

    public BoneMover(Transform bone, Bounds bounds)
    {
        // m_baseTransform =
        // Matrix4x4.Rotate(Quaternion.AngleAxis(bone.eulerAngles.z - parent.eulerAngles.z , Vector3.forward));
        // var parent = bone.parent;

        // basically, parent.worldToLocalMatrix.MultiplyVector(bone.position - parent.position)
        // m_basePosition = bone.position - parent.position;
        m_basePosition = bone.localPosition;
        m_baseLocQuat = bone.localRotation;
        m_baseQuat = bone.rotation;

        var mat = Matrix4x4.Rotate(m_baseQuat);
        var rotatedMin = mat.MultiplyVector(bounds.min);
        var rotatedMax = mat.MultiplyVector(bounds.max);
        bounds.SetMinMax(new Vector3(
                Mathf.Min(rotatedMin.x, rotatedMax.x),
                Mathf.Min(rotatedMin.y, rotatedMax.y),
                Mathf.Min(rotatedMin.z, rotatedMax.z)),
            new Vector3(
                Mathf.Max(rotatedMin.x, rotatedMax.x),
                Mathf.Max(rotatedMin.y, rotatedMax.y),
                Mathf.Max(rotatedMin.z, rotatedMax.z)));
        // bounds.SetMinMax(mat.MultiplyVector(bounds.min), mat.MultiplyVector(bounds.max));

        m_bounds = bounds;
        m_bone = bone;
    }

    // public void Update(float lerpValue)
    // {
    //     var diff = Vector3.Lerp(m_bounds.min, m_bounds.max, lerpValue);
    //     // diff = Vector3.zero;
    //     // m_bone.FromMatrix(parent.localToWorldMatrix
    //     //     * m_baseTransform
    //     //     * Matrix4x4.Translate(parent.worldToLocalMatrix.MultiplyPoint3x4(m_basePosition + diff)));
    //
    //     // m_bone.FromMatrix(parent.localToWorldMatrix);
    //
    //     // Move(diff);
    // }

    public void Update(Vector2 lerpValues)
    {
        Update(lerpValues, LerpMethod.Normal, LerpMethod.Normal);
    }

    public void Update(Vector2 lerpValues, params LerpMethod[] methods)
    {
        if (methods.Length > 2)
            Debug.LogErrorFormat("poo poo");

        lerpValues = Matrix4x4.Rotate(m_baseQuat).MultiplyPoint3x4(lerpValues);

        lerpValues.x = ApplyLerpMethod(lerpValues.x, methods[0]);
        lerpValues.y = ApplyLerpMethod(lerpValues.y,
            (methods.Length == 1 ? methods[0] : methods[1]));

        var diff = new Vector3()
        {
            x = Mathf.Lerp(m_bounds.min.x, m_bounds.max.x, Mathf.Abs(lerpValues.x)),
            y = -Mathf.Lerp(m_bounds.min.y, m_bounds.max.y, Mathf.Abs(lerpValues.y)),
        };

        var parent = m_bone.parent;
        m_bone.FromMatrix(parent.localToWorldMatrix
            * Matrix4x4.TRS(m_basePosition + diff, m_baseLocQuat, Vector3.one));
    }

    private float ApplyLerpMethod(float value, LerpMethod method)
    {
        switch (method)
        {
            case LerpMethod.Normal:
                return (value + 1) / 2.0f;
            case LerpMethod.Inverse:
                return (value - 1) / -2.0f;
            case LerpMethod.NegativeOnly:
                return Mathf.Clamp01(value + 1);
            case LerpMethod.InverseNegativeOnly:
                return Mathf.Clamp01(-value);
            case LerpMethod.PositiveOnly:
                return Mathf.Clamp01(value);
            case LerpMethod.InversePositiveOnly:
                return -Mathf.Clamp01(value) + 1.0f;
            case LerpMethod.Zero:
                return 0;
            case LerpMethod.One:
                return 1;
            default:
                Debug.LogErrorFormat("Received unhandled lerp method: {0}", method);
                break;
        }

        return value;
    }

}
