using UnityEngine;

namespace Rigr.Utils
{
    public static class CurveUtils
    {
        public static SpriteShape CreateSmoothCurve(SpriteRenderer sprite)
        {
            var texture = sprite.sprite.texture;
            var colors = texture.GetPixels32();
            var rect = sprite.sprite.rect;

            // i'm a heathen and i use anim curves
            var topCurve = new AnimationCurve();
            var botCurve = new AnimationCurve();

            // TODO adjust this based on texture size probs
            const int xIncrement = 1;
            const int yIncrement = 1;

            for (int x = (int)rect.xMin; x < (int)rect.xMax; x += xIncrement)
            {
                int firstSolid = (int)rect.yMin;
                int lastSolid = (int)rect.yMax;
                bool prevIsTransparent = true;
                // TODO: i could traverse this in a more cache coherent manner but
                // do i really have the emotional capability to do so?
                for (int y = (int)rect.yMin; y < (int)rect.yMax; y += yIncrement)
                {
                    var pixel = colors[y * texture.width + x];
                    bool isTransparent = pixel.a <= 0; // TODO: use alpha cutoff
                    if (isTransparent && !prevIsTransparent)
                    {
                        lastSolid = y - 1;
                    }
                    if (!isTransparent && prevIsTransparent)
                    {
                        firstSolid = y;
                    }

                    prevIsTransparent = isTransparent;
                }

                botCurve.AddKey((x - rect.xMin) / rect.width,
                    (firstSolid - rect.yMin) / rect.height);
                topCurve.AddKey((x - rect.xMin) / rect.width,
                    (lastSolid - rect.yMin) / rect.height);
            }

            topCurve = SmoothCurve(topCurve);
            botCurve = SmoothCurve(botCurve);

            return new SpriteShape() { Top = topCurve, Bot = botCurve };
        }

        private static AnimationCurve SmoothCurve(AnimationCurve curve)
        {
            var keys = curve.keys;
            var points = new Vector2[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                points[i] = new Vector2(keys[i].time, keys[i].value);
            }

            points = AnimationCurveSmoothener.MakeSmoothCurve(points, 1.0f);
            var newCurve = new AnimationCurve();
            for (int i = 0; i < points.Length; i++)
            {
                newCurve.AddKey(points[i].x, points[i].y);
            }
            return newCurve;
        }
    }
}
