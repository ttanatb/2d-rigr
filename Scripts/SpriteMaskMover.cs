using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

// TODO: auto add this component in some init
public class SpriteMaskMover : MonoBehaviour
{
    private Transform m_bone = null;
    private Matrix4x4 m_baseTransform = Matrix4x4.identity;

    private void Start()
    {
        var thisTransform = this.transform;
        TryGetComponent(out SpriteSkin skin);
        m_bone = skin.boneTransforms[0];


        var rot = Quaternion.AngleAxis(thisTransform.eulerAngles.z - m_bone.eulerAngles.z, Vector3.forward);
        m_baseTransform = Matrix4x4.Rotate(rot) * Matrix4x4.Translate(thisTransform.position - m_bone.position);
    }

    // Start is called before the first frame update
    public void Move()
    {
        transform.FromMatrix(m_bone.localToWorldMatrix * m_baseTransform);
    }
}
