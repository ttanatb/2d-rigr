using UnityEngine;

namespace Rigr
{
    [System.Serializable]
    public struct BoneSide
    {
        [SerializeField] private BoneConfigSO.BoneName m_boneName;
        [SerializeField] private BoneConfigSO.Side m_side;

        public BoneSide(BoneConfigSO.BoneName name, BoneConfigSO.Side side)
        {
            m_boneName = name;
            m_side = side;
        }
    }
}
