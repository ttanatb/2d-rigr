using UnityEngine;

namespace Rigr
{
    public class MouthControl
    {
        private BoneMover m_leftCorner = null;
        private BoneMover m_rightCorner = null;

        private BoneMover[] m_upperLips = null;
        private BoneMover[] m_lowerLips = null;

        public MouthControl(FaceBones bones, FaceSpriteRenderers sprites, BoneConfigSO config)
        {
            var lipBounds = new Bounds {
                size = new Vector3() {
                    x = config.LipMovementWidth,
                    y = config.LipMovementHeight,
                }
            };
            m_leftCorner = new BoneMover(bones.LLip, lipBounds);
            m_rightCorner = new BoneMover(bones.RLip, lipBounds);

            // TODO: Create new class for upper/lower lips
            // up-down movement based on curve
            // left-right based on overall mouth width (compression)
            m_upperLips = new BoneMover[bones.UpperLips.Length];
            foreach (var bone in bones.UpperLips)
            {

            }
            m_lowerLips = new BoneMover[bones.LowerLips.Length];
        }
    }
}
