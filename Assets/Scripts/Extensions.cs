using UnityEngine;

public static class Extensions
{
    public static Vector3 NoY(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
}
