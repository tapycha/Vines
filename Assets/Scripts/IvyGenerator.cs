using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class IvyGenerator : MonoBehaviour
{
    [SerializeField] private int ivyAmount;
    [SerializeField] private int ivyLength;
    [SerializeField] private int ivyJoinAngleLimitMin;
    [SerializeField] private int ivyJoinAngleLimitMax;
    [SerializeField] private float deltaYDistance;
    [SerializeField] private float deltaSDistance;
    [SerializeField] private float fallThreshold;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float width;
    [SerializeField] private bool isDebug;


    private const float DebugTime = 2000;

    void Start()
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();
        A();
        sw.Stop();

        Debug.Log("Elapsed=" + sw.Elapsed);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            A();
            sw.Stop();

            Debug.Log("Elapsed=" + sw.Elapsed);
        }
    }

    private void A()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        var ray = new Ray(transform.position, transform.up);

        for (var i = 0; i < ivyAmount; i++)
        {
            var pos = new List<Ray>();
            SpawnIvy(ref pos, ray, ivyLength, deltaYDistance, deltaSDistance);
            if (isDebug)
            {
                for (int j = 0; j < pos.Count - 1; j++)
                {
                    Debug.DrawLine(pos[j].origin, pos[j + 1].origin, Color.yellow, DebugTime);
                }
            }

            var pipe = transform.GetComponent<ProceduralMesh>();
            pipe.MakeMesh(pos, width);
        }
    }

    private void SpawnIvy(ref List<Ray> pos, Ray spawnPoint, int length, float deltaUp, float deltaSide,
        Vector3 prevDir = default)
    {
        for (var i = 0; i < length; i++)
        {
            if (pos.Count > 0)
            {
                var previousPoint = pos[pos.Count - 1];
                var distance =
                    new Plane(previousPoint.direction, previousPoint.origin).GetDistanceToPoint(spawnPoint.origin);
                if (distance < -fallThreshold)
                {
                    var midDir = (previousPoint.direction + spawnPoint.direction).normalized * deltaUp;
                    var newPoint = GetMid2PointExtrude(spawnPoint, previousPoint);
                    if (previousPoint.direction == spawnPoint.direction)
                    {
                        newPoint = GetPointPlaneIntersect(newPoint, previousPoint);
                    }

                    pos.Add(new Ray(newPoint, midDir));
                    if (isDebug)
                    {
                        var newDir = previousPoint.origin - spawnPoint.origin;
                        Debug.DrawRay(spawnPoint.origin - newDir * 0.1f, newDir * 1.1f, Color.red, DebugTime);
                        Debug.DrawRay(newPoint, midDir, Color.green, DebugTime);
                    }
                }
            }
            else
            {
                if (isDebug)
                {
                    Debug.DrawRay(spawnPoint.origin, spawnPoint.direction * deltaUp, Color.cyan, DebugTime);
                }
            }

            pos.Add(spawnPoint);
            var raycastOrigin = spawnPoint.origin;
            var dir = spawnPoint.direction * deltaUp;
            var isGoodPos = false;
            raycastOrigin += dir;
            Vector3 lhs;
            if (prevDir == default)
            {
                lhs = Random.insideUnitSphere;
            }
            else
            {
                var angle = Random.value > 0.5f //if true right else left turn
                    ? Random.Range(90 - ivyJoinAngleLimitMax, 90 - ivyJoinAngleLimitMin)
                    : Random.Range(90 + ivyJoinAngleLimitMin, 90 + ivyJoinAngleLimitMax);
                lhs = Quaternion.AngleAxis(angle, spawnPoint.direction) * prevDir;
            }

            dir = Vector3.Cross(lhs, dir).normalized * deltaSide;
            if (isDebug)
            {
                Debug.DrawRay(raycastOrigin, dir, Color.red, DebugTime);
            }

            if (Physics.Raycast(raycastOrigin, dir, out var hitInfo, deltaSide, layerMask))
            {
                isGoodPos = true;
            }
            else
            {
                var newPos = raycastOrigin + dir;
                if (isDebug)
                {
                    Debug.DrawRay(newPos, -spawnPoint.direction * deltaSide, Color.magenta, DebugTime);
                }

                if (Physics.Raycast(newPos, -spawnPoint.direction, out hitInfo, deltaSide, layerMask))
                {
                    isGoodPos = true;
                }
                else
                {
                    newPos -= spawnPoint.direction * deltaSide;
                    if (isDebug)
                    {
                        Debug.DrawRay(newPos, -dir, Color.magenta, DebugTime);
                    }

                    if (Physics.Raycast(newPos, -dir, out hitInfo, deltaSide, layerMask))
                    {
                        isGoodPos = true;
                    }
                }
            }

            if (isGoodPos)
            {
                if (isDebug)
                {
                    if (prevDir != default)
                    {
                        Debug.DrawRay(spawnPoint.origin + hitInfo.normal * 0.05f, prevDir, Color.grey, DebugTime);
                        Debug.DrawLine(spawnPoint.origin + hitInfo.normal * 0.1f, hitInfo.point + hitInfo.normal * 0.1f,
                            Color.green, DebugTime);
                    }
                }

                var newPrevDir = hitInfo.point - spawnPoint.origin;
                spawnPoint = new Ray(hitInfo.point, hitInfo.normal);
                prevDir = newPrevDir;
            }
            else
            {
                break;
            }
        }

        pos.Add(spawnPoint);
    }

    private Vector3 GetPointPlaneIntersect(Vector3 point1, Ray point2) =>
        Vector3.ProjectOnPlane(point1 - point2.origin, point2.direction) + point2.origin;

    private Vector3 GetMid2PointExtrude(Ray point1, Ray point2)
    {
        var a = GetPointPlaneIntersect(point1.origin, point2);
        var b = GetPointPlaneIntersect(point2.origin, point1);
        // Debug.DrawLine(a, b, Color.blue, DebugTime);
        return (b - a) / 2f + a;
    }
}