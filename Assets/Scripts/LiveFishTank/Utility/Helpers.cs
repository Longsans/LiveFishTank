using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static float ConvertAngleToRealistic(float angle)
    {
        if (angle > 180f)
            return -(360f - angle);
        if (angle < -180f)
            return 360f + angle;

        return angle;
    }
}
