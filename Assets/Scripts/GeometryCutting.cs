using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class GeometryCutting : MonoBehaviour
{
    [SerializeField] private Vector3 normal = Vector3.up;
    [SerializeField] private Vector3 point = Vector3.zero;
    [SerializeField] private float offset = 0.1f;

    [SerializeField] private Mesh mesh;

    [SerializeField] private MeshFilter meshFilter1;
    [SerializeField] private MeshFilter meshFilter2;

    private Mesh mesh1;
    private Mesh mesh2;

    private void OnValidate()
    {
        Invalidate();
    }

    public void Invalidate()
    {
        if (mesh == null) return;
        var job = new CutJob()
        {
            CutPlane = new Plane(normal, point),
            Mesh = Mesh.AcquireReadOnlyMeshData(mesh)[0],
            TriangleCuts = new NativeList<TriangleCut>(Allocator.TempJob),
            Result = new NativeArray<bool>(1, Allocator.TempJob),
            AboveCutTriangles = new NativeList<int>((int)mesh.GetIndexCount(0), Allocator.TempJob),
            BellowCutTriangles = new NativeList<int>((int)mesh.GetIndexCount(0), Allocator.TempJob)
        };
        job.Run();

        if (job.Result[0])
        {
            BuildMeshes(job);

            meshFilter1.transform.localPosition = normal.normalized * offset;
            meshFilter2.transform.localPosition = normal.normalized * -offset;
        }

        job.TriangleCuts.Dispose();
        job.Result.Dispose();
        job.AboveCutTriangles.Dispose();
        job.BellowCutTriangles.Dispose();
    }

    private void BuildMeshes(in CutJob cutJob)
    {
        var meshes = Mesh.AllocateWritableMeshData(2);
        var job = new BuildCutMeshesJob
        {
            TriangleCuts = cutJob.TriangleCuts,
            Mesh = cutJob.Mesh,
            Result = meshes,
            AboveCutTriangles = cutJob.AboveCutTriangles,
            BellowCutTriangles = cutJob.BellowCutTriangles
        };
        job.Run();

        if (mesh1 == null) mesh1 = new Mesh();
        if (mesh2 == null) mesh2 = new Mesh();

        Mesh.ApplyAndDisposeWritableMeshData(meshes, new[] { mesh1, mesh2 }, MeshUpdateFlags.Default);

        if (meshFilter1 != null) meshFilter1.sharedMesh = mesh1;
        if (meshFilter2 != null) meshFilter2.sharedMesh = mesh2;
    }
}