using UnityEngine;

internal struct Vertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Color32 Color;
    public Vector4 Uv0;

    public static Vertex Lerp(in Vertex a, in Vertex b, float t)
    {
        return new Vertex
        {
            Position = Vector3.Lerp(a.Position, b.Position, t),
            Normal = Vector3.Lerp(a.Normal, b.Normal, t),
            Color = Color32.Lerp(a.Color, b.Color, t),
            Uv0 = Vector4.Lerp(a.Uv0, b.Uv0, t),
        };
    }
}