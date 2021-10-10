using UnityEngine;

[System.Serializable]
public class FaceTurnControl
{
    private readonly Transform m_leftCheekBone = null;
    private readonly Transform m_rightCheekBone = null;
    private readonly Transform m_leftEarBone = null;
    private readonly Transform m_rightEarBone = null;
    private readonly Transform m_leftEyeBone = null;
    private readonly Transform m_rightEyeBone = null;
    private readonly Transform m_faceTransform = null;

    private Bounds m_faceBounds = new Bounds();
    private Bounds m_leftCheekBounds = new Bounds();
    private Bounds m_rightCheekBounds = new Bounds();
    private Bounds m_leftEyeBounds = new Bounds();
    private Bounds m_rightEyeBounds = new Bounds();
    private Bounds m_leftEarBounds = new Bounds();
    private Bounds m_rightEarBounds = new Bounds();
    private Bounds m_centerHairBounds = new Bounds();
    private Bounds m_hairAccBounds = new Bounds();

    [SerializeField] private BoneMover m_faceBoneMover = null;
    [SerializeField] private BoneMover m_leftCheekBoneMover = null;
    [SerializeField] private BoneMover m_rightCheekBoneMover = null;
    [SerializeField] private BoneMover m_leftEarBoneMover = null;
    [SerializeField] private BoneMover m_rightEarBoneMover = null;
    [SerializeField] private BoneMover m_leftEyeBoneMover = null;
    [SerializeField] private BoneMover m_rightEyeBoneMover = null;
    [SerializeField] private BoneMover m_leftHairBgMover = null;
    [SerializeField] private BoneMover m_rightHairBgMover = null;
    [SerializeField] private BoneMover m_centerHairMover = null;
    [SerializeField] private BoneMover m_hairAccMover = null;

    private readonly BoneConfigSO m_boneConfigSo = null;

    public FaceTurnControl(FaceBones faceBones,  FaceSpriteRenderers faceSprites, BoneConfigSO config)
    {
        m_leftCheekBone = faceBones.LCheek;
        m_rightCheekBone = faceBones.RCheek;
        m_faceTransform = faceBones.Face;
        m_leftEarBone = faceBones.LEar;
        m_rightEarBone = faceBones.REar;
        m_leftEyeBone = faceBones.LEye;
        m_rightEyeBone = faceBones.REye;
        m_boneConfigSo = config;
        CreateBounds(faceBones, faceSprites);
        CreateBoneMovers(faceBones);
    }

    private void CreateBounds(FaceBones faceBones, FaceSpriteRenderers faceSprite)
    {
        var faceSpriteBounds = new Bounds();
        faceSpriteBounds.Encapsulate(faceSprite.LEyeBrow.bounds);
        faceSpriteBounds.Encapsulate(faceSprite.REyeBrow.bounds);
        faceSpriteBounds.Encapsulate(faceSprite.LWhiteEye.bounds);
        faceSpriteBounds.Encapsulate(faceSprite.RWhiteEye.bounds);
        faceSpriteBounds.Encapsulate(faceSprite.LEyeball.bounds);
        faceSpriteBounds.Encapsulate(faceSprite.REyeball.bounds);
        faceSpriteBounds.Encapsulate(faceSprite.Mouth.bounds);
        faceSpriteBounds.Encapsulate(faceSprite.Nose.bounds);

        // face bounds
        var faceBounds = faceSprite.Face.bounds;
        m_faceBounds.SetMinMax(-faceBounds.extents + faceSpriteBounds.extents,
            faceBounds.extents - faceSpriteBounds.extents);

        // Eyes
        {
            var noseBounds = faceSprite.Nose.bounds;
            var leftBoundsMax = new Vector3() {
                x = (noseBounds.min.x - faceSprite.LWhiteEye.bounds.max.x) / 1.5f
            };
            var rightBoundsMin = new Vector3() {
                x = (noseBounds.max.x - faceSprite.RWhiteEye.bounds.min.x) / 1.5f
            };

            m_leftEyeBounds.SetMinMax(Vector3.zero, leftBoundsMax);
            m_rightEyeBounds.SetMinMax(rightBoundsMin, Vector3.zero);
        }


        // left & right cheeks
        {
            var leftBoundsMax = new Vector3() {
                x = faceSprite.LEyeBrow.bounds.min.x - faceBones.LCheek.position.x
            };
            var rightBoundsMin = new Vector3() {
                x = faceSprite.REyeBrow.bounds.max.x - faceBones.RCheek.position.x
            };

            m_leftCheekBounds.SetMinMax(Vector3.zero, leftBoundsMax);
            m_rightCheekBounds.SetMinMax(rightBoundsMin, Vector3.zero);
        }

        // TODO: figure out a better system than this lmao
        m_leftEarBounds = m_leftCheekBounds;
        m_rightEarBounds = m_rightCheekBounds;

        // ears
        m_leftEarBounds = m_faceBounds;
        m_rightEarBounds = m_faceBounds;
        m_leftEarBounds.extents  = new Vector3() { x = m_leftEarBounds.extents.x  / 4.0f };
        m_rightEarBounds.extents = new Vector3() { x = m_rightEarBounds.extents.x / 4.0f };

        m_centerHairBounds = m_faceBounds;
        m_centerHairBounds.extents  = new Vector3() { x = m_centerHairBounds.extents.x / 2.0f };

        m_hairAccBounds = m_faceBounds;
        m_hairAccBounds.extents  = new Vector3() { x = m_hairAccBounds.extents.x / 3.0f };
    }

