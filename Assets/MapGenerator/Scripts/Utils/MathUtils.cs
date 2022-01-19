using UnityEngine;

public static class MathUtils
{
    public static float Saturate(float value)
    {
        return Mathf.Max(Mathf.Min(value, 1), 0);
    }

    public static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }

    public static Vector2 Floor(Vector2 vector)
    {
        return new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
    }

    public static float smooth_snap(float t, float m)
    {
        // input: t in [0..1]
        // maps input to an output that goes from 0..1,
        // but spends most of its time at 0 or 1, except for
        // a quick, smooth jump from 0 to 1 around input values of 0.5.
        // the slope of the jump is roughly determined by 'm'.
        // note: 'm' shouldn't go over ~16 or so (precision breaks down).

        //float t1 =     pow((  t)*2, m)*0.5;
        //float t2 = 1 - pow((1-t)*2, m)*0.5;
        //return (t > 0.5) ? t2 : t1;

        // optimized:
        float c = (t > 0.5) ? 1 : 0;
        float s = 1 - c * 2;
        return c + s * Mathf.Pow((c + s * t) * 2, m) * 0.5f;
    }
}
