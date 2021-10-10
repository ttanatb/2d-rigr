using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "BoneConfigSO", menuName = "2DRigr/BoneConfigSO", order = 0)]
public class BoneConfigSO : ScriptableObject
{
    [SerializeField] private string m_leftFormat = "{0}_l";
    [SerializeField] private string m_rightFormat = "{0}_r";
    [SerializeField] private string m_incrementFormat = "{0}_{1}";

    public enum Side
    {
        Invalid = 0,
        Left,
        Right
    }

    public enum BoneName
    {
        Invalid = 0,
        Eyebrow,
        Eyelid,
        Eyeball,
        Eye,
        Nose,
        Face,
        Cheek,
        Ear,
        Head,
        Mouth,
        Lip,
        UpperLip,
        LowerLip,
        HairBg,
        HairCenter,
        HairAcc,
        Neck,
    }

    public enum SpriteName
    {
        Invalid = 0,
        Eyebrow,
        Eyelid,
        EyelidBorder,
        WhiteEye,
        Eyeball,
        Ear,
        Nose,
        Mouth,
        Face,
        Neck,
        HairBg,
        HairBase,
        HairAcc,
        UpperLip,
        LowerLip,
    }

    [SerializeField] private BoneNameStringDictionary m_boneToNameDict = new BoneNameStringDictionary()
    {
        { BoneName.Eyebrow, "eyebrow" },
        { BoneName.Eyelid, "eyelid" },
        { BoneName.Eyeball, "iris" },
        { BoneName.Eye, "eye" },
        { BoneName.Nose, "nose" },
        { BoneName.Face, "face" },
        { BoneName.Cheek, "cheek" },
        { BoneName.Ear, "ear" },
        { BoneName.Head, "head" },
        { BoneName.Mouth, "mouth" },
        { BoneName.Lip, "lip" },
        { BoneName.UpperLip, "lip_top" },
        { BoneName.LowerLip, "lip_bot" },
        { BoneName.HairBg, "hair_bg" },
        { BoneName.HairCenter, "hair" },
        { BoneName.HairAcc, "hair_acc" },
        { BoneName.Neck, "neck" },
    };


    [SerializeField] private SpriteNameStringDictionary m_spriteToNameDict = new SpriteNameStringDictionary()
    {
        { SpriteName.Eyebrow, "brow" },
        { SpriteName.WhiteEye, "eye_white" },
        { SpriteName.Eyeball, "iris" },
        { SpriteName.Ear, "ear" },
        { SpriteName.Nose, "nose" },
        { SpriteName.Mouth, "mouth" },
        { SpriteName.Face, "face" },
        { SpriteName.Neck, "neck" },
        { SpriteName.HairBg, "hair_bg" },
        { SpriteName.HairBase, "hair_base" },
        { SpriteName.HairAcc, "hair_acc" },
        { SpriteName.UpperLip, "lip_top" },
        { SpriteName.LowerLip, "lip_bot" },
        { SpriteName.Eyelid, "eyelid" },
        { SpriteName.EyelidBorder, "eyelid_border" },
    };

    [SerializeField] private BoneSideLerpMethodDictionary m_boneToLerpMethodDict = new BoneSideLerpMethodDictionary()
    {
        { new BoneSide(BoneName.Face, Side.Invalid), BoneMover.LerpMethod.Normal },
        { new BoneSide(BoneName.Eye, Side.Left), BoneMover.LerpMethod.PositiveOnly },
        { new BoneSide(BoneName.Eye, Side.Right), BoneMover.LerpMethod.NegativeOnly },
        { new BoneSide(BoneName.Cheek, Side.Left), BoneMover.LerpMethod.InverseNegativeOnly },
        { new BoneSide(BoneName.Cheek, Side.Right), BoneMover.LerpMethod.InversePositiveOnly },
        { new BoneSide(BoneName.Ear, Side.Left), BoneMover.LerpMethod.InverseNegativeOnly },
        { new BoneSide(BoneName.Ear, Side.Right), BoneMover.LerpMethod.InversePositiveOnly },
        { new BoneSide(BoneName.HairBg, Side.Left), BoneMover.LerpMethod.InverseNegativeOnly },
        { new BoneSide(BoneName.HairBg, Side.Right), BoneMover.LerpMethod.InversePositiveOnly },
        { new BoneSide(BoneName.HairCenter, Side.Invalid), BoneMover.LerpMethod.Normal },
        { new BoneSide(BoneName.HairAcc, Side.Invalid), BoneMover.LerpMethod.Normal },
    };

    [SerializeField] private int m_lipBoneCount = 7;
    [SerializeField] private int m_eyelidBoneCount = 8;
    [SerializeField] private int m_eyebrowCount = 8;

    public int LipBoneCount => m_lipBoneCount;
    public int EyelidBoneCount => m_eyelidBoneCount;
    public int EyebrowBoneCount => m_eyebrowCount;

    public string IncrementFormat => m_incrementFormat;

    public string GetName(BoneName boneName, Side side = Side.Invalid)
    {
        if (!m_boneToNameDict.ContainsKey(boneName))
        {
            Debug.LogErrorFormat("No name registered for bone named {0}", boneName);
            return "";
        }

        string boneTransformName = m_boneToNameDict[boneName];
        if (side == Side.Invalid) return boneTransformName;

        return string.Format(
            side == Side.Left ? m_leftFormat : m_rightFormat,
            boneTransformName);
    }

    public string GetName(SpriteName spriteName, Side side = Side.Invalid)
    {
        if (!m_spriteToNameDict.ContainsKey(spriteName))
        {
            Debug.LogErrorFormat("No name registered for sprite named {0}", spriteName);
            return "";
        }

        string name = m_spriteToNameDict[spriteName];
        if (side == Side.Invalid) return name;

        return string.Format(
            side == Side.Left ? m_leftFormat : m_rightFormat,
            name);
    }

    public BoneMover.LerpMethod GetLerpMethod(BoneName boneName, Side side = Side.Invalid)
    {
        if (!m_boneToLerpMethodDict.ContainsKey(new BoneSide(boneName, side)))
        {
            Debug.LogError($"No look dir registered for bone named {boneName}-{side}");
            return BoneMover.LerpMethod.Invalid;
        }

        return m_boneToLerpMethodDict[new BoneSide(boneName, side)];
    }
}
