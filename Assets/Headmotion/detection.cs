using UnityEngine;

namespace headmotion
{
    public class detection
    {
        public static float LinearMap(float value, float s0, float s1, float d0, float d1)
        {
            return d0 + (value - s0) * (d1 - d0) / (s1 - s0);
        }

        public static float WrapAngle(float degree)
        {
            if (degree > 180f)
            {
                return degree - 360f;
            }
            return degree;
        }
    }
}