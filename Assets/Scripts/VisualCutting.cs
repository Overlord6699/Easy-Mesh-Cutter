using UnityEngine;

public class VisualCutting : MonoBehaviour
{
    private static readonly int cuttingPlanePoint = Shader.PropertyToID("_CuttingPlanePoint");
    private static readonly int cuttingPlaneNormal = Shader.PropertyToID("_CuttingPlaneNormal");

    [SerializeField] private Vector3 normal;
    [SerializeField] private Vector3 point;
    [SerializeField] private float offset = 0.1f;

    [SerializeField] private Material material;
    [SerializeField] private MeshRenderer renderer1;
    [SerializeField] private MeshRenderer renderer2;

    private void OnValidate()
    {
        Invalidate();
    }

    public void Invalidate()
    {
        if (material == null || renderer1 == null || renderer2 == null) return;

        if (renderer1.sharedMaterial == null
            || ReferenceEquals(renderer1.sharedMaterial, material)
            || !ReferenceEquals(renderer1.sharedMaterial.shader, material.shader))
            renderer1.sharedMaterial = new Material(material);

        if (renderer2.sharedMaterial == null
            || ReferenceEquals(renderer2.sharedMaterial, material)
            || !ReferenceEquals(renderer2.sharedMaterial.shader, material.shader))
            renderer2.sharedMaterial = new Material(material);

        renderer1.sharedMaterial.SetVector(cuttingPlaneNormal, normal);
        renderer1.sharedMaterial.SetVector(cuttingPlanePoint, point);
        renderer2.sharedMaterial.SetVector(cuttingPlaneNormal, -normal);
        renderer2.sharedMaterial.SetVector(cuttingPlanePoint, point);

        renderer1.transform.localPosition = normal.normalized * (offset * 0.5f);
        renderer2.transform.localPosition = normal.normalized * -(offset * 0.5f);
    }
}