    private void CreateBoneMovers(FaceBones faceBones)
    {
        m_faceBoneMover = new BoneMover(m_faceTransform, m_faceBounds);
        m_leftCheekBoneMover = new BoneMover(m_leftCheekBone, m_leftCheekBounds);
        m_rightCheekBoneMover = new BoneMover(m_rightCheekBone, m_rightCheekBounds);
        m_leftEarBoneMover = new BoneMover(m_leftEarBone, m_leftEarBounds);
        m_rightEarBoneMover = new BoneMover(m_rightEarBone, m_rightEarBounds);
        m_leftEyeBoneMover = new BoneMover(m_leftEyeBone, m_leftEyeBounds);
        m_rightEyeBoneMover = new BoneMover(m_rightEyeBone, m_rightEyeBounds);
        m_leftHairBgMover = new BoneMover(faceBones.LHairBG, m_leftEarBounds);
        m_rightHairBgMover = new BoneMover(faceBones.RHairBG, m_rightEarBounds);
        m_centerHairMover = new BoneMover(faceBones.HairCenter, m_centerHairBounds);
        m_hairAccMover = new BoneMover(faceBones.HairAcc, m_hairAccBounds);
    }

    public void UpdatePosition(SpriteRenderer faceSpriteRenderer, Vector2 lookDir, float rotation)
    {
        lookDir = new Vector2()
        {
            x = Mathf.Clamp(lookDir.x, -1, 1),
            y = Mathf.Clamp(lookDir.y, -1, 1)
        };
        // var scale = (lookDir + Vector2.one) / 2.0f;
        //
        // var faceSpriteBounds = faceSpriteRenderer.bounds;
        // float lScale = Mathf.Clamp01(lookDir.x + 1.0f);
        // float rScale = Mathf.Clamp01(lookDir.x);

        m_faceBoneMover.Update(lookDir, m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.Face));

        m_leftCheekBoneMover.Update(lookDir,
            m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.Cheek, BoneConfigSO.Side.Left));
        m_rightCheekBoneMover.Update(lookDir,
            m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.Cheek, BoneConfigSO.Side.Right));

        m_leftEyeBoneMover.Update(lookDir,
            m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.Eye, BoneConfigSO.Side.Left));
        m_rightEyeBoneMover.Update(lookDir,
            m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.Eye, BoneConfigSO.Side.Right));

        m_leftEarBoneMover.Update(lookDir,
            m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.Ear, BoneConfigSO.Side.Left));
        m_rightEarBoneMover.Update(lookDir,
            m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.Ear, BoneConfigSO.Side.Right));

        m_leftHairBgMover.Update(lookDir,
            m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.HairBg, BoneConfigSO.Side.Left));
        m_rightHairBgMover.Update(lookDir,
            m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.HairBg, BoneConfigSO.Side.Right));

        m_centerHairMover.Update(lookDir, m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.HairCenter));
        m_hairAccMover.Update(lookDir, m_boneConfigSo.GetLerpMethod(BoneConfigSO.BoneName.HairAcc));
    }
}
