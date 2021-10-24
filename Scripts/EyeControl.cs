using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rigr.Utils;

namespace Rigr
{
    public class EyeControl
    {
        private readonly Transform[] m_eyelidBones = null;
        private float[] m_topRots = null;
        private float[] m_botRots = null;
        private readonly AnimationCurve m_topCurve = null;
        private readonly AnimationCurve m_botCurve = null;

        private readonly Vector3[] m_basePositions;
        private readonly Quaternion[] m_baseLocalRots;
        private readonly Quaternion[] m_baseRots;

        private readonly Transform m_eyeballBone = null;
        private readonly Vector3 m_baseEyeballPos;
        private readonly Quaternion m_baseEyeballLocalRot;
        private readonly Quaternion m_baseEyeballRot;
        private readonly Bounds m_eyeballBounds;

        public EyeControl(SpriteRenderer whiteEyeSprite, Transform[] eyelidBones, Transform eyeballBone)
        {
            m_eyelidBones = eyelidBones;
            m_eyeballBone = eyeballBone;
            var eyeShape = CurveUtils.CreateSmoothCurve(whiteEyeSprite);
            m_topCurve = eyeShape.Top;
            m_botCurve = eyeShape.Bot;

            m_basePositions = new Vector3[eyelidBones.Length];
            m_baseLocalRots = new Quaternion[eyelidBones.Length];
            m_baseRots = new Quaternion[eyelidBones.Length];

            for (int i = 0; i < eyelidBones.Length; i++)
            {
                m_basePositions[i] = eyelidBones[i].localPosition;
                m_baseLocalRots[i] = eyelidBones[i].localRotation;
                m_baseRots[i] = eyelidBones[i].rotation;
            }

            CalculateRotation(whiteEyeSprite);

            var eyeballBounds = whiteEyeSprite.bounds;
            m_baseEyeballPos = m_eyeballBone.parent.position - eyeballBounds.center;
            m_baseEyeballLocalRot = m_eyeballBone.localRotation;
            m_baseEyeballRot = m_eyeballBone.rotation;

            m_eyeballBounds = eyeballBounds;
            m_eyeballBounds.center = Vector3.zero;

            var mat = Matrix4x4.Rotate(m_baseEyeballRot);
            var rotatedMin = mat.MultiplyVector(m_eyeballBounds.min);
            var rotatedMax = mat.MultiplyVector(m_eyeballBounds.max);
            m_eyeballBounds.SetMinMax(new Vector3(
                    Mathf.Min(rotatedMin.x, rotatedMax.x),
                    Mathf.Min(rotatedMin.y, rotatedMax.y),
                    Mathf.Min(rotatedMin.z, rotatedMax.z)),
                new Vector3(
                    Mathf.Max(rotatedMin.x, rotatedMax.x),
                    Mathf.Max(rotatedMin.y, rotatedMax.y),
                    Mathf.Max(rotatedMin.z, rotatedMax.z)));
        }

        private void CalculateRotation(SpriteRenderer sprite)
        {
            var bounds = sprite.bounds;
            // rotations are inverted here don't ask why bc idk
            m_botRots = CalcRotation(m_eyelidBones, bounds, m_topCurve);
            m_topRots = CalcRotation(m_eyelidBones, bounds, m_botCurve);
        }

        private static float[] CalcRotation(Transform[] bones,
            Bounds bounds,
            AnimationCurve curve,
            float eps = 0.015f,
            float baseAngle = 0.0f)
        {
            float[] res = new float[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                float currEps = eps;
                var bone = bones[i];
                // get y & x in graph space
                float x = (bone.position.x - bounds.min.x) / bounds.size.x;
                x = Mathf.Clamp(x, 0.0f, 1.0f); // weird sometimes you go out of range
                float y = curve.Evaluate(x);

                if (x > 0.5f) currEps = -currEps;

                float x2 = x + currEps;
                float y2 = curve.Evaluate(x2);


                // scale it back by bounds
                x *= bounds.size.x;
                x2 *= bounds.size.x;
                y *= bounds.size.y;
                y2 *= bounds.size.y;

                if (currEps < 0)
                {
                    SwapInt(ref x, ref x2);
                    SwapInt(ref y, ref y2);
                }

                var topNorm = new Vector2(y - y2, x2 - x);
                // Vector2 topNorm = normFunc(x, x2, y, y2);
                float angle = Vector2.Angle(Vector2.up, topNorm);
                if (Vector2.Dot(topNorm, Vector2.left) < 0) angle = -angle;
                res[i] = angle + baseAngle;

                float botY = curve.Evaluate(x);
            }
            return res;
        }

        private static void SwapInt(ref float left, ref float right)
        {
            (left, right) = (right, left);
        }

        private void UpdateEyelidBones(Bounds bounds, float eyelid)
        {
            for (int i = 0; i < m_eyelidBones.Length; i++)
            {
                var bone = m_eyelidBones[i];
                var bonePos = bone.position;
                float x = (bonePos.x - bounds.min.x) / bounds.size.x;

                float topPos = m_topCurve.Evaluate(x);
                float botPos = m_botCurve.Evaluate(x);

                float y = Mathf.Lerp(botPos, topPos, eyelid) * bounds.size.y;
                var parent = bone.parent;

                var diff = new Vector3()
                {
                    y = y + bounds.min.y - parent.position.y //- bounds.center.y
                };
                diff = Matrix4x4.Rotate(m_baseRots[i]).MultiplyPoint3x4(diff);

                // var pos = bonePos;
                // pos.y = y * bounds.size.y + bounds.min.y;
                // bonePos = pos;
                // bone.position = bonePos;

                float rotationValue = eyelid > 0.5f
                    ? Mathf.Lerp(0.0f, m_topRots[i], (eyelid - 0.5f) * 2.0f)
                    : Mathf.Lerp(m_botRots[i], 0.0f, eyelid * 2.0f);
                // bone.rotation = Quaternion.AngleAxis(rotationValue, Vector3.forward);


                bone.FromMatrix(parent.localToWorldMatrix
                    * Matrix4x4.TRS(m_basePositions[i] + diff,
                        Quaternion.AngleAxis(rotationValue, Vector3.forward) * m_baseLocalRots[i],
                        Vector3.one));
            }
        }

        private void UpdateEyeball(Bounds whiteEyeBounds, Bounds eyeBallBounds, Vector2 eyeballCoord)
        {
            // eyeballCoord.x = Mathf.Clamp01(eyeballCoord.x);
            // eyeballCoord.y = Mathf.Clamp01(eyeballCoord.y);
            //
            // var eyeballExtents = eyeBallBounds.extents;
            // eyeballExtents.y = 0.0f;
            //
            // Vector2 min = whiteEyeBounds.min + eyeballExtents;
            // Vector2 max = whiteEyeBounds.max - eyeballExtents;
            //
            // float boneOffset = m_eyeballBone.position.y - eyeBallBounds.center.y;
            //
            // float x = eyeballCoord.x * (max.x - min.x) + min.x;
            //
            // float topPos = m_topCurve.Evaluate(eyeballCoord.x);
            // float botPos = m_botCurve.Evaluate(eyeballCoord.x);
            // float y = Mathf.Lerp(botPos, topPos, eyeballCoord.y) * (max.y - min.y) + min.y + boneOffset;
            // y = eyeballCoord.y * (max.y - min.y) + min.y + boneOffset;
            //
            // m_eyeballBone.position = new Vector3()
            // { x = x, y = y, z = m_eyeballBone.transform.position.z };

            eyeballCoord = Matrix4x4.Rotate(m_baseEyeballRot).MultiplyPoint3x4(eyeballCoord);
            eyeballCoord.x = (Mathf.Clamp(eyeballCoord.x, -1.0f, 1.0f) + 1) / 2.0f;
            eyeballCoord.y = (Mathf.Clamp(eyeballCoord.y, -1.0f, 1.0f) + 1) / 2.0f;

            var diff = new Vector3()
            {
                x = -Mathf.Lerp(m_eyeballBounds.min.x, m_eyeballBounds.max.x, Mathf.Abs(eyeballCoord.x)),
                y = -Mathf.Lerp(m_eyeballBounds.min.y, m_eyeballBounds.max.y, Mathf.Abs(eyeballCoord.y)),
            };

            var parent = m_eyeballBone.parent;
            m_eyeballBone.FromMatrix(parent.localToWorldMatrix
                * Matrix4x4.TRS(m_baseEyeballPos + diff, m_baseEyeballLocalRot, Vector3.one));
        }

        // Update is called once per frame
        public void UpdatePosition(Bounds whiteEyeBounds, Bounds eyeballBounds, Vector2 eyeballCoord, float eyelid)
        {
            UpdateEyeball(whiteEyeBounds, eyeballBounds, eyeballCoord);
            UpdateEyelidBones(whiteEyeBounds, eyelid);
        }
    }
}
