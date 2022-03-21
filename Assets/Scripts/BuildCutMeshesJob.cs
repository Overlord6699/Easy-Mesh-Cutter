using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
internal struct BuildCutMeshesJob : IJob
{
    [ReadOnly] public Mesh.MeshData Mesh;
    [ReadOnly] public NativeArray<TriangleCut> TriangleCuts;
    [ReadOnly] public NativeArray<int> AboveCutTriangles;
    [ReadOnly] public NativeArray<int> BellowCutTriangles;

    public Mesh.MeshDataArray Result;

    public void Execute()
    {
        var aboveCutIndexCount = AboveCutTriangles.Length;
        var bellowCutIndexCount = BellowCutTriangles.Length;
        foreach (var triangleCut in TriangleCuts)
        {
            if (triangleCut.Cut1.From == triangleCut.Cut2.From)
            {
                aboveCutIndexCount += 3;
                bellowCutIndexCount += 6;
            }
            else
            {
                aboveCutIndexCount += 6;
                bellowCutIndexCount += 3;
            }
        }

        var aboveIndices = new NativeArray<int>(aboveCutIndexCount, Allocator.Temp);
        var bellowIndices = new NativeArray<int>(bellowCutIndexCount, Allocator.Temp);
        NativeArray<int>.Copy(AboveCutTriangles, aboveIndices, AboveCutTriangles.Length);
        NativeArray<int>.Copy(BellowCutTriangles, bellowIndices, BellowCutTriangles.Length);

        var meshBuffers = new MeshBuffers(Mesh, Allocator.Temp);
        var additionalPoints = new NativeList<Vertex>(TriangleCuts.Length * 2, Allocator.Temp);

        var aboveIndex = AboveCutTriangles.Length;
        var bellowIndex = BellowCutTriangles.Length;
        var vertexCount = Mesh.vertexCount;
        foreach (var triangleCut in TriangleCuts)
        {
            if (triangleCut.Cut1.From == triangleCut.Cut2.From)
            {
                var v0Index = triangleCut.Cut1.From;
                var v1Index = triangleCut.Cut1.To;
                var v2Index = triangleCut.Cut2.To;
                var v0 = meshBuffers[v0Index];
                var v1 = meshBuffers[v1Index];
                var v2 = meshBuffers[v2Index];

                var q1 = Vertex.Lerp(v0, v1, triangleCut.Cut1.Intersection);
                var q2 = Vertex.Lerp(v0, v2, triangleCut.Cut2.Intersection);

                var q1Index = additionalPoints.Length + vertexCount;
                var q2Index = additionalPoints.Length + vertexCount + 1;

                additionalPoints.Add(q1);
                additionalPoints.Add(q2);

                aboveIndices[aboveIndex++] = v0Index;
                aboveIndices[aboveIndex++] = q1Index;
                aboveIndices[aboveIndex++] = q2Index;

                bellowIndices[bellowIndex++] = v1Index;
                bellowIndices[bellowIndex++] = v2Index;
                bellowIndices[bellowIndex++] = q1Index;

                bellowIndices[bellowIndex++] = v2Index;
                bellowIndices[bellowIndex++] = q2Index;
                bellowIndices[bellowIndex++] = q1Index;
            }
            else
            {
                var v0Index = triangleCut.Cut1.To;
                var v1Index = triangleCut.Cut1.From;
                var v2Index = triangleCut.Cut2.From;
                var v0 = meshBuffers[v0Index];
                var v1 = meshBuffers[v1Index];
                var v2 = meshBuffers[v2Index];
                var q1 = Vertex.Lerp(v1, v0, triangleCut.Cut1.Intersection);
                var q2 = Vertex.Lerp(v2, v0, triangleCut.Cut2.Intersection);

                var q1Index = additionalPoints.Length + vertexCount;
                var q2Index = additionalPoints.Length + vertexCount + 1;

                additionalPoints.Add(q1);
                additionalPoints.Add(q2);

                bellowIndices[bellowIndex++] = v0Index;
                bellowIndices[bellowIndex++] = q2Index;
                bellowIndices[bellowIndex++] = q1Index;

                aboveIndices[aboveIndex++] = v1Index;
                aboveIndices[aboveIndex++] = q1Index;
                aboveIndices[aboveIndex++] = v2Index;

                aboveIndices[aboveIndex++] = v2Index;
                aboveIndices[aboveIndex++] = q1Index;
                aboveIndices[aboveIndex++] = q2Index;
            }
        }

        var resultMesh0 = Result[0];
        var resultMesh1 = Result[1];
        meshBuffers.CopyBuffers(resultMesh0, additionalPoints);
        meshBuffers.CopyBuffers(resultMesh1, additionalPoints);

        resultMesh0.SetIndexBufferParams(aboveIndices.Length, IndexFormat.UInt32);
        var resultMesh0Indices = resultMesh0.GetIndexData<int>();
        aboveIndices.CopyTo(resultMesh0Indices);
        resultMesh0.subMeshCount = 1;
        resultMesh0.SetSubMesh(0, new SubMeshDescriptor(0, aboveIndices.Length, MeshTopology.Triangles),
            MeshUpdateFlags.Default);

        resultMesh1.SetIndexBufferParams(bellowIndices.Length, IndexFormat.UInt32);
        var resultMesh1Indices = resultMesh1.GetIndexData<int>();
        bellowIndices.CopyTo(resultMesh1Indices);
        resultMesh1.subMeshCount = 1;
        resultMesh1.SetSubMesh(0, new SubMeshDescriptor(0, bellowIndices.Length, MeshTopology.Triangles),
            MeshUpdateFlags.Default);

        meshBuffers.Dispose();
        aboveIndices.Dispose();
        bellowIndices.Dispose();
        additionalPoints.Dispose();
    }
}