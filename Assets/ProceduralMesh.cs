using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
    private Mesh _mesh;

    private Vector3[] _vertices;
    private int[] _triangles;
    private Vector2[] _uv;

    private int verticeOffset;
    private int trianglesOffset;

    private const float directionOffset = 0.01f;
    private readonly int[] triangleVecticeOffset = { 0, 2, 1, 1, 2, 3, 2, 4, 3, 3, 4, 5 };

    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
    }

    public void MakeMesh(List<Ray>[] pos, float width)
    {
        var totalPositionsCount = pos.Sum(t => t.Count);
        var verticesCount = (totalPositionsCount - 1) * 4;
        var trianglesCount = ((totalPositionsCount - 1) * 2 - 1) * 6;
        _vertices = new Vector3[verticesCount];
        _uv = new Vector2[verticesCount];
        _triangles = new int[trianglesCount];
        verticeOffset = trianglesOffset = 0;
        foreach (var t in pos)
        {
            MakeMeshData(t, width);
        }

        CreateMesh();
    }

    private Vector3 GetLeftRightPoints(Ray point, Ray nextPoint, float width) =>
        Vector3.Cross(nextPoint.origin - point.origin, point.direction).normalized * width;

    private void MakeMeshData(List<Ray> positions, float width)
    {
        var verticesCount = (positions.Count - 1) * 4;
        var trianglesCount = ((positions.Count - 1) * 2 - 1) * 6;
        var prevOffset = GetLeftRightPoints(positions[0], positions[1], width);
        var firstPos = GetRelativePosition(positions[0]);
        _vertices[verticeOffset] = firstPos + prevOffset;
        _vertices[verticeOffset + 1] = firstPos - prevOffset;
        _uv[verticeOffset] = Vector2.zero;
        _uv[verticeOffset + 1] = Vector2.up;
        var verticesLinePart = 1 / (positions.Count * 2f);
        for (var i = 1; i < positions.Count; i++)
        {
            var offset = i + 1 == positions.Count
                ? GetLeftRightPoints(positions[i - 1], positions[i], width)
                : GetLeftRightPoints(positions[i], positions[i + 1], width);
            if (i == 1)
            {
                var verOffset = i * 2 + verticeOffset;
                var pastDirection = positions[i - 1].origin - positions[i].origin;
                var nextPos = GetRelativePosition(positions[i]) + pastDirection.normalized * width;
                _vertices[verOffset] = nextPos + offset;
                _vertices[verOffset + 1] = nextPos - offset;
                _uv[verOffset] = new Vector2(verticesLinePart, 0);
                _uv[verOffset + 1] = new Vector2(verticesLinePart, 1);
                var prev = i - 1;
                var triOffset = prev * 6 + trianglesOffset;
                var verTrisOffset = prev * 2 + verticeOffset;
                for (var j = 0; j < 6; j++)
                {
                    _triangles[triOffset + j] = verTrisOffset + triangleVecticeOffset[j];
                }
            }
            else
            {
                var verOffset = (i - 1) * 4 + verticeOffset;
                var pastDirection = positions[i - 1].origin - positions[i].origin;
                var nextPos = GetRelativePosition(positions[i]) + pastDirection.normalized * width;
                var prevPos = GetRelativePosition(positions[i - 1]) - pastDirection.normalized * width;
                _vertices[verOffset] = prevPos + offset;
                _vertices[verOffset + 1] = prevPos - offset;
                _vertices[verOffset + 2] = nextPos + offset;
                _vertices[verOffset + 3] = nextPos - offset;
                var uvX = (i * 2 - 1) * verticesLinePart;
                _uv[verOffset] = new Vector2(uvX, 0);
                _uv[verOffset + 1] = new Vector2(uvX, 1);
                _uv[verOffset + 2] = new Vector2(uvX + verticesLinePart, 0);
                _uv[verOffset + 3] = new Vector2(uvX + verticesLinePart, 1);
                var triOffset = (i - 2) * 12 + 6 + trianglesOffset;
                var verTrisOffset = (i - 2) * 4 + 2 + verticeOffset;
                for (var j = 0; j < 12; j++)
                {
                    _triangles[triOffset + j] = verTrisOffset + triangleVecticeOffset[j];
                }
            }
        }

        verticeOffset += verticesCount;
        trianglesOffset += trianglesCount;
    }

    private Vector3 GetRelativePosition(Ray ray) => ray.origin - transform.position + ray.direction * directionOffset;

    private void CreateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uv;
        _mesh.RecalculateNormals();
    }
}