using UnityEngine;

namespace Rigr
{
    [System.Serializable]
    public struct FaceBones
    {
        public Transform[] LEyeBrows;
        public Transform[] REyeBrows;
        public Transform[] LEyelids;
        public Transform[] REyelids;
        public Transform LEyeball;
        public Transform REyeball;
        public Transform LEye;
        public Transform REye;
        public Transform Nose;
        public Transform Face;
        public Transform LCheek;
        public Transform RCheek;
        public Transform LEar;
        public Transform REar;
        public Transform Head;
        public Transform LLip;
        public Transform RLip;
        public Transform[] UpperLips;
        public Transform[] LowerLips;
        public Transform LHairBG;
        public Transform RHairBG;
        public Transform HairCenter;
        public Transform HairAcc;

        public FaceBones(BoneConfigSO boneConfig, Transform root)
        {
            LEyeBrows = BoneFinder.FindAllBonesWithName(
                boneConfig.GetName(BoneConfigSO.BoneName.Eyebrow, BoneConfigSO.Side.Left), root);
            REyeBrows = BoneFinder.FindAllBonesWithName(
                boneConfig.GetName(BoneConfigSO.BoneName.Eyebrow, BoneConfigSO.Side.Right), root);
            LEyelids = BoneFinder.FindAllBonesWithName(
                boneConfig.GetName(BoneConfigSO.BoneName.Eyelid, BoneConfigSO.Side.Left), root);
            REyelids = BoneFinder.FindAllBonesWithName(
                boneConfig.GetName(BoneConfigSO.BoneName.Eyelid, BoneConfigSO.Side.Right), root);
            LEyeball = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Eyeball, BoneConfigSO.Side.Left), root);
            REyeball = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Eyeball, BoneConfigSO.Side.Right), root);
            LEye = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Eye, BoneConfigSO.Side.Left), root);
            REye = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Eye, BoneConfigSO.Side.Right), root);

            Nose = BoneFinder.FindBoneWithPrefix(boneConfig.GetName(BoneConfigSO.BoneName.Nose), root);
            Face = BoneFinder.FindBoneWithPrefix(boneConfig.GetName(BoneConfigSO.BoneName.Face), root);

            LCheek = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Cheek, BoneConfigSO.Side.Left), root);
            RCheek = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Cheek, BoneConfigSO.Side.Right), root);
            LEar = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Ear, BoneConfigSO.Side.Left), root);
            REar = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Ear, BoneConfigSO.Side.Right), root);

            Head = BoneFinder.FindBoneWithPrefix(boneConfig.GetName(BoneConfigSO.BoneName.Head), root);

            LLip = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Lip, BoneConfigSO.Side.Left), root);
            RLip = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.Lip, BoneConfigSO.Side.Right), root);

            UpperLips = BoneFinder.FindAllBonesWithName(boneConfig.GetName(BoneConfigSO.BoneName.UpperLip), root);
            LowerLips = BoneFinder.FindAllBonesWithName(boneConfig.GetName(BoneConfigSO.BoneName.LowerLip), root);

            LHairBG = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.HairBg, BoneConfigSO.Side.Left), root);
            RHairBG = BoneFinder.FindBoneWithPrefix(
                boneConfig.GetName(BoneConfigSO.BoneName.HairBg, BoneConfigSO.Side.Right), root);
            HairCenter = BoneFinder.FindBoneWithPrefix(boneConfig.GetName(BoneConfigSO.BoneName.HairCenter), root);
            HairAcc = BoneFinder.FindBoneWithPrefix(boneConfig.GetName(BoneConfigSO.BoneName.HairAcc), root);
        }
    }
}
