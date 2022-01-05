using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
    private Mesh _mesh;

    private Vector3[] _vertices;
    private int[] _triangles;

    private const float directionOffset = 0.01f;
    private const float width = 0.05f;

    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
    }

    public void MakeMesh(List<Ray> pos)
    {
        MakeMeshData(pos, width);
        CreateMesh();
    }

    private Vector3 GetLeftRightPoints(Ray point, Ray nextPoint, float width) =>
        Vector3.Cross(nextPoint.origin - point.origin, point.direction).normalized * width;

    private void MakeMeshData(List<Ray> positions, float width)
    {
        var verticesCount = (positions.Count - 1) * 4;
        var trianglesCount = ((positions.Count - 1) * 2 - 1) * 6;
        _vertices = new Vector3[verticesCount];
        _triangles = new int[trianglesCount];

        var prevOffset = GetLeftRightPoints(positions[0], positions[1], width);
        var firstPos = positions[0].origin - transform.position + positions[0].direction * directionOffset;
        _vertices[0] = firstPos + prevOffset;
        _vertices[1] = firstPos - prevOffset;
        for (var i = 1; i < positions.Count; i++)
        {
            var offset = i + 1 == positions.Count
                ? GetLeftRightPoints(positions[i - 1], positions[i], width)
                : GetLeftRightPoints(positions[i], positions[i + 1], width);
            var verOffset = i * 2;
            var nextPos = positions[i].origin - transform.position + positions[i].direction * directionOffset;
            _vertices[verOffset] = nextPos + offset;
            _vertices[verOffset + 1] = nextPos - offset;
            var prev = i - 1;
            var triOffset = prev * 6;
            var verTrisOffset = prev * 2;
            _triangles[triOffset] = verTrisOffset;
            _triangles[triOffset + 1] = verTrisOffset + 2;
            _triangles[triOffset + 2] = verTrisOffset + 1;
            _triangles[triOffset + 3] = verTrisOffset + 1;
            _triangles[triOffset + 4] = verTrisOffset + 2;
            _triangles[triOffset + 5] = verTrisOffset + 3;
        }
    }

    private void CreateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.RecalculateNormals();
    }
}