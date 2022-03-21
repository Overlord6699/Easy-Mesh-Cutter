using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

internal struct MeshBuffers : IDisposable
{
    private NativeArray<Vector3> positions;
    private NativeArray<Vector3> normals;
    private NativeArray<Color32> colors;
    private NativeArray<Vector4> uv0;

    private readonly bool hasNormals;
    private readonly bool hasColors;
    private readonly bool hasUv0;

    private NativeList<VertexAttributeDescriptor> attributeList;

    public MeshBuffers(Mesh.MeshData Mesh, Allocator allocator)
    {
        hasColors = Mesh.HasVertexAttribute(VertexAttribute.Color);
        hasNormals = Mesh.HasVertexAttribute(VertexAttribute.Normal);
        hasUv0 = Mesh.HasVertexAttribute(VertexAttribute.TexCoord0);

        positions = new NativeArray<Vector3>(Mesh.vertexCount, allocator);
        normals = hasNormals ? new NativeArray<Vector3>(Mesh.vertexCount, allocator) : default;
        colors = hasColors ? new NativeArray<Color32>(Mesh.vertexCount, allocator) : default;
        uv0 = hasUv0 ? new NativeArray<Vector4>(Mesh.vertexCount, allocator) : default;

        attributeList = new NativeList<VertexAttributeDescriptor>(16, allocator);

        Mesh.GetVertices(positions);
        var stream = 0;
        attributeList.Add(new VertexAttributeDescriptor(
            VertexAttribute.Position, VertexAttributeFormat.Float32, 3, stream++));
        if (hasNormals)
        {
            Mesh.GetNormals(normals);
            attributeList.Add(new VertexAttributeDescriptor(
                VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, stream++));
        }

        if (hasColors)
        {
            Mesh.GetColors(colors);
            attributeList.Add(
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4, stream++));
        }

        if (hasUv0)
        {
            Mesh.GetUVs(0, uv0);
            attributeList.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                VertexAttributeFormat.Float32, 4, stream));
        }
    }

    public void CopyBuffers(Mesh.MeshData newMesh, NativeArray<Vertex> additionalPoints)
    {
        newMesh.SetVertexBufferParams(positions.Length + additionalPoints.Length, attributeList);

        var stream = 0;
        var originalVertexCount = positions.Length;
        {
            var targetPositions = newMesh.GetVertexData<Vector3>(stream++);
            NativeArray<Vector3>.Copy(positions, targetPositions, originalVertexCount);
            for (int i = 0; i < additionalPoints.Length; i++)
            {
                targetPositions[i + originalVertexCount] = additionalPoints[i].Position;
            }

            if (hasNormals)
            {
                var targetNormals = newMesh.GetVertexData<Vector3>(stream++);
                NativeArray<Vector3>.Copy(normals, targetNormals, originalVertexCount);
                for (int i = 0; i < additionalPoints.Length; i++)
                {
                    targetNormals[i + originalVertexCount] = additionalPoints[i].Normal;
                }
            }

            if (hasColors)
            {
                var targetColors = newMesh.GetVertexData<Color32>(stream++);
                NativeArray<Color32>.Copy(colors, targetColors, originalVertexCount);
                for (int i = 0; i < additionalPoints.Length; i++)
                {
                    targetColors[i + originalVertexCount] = additionalPoints[i].Color;
                }
            }

            if (hasUv0)
            {
                var targetUv0 = newMesh.GetVertexData<Vector4>(stream++);
                NativeArray<Vector4>.Copy(uv0, targetUv0, originalVertexCount);
                for (int i = 0; i < additionalPoints.Length; i++)
                {
                    targetUv0[i + originalVertexCount] = additionalPoints[i].Uv0;
                }
            }
        }
    }

    public Vertex this[int index] =>
        new Vertex
        {
            Position = positions[index],
            Normal = hasNormals ? normals[index] : Vector3.zero,
            Color = hasColors ? colors[index] : (Color32)Color.white,
            Uv0 = hasUv0 ? uv0[index] : Vector4.zero
        };

    public void Dispose()
    {
        positions.Dispose();
        if (hasNormals) normals.Dispose();
        if (hasColors) colors.Dispose();
        if (hasUv0) uv0.Dispose();
        attributeList.Dispose();
    }
}