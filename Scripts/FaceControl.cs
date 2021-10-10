using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceControl : MonoBehaviour
{
    private EyeControl m_leftEye = null;
    private EyeControl m_rightEye = null;
    [SerializeField] private FaceTurnControl m_faceTurnControl = null;

    [SerializeField] private BoneConfigSO m_boneConfigSo = null;
    private FaceSpriteRenderers m_faceSprites = new FaceSpriteRenderers();
    private FaceBones m_faceBones = new FaceBones();

    [SerializeField] private Vector2 m_eyeballCoord = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 m_eyeballCoordL = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 m_eyeballCoordR = new Vector2(0.5f, 0.5f);
    [SerializeField] private bool m_lockEyeBall = true;

    [SerializeField] private float m_eyelid = 0.9f;
    [SerializeField] private float m_eyelidL = 0.9f;
    [SerializeField] private float m_eyelidR = 0.9f;
    [SerializeField] private bool m_lockEyelid = true;

    [SerializeField] private Vector2 m_lookDir = Vector2.zero;
    [SerializeField] private float m_headRot = 0f;

    [SerializeField] private Transform m_rootBone = null;

    [SerializeField] private AnimationCurve m_testCurve;

    private SpriteMaskMover[] m_spriteMaskMovers = null;

    // Start is called before the first frame update
    private void Start()
    {
        m_faceBones = new FaceBones(m_boneConfigSo, m_rootBone);

        var root = transform.root;
        m_faceSprites = new FaceSpriteRenderers(m_boneConfigSo, root);

        m_leftEye = new EyeControl(m_faceSprites.LWhiteEye, m_faceBones.LEyelids, m_faceBones.LEyeball);
        m_rightEye = new EyeControl(m_faceSprites.RWhiteEye, m_faceBones.REyelids, m_faceBones.REyeball);

        m_faceTurnControl = new FaceTurnControl(m_faceBones, m_faceSprites, m_boneConfigSo);

        m_spriteMaskMovers = root.GetComponentsInChildren<SpriteMaskMover>();
    }

    // Update is called once per frame
    private void Update()
    {

        if (m_lockEyeBall)
        {
            m_eyeballCoordL = m_eyeballCoord;
            m_eyeballCoordR = m_eyeballCoord;
        }
        if (m_lockEyelid)
        {
            m_eyelidL = m_eyelid;
            m_eyelidR = m_eyelid;
        }

        var lWhiteEyeBounds = m_faceSprites.LWhiteEye.bounds;
        m_leftEye.UpdatePosition(lWhiteEyeBounds, lWhiteEyeBounds, m_eyeballCoordL, m_eyelidL);
        var rWhiteEyeBounds = m_faceSprites.RWhiteEye.bounds;
        m_rightEye.UpdatePosition(rWhiteEyeBounds,rWhiteEyeBounds, m_eyeballCoordR, m_eyelidR);

        m_faceTurnControl.UpdatePosition(m_faceSprites.Face, m_lookDir, m_headRot);

        foreach (var mask in m_spriteMaskMovers)
            mask.Move();
    }
}
