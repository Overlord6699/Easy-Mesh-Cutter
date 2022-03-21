using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
internal struct CutJob : IJob
{
    [ReadOnly] public Plane CutPlane;
    [ReadOnly] public Mesh.MeshData Mesh;

    [WriteOnly] public NativeArray<bool> Result;
    [WriteOnly] public NativeList<int> AboveCutTriangles;
    [WriteOnly] public NativeList<int> BellowCutTriangles;
    [WriteOnly] public NativeList<TriangleCut> TriangleCuts;

    public void Execute()
    {
        using var vertices = new NativeArray<Vector3>(Mesh.vertexCount, Allocator.Temp);
        Mesh.GetVertices(vertices);

        using var indices = new NativeArray<int>(Mesh.GetSubMesh(0).indexCount, Allocator.Temp);
        Mesh.GetIndices(indices, 0);

        Result[0] = false;

        for (int i = 0; i < indices.Length; i += 3)
        {
            var index0 = indices[i];
            var index1 = indices[i + 1];
            var index2 = indices[i + 2];

            var d0 = Vector3.Dot(CutPlane.normal, vertices[index0]) + CutPlane.distance;
            var d1 = Vector3.Dot(CutPlane.normal, vertices[index1]) + CutPlane.distance;
            var d2 = Vector3.Dot(CutPlane.normal, vertices[index2]) + CutPlane.distance;

            var isAbove0 = d0 > 0;
            var isAbove1 = d1 > 0;
            var isAbove2 = d2 > 0;

            if (isAbove0 && isAbove1 && isAbove2)
            {
                AboveCutTriangles.Add(index0);
                AboveCutTriangles.Add(index1);
                AboveCutTriangles.Add(index2);
                continue;
            }

            if (!isAbove0 && !isAbove1 && !isAbove2)
            {
                BellowCutTriangles.Add(index0);
                BellowCutTriangles.Add(index1);
                BellowCutTriangles.Add(index2);
                continue;
            }

            Result[0] = true;

            if (isAbove1 == isAbove2)
            {
                if (isAbove0)
                {
                    TriangleCuts.Add(new TriangleCut
                    {
                        Cut1 = new EdgeCut
                            { From = index0, To = index1, Intersection = Intersection(d0, d1) },
                        Cut2 = new EdgeCut
                            { From = index0, To = index2, Intersection = Intersection(d0, d2) }
                    });
                }
                else
                {
                    TriangleCuts.Add(new TriangleCut
                    {
                        Cut1 = new EdgeCut
                            { From = index2, To = index0, Intersection = Intersection(d2, d0) },
                        Cut2 = new EdgeCut
                            { From = index1, To = index0, Intersection = Intersection(d1, d0) }
                    });
                }

                continue;
            }

            if (isAbove0 == isAbove2)
            {
                if (isAbove1)
                {
                    TriangleCuts.Add(new TriangleCut
                    {
                        Cut1 = new EdgeCut
                            { From = index1, To = index2, Intersection = Intersection(d1, d2) },
                        Cut2 = new EdgeCut
                            { From = index1, To = index0, Intersection = Intersection(d1, d0) }
                    });
                }
                else
                {
                    TriangleCuts.Add(new TriangleCut
                    {
                        Cut1 = new EdgeCut
                            { From = index0, To = index1, Intersection = Intersection(d0, d1) },
                        Cut2 = new EdgeCut
                            { From = index2, To = index1, Intersection = Intersection(d2, d1) }
                    });
                }

                continue;
            }

            //if (isAbove0 == isAbove1) No condition check needed, this two values are equal anyway
            {
                if (isAbove2)
                {
                    TriangleCuts.Add(new TriangleCut
                    {
                        Cut1 = new EdgeCut
                            { From = index2, To = index0, Intersection = Intersection(d2, d0) },
                        Cut2 = new EdgeCut
                            { From = index2, To = index1, Intersection = Intersection(d2, d1) }
                    });
                }
                else
                {
                    TriangleCuts.Add(new TriangleCut
                    {
                        Cut1 = new EdgeCut
                            { From = index1, To = index2, Intersection = Intersection(d1, d2) },
                        Cut2 = new EdgeCut
                            { From = index0, To = index2, Intersection = Intersection(d0, d2) }
                    });
                }

                continue;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Intersection(float d1, float d2)
    {
        return d1 / (d1 + -d2);
    }
}