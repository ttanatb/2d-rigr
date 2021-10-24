using UnityEngine;
using System;

namespace Rigr.Utils
{
    public static class BoundsExtension
    {
        public enum XSide
        {
            Left,
            FifthPercentile,
            MidLeft,
            Center,
            MidRight,
            NinetyFifthPercentile,
            Right,
            Varied,
        }

        public enum YSide
        {
            Top,
            Center,
            Bottom,
            Varied,
            CenterNoAlpha,
        }

        public static Vector3 AdjacentToBounds(this Bounds bounds, YSide ySide, XSide xSide,
            int index = -1, int count = 0, int frontOffset = 0, int backOffset = 0,
            SpriteShape shape = new SpriteShape())
        {
            if (count > 1) count -= 1;
            count += backOffset + frontOffset;
            index += frontOffset;
            float factor = index / (float)count;
            var pos = bounds.center;
            switch (xSide)
            {
                case XSide.Left:
                    pos += Vector3.left * bounds.extents.x
                        + Vector3.right * bounds.size.x * 0.01f;
                    break;
                case XSide.FifthPercentile:
                    pos += Vector3.left * bounds.extents.x
                        + Vector3.right * bounds.size.x * 0.05f;
                    break;
                case XSide.MidLeft:
                    pos += Vector3.left * bounds.extents.x / 2.0f;
                    break;
                case XSide.Center:
                    break;
                case XSide.MidRight:
                    pos += Vector3.right * bounds.extents.x / 2.0f;
                    break;
                case XSide.NinetyFifthPercentile:
                    pos += Vector3.right * bounds.extents.x
                        + Vector3.left * bounds.size.x * 0.05f;
                    break;
                case XSide.Right:
                    pos += Vector3.right * bounds.extents.x
                        + Vector3.left * bounds.size.x * 0.01f;
                    break;
                case XSide.Varied:
                    if (count == 0 || index == -1)
                        break;
                    pos += Vector3.left * bounds.extents.x
                        + Vector3.right * bounds.size.x * 0.01f
                        + (Vector3.right * factor * bounds.size.x * 0.98f);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(xSide), xSide, null);
            }
            switch (ySide)
            {
                case YSide.Top:
                    pos += Vector3.up * bounds.extents.y;
                    break;
                case YSide.Center:
                    break;
                case YSide.Bottom:
                    pos += Vector3.down * bounds.extents.y;
                    break;
                case YSide.Varied:
                    if (count == 0 || index == -1)
                        break;
                    pos += Vector3.up * bounds.extents.y
                        + (Vector3.down * factor * bounds.size.y);
                    break;
                case YSide.CenterNoAlpha:
                    float x = Mathf.Clamp01((pos.x - bounds.min.x) / bounds.size.x);
                    float y = (shape.Top.Evaluate(x) + shape.Bot.Evaluate(x)) / 2.0f;
                    pos += Vector3.up * (y * bounds.size.y) + Vector3.down * bounds.extents.y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ySide), ySide, null);
            }
            return pos;
        }
    }
}
