using UnityEngine;

[System.Serializable]
public struct FaceSpriteRenderers
{
    public SpriteRenderer LEyeBrow;
    public SpriteRenderer REyeBrow;
    public SpriteRenderer LWhiteEye;
    public SpriteRenderer RWhiteEye;
    public SpriteRenderer LEyeball;
    public SpriteRenderer REyeball;
    public SpriteRenderer LEar;
    public SpriteRenderer REar;
    public SpriteRenderer Nose;
    public SpriteRenderer Mouth;
    public SpriteRenderer Face;

    public FaceSpriteRenderers(BoneConfigSO boneConfig, Transform root)
    {
        LEyeBrow = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.Eyebrow, BoneConfigSO.Side.Left), root)
            ?.TryGetComponent(out LEyeBrow);
        REyeBrow = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.Eyebrow, BoneConfigSO.Side.Right), root)
            ?.TryGetComponent(out REyeBrow);
        LWhiteEye = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.WhiteEye, BoneConfigSO.Side.Left), root)
            ?.TryGetComponent(out LWhiteEye);
        RWhiteEye = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.WhiteEye, BoneConfigSO.Side.Right),
            root)?.TryGetComponent(out RWhiteEye);
        LEyeball = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.Eyeball, BoneConfigSO.Side.Left), root)
            ?.TryGetComponent(out LEyeball);
        REyeball = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.Eyeball, BoneConfigSO.Side.Right), root)
            ?.TryGetComponent(out REyeball);
        LEar = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.Ear, BoneConfigSO.Side.Left), root)
            ?.TryGetComponent(out LEar);
        REar = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.Ear, BoneConfigSO.Side.Right), root)
            ?.TryGetComponent(out REar);
        Nose = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.Nose), root)?.TryGetComponent(out Nose);
        Mouth = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.Mouth), root)?.TryGetComponent(out Mouth);
        Face = null;
        BoneFinder.FindBoneWithName(boneConfig.GetName(BoneConfigSO.SpriteName.Face), root)?.TryGetComponent(out Face);
    }
}
