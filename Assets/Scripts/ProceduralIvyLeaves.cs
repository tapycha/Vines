using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralIvyLeaves : MonoBehaviour
{
    private Mesh _mesh;

    private Vector3[] _vertices;
    private int[] _triangles;
    private Vector2[] _uv;

    private int _verticesOffset;
    private int _trianglesOffset;
    private float _directionOffsetMin;
    private float _directionOffsetMax;
    private float _widthMult;

    private readonly int[] _triangleVerticesOffset = { 0, 2, 1, 1, 2, 3 };

    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
    }

    public void MakeMesh(List<Ray>[] pos, float width, float directionOffsetMin, float directionOffsetMax,
        float widthMult)
    {
        var totalPositionsCount = pos.Sum(t => t.Count);
        var verticesCount = totalPositionsCount * 4;
        var trianglesCount = totalPositionsCount * 6;
        _vertices = new Vector3[verticesCount];
        _uv = new Vector2[verticesCount];
        _triangles = new int[trianglesCount];
        _verticesOffset = _trianglesOffset = 0;
        _directionOffsetMin = directionOffsetMin;
        _directionOffsetMax = directionOffsetMax;
        _widthMult = widthMult;
        var longest = pos.Max(item => item.Count);
        GetComponent<MeshRenderer>().material.SetVector("Tiling", new Vector4(longest * 2, 1, 0, 0));
        foreach (var t in pos)
        {
            MakeMeshData(t, width, longest);
        }

        CreateMesh();
    }

    private Vector3 GetLeftRightPoints(Ray point, Ray nextPoint, float width) =>
        Vector3.Cross(nextPoint.origin - point.origin, point.direction).normalized * width;

    private void MakeMeshData(List<Ray> positions, float width, int longest)
    {
        var verticesCount = positions.Count * 4;
        var trianglesCount = positions.Count * 6;
        var verticesLinePart = 1 / (longest * 2f);
        for (var i = 0; i < positions.Count - 1; i++)
        {
            var offset = i + 1 == positions.Count
                ? GetLeftRightPoints(positions[i - 1], positions[i], width)
                : GetLeftRightPoints(positions[i], positions[i + 1], width);
            var verOffset = i * 4 + _verticesOffset;
            var dir = positions[i + 1].origin - positions[i].origin;
            var prevPos = GetRelativePosition(positions[i], _directionOffsetMin);
            var nextPos = GetRelativePosition(positions[i], _directionOffsetMax) + dir.normalized * width * _widthMult;
            _vertices[verOffset] = prevPos + offset;
            _vertices[verOffset + 1] = prevPos - offset;
            _vertices[verOffset + 2] = nextPos + offset;
            _vertices[verOffset + 3] = nextPos - offset;
            var uvX = i * 2 * verticesLinePart;
            _uv[verOffset] = new Vector2(uvX, 0);
            _uv[verOffset + 1] = new Vector2(uvX, 1);
            _uv[verOffset + 2] = new Vector2(uvX + verticesLinePart, 0);
            _uv[verOffset + 3] = new Vector2(uvX + verticesLinePart, 1);
            var triOffset = i * 6 + _trianglesOffset;
            var verTrisOffset = i * 4 + _verticesOffset;
            for (var j = 0; j < 6; j++)
            {
                _triangles[triOffset + j] = verTrisOffset + _triangleVerticesOffset[j];
            }
        }

        _verticesOffset += verticesCount;
        _trianglesOffset += trianglesCount;
    }

    private Vector3 GetRelativePosition(Ray ray, float up) => ray.origin - transform.position + ray.direction * up;

    private void CreateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uv;
        _mesh.RecalculateNormals();
    }
}