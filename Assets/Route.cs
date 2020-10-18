using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{

    [SerializeField]
    private int[] curveLengths = null;

    [SerializeField]
    [Min(1)]
    private int numDebugPoints = 0;

    [SerializeField]
    [Min(1)]
    private int numApproximationPoints = 20;

    private Transform[] controlPoints = null;

    public SortedList<float, GameObject> collidedObjects = new SortedList<float, GameObject>();

    public Bezier bezier = null;

    void Start()
    {
        SetControlPoints();
        bezier = new Bezier(controlPoints, curveLengths);

        Mesh detectionMesh = new Mesh();
        detectionMesh.vertices = bezier.GenAllPoints(numApproximationPoints);
        int[] triangles = new int[(numApproximationPoints - 1) * 3];
        for (int i = 0; i < (triangles.Length - 1); i += 3)
        {
            int current = i / 3;
            triangles[i] = current;
            triangles[i + 1] = current + 1;
            triangles[i + 2] = current;
        }
        detectionMesh.triangles = triangles;

        MeshCollider frontDetection = GetComponent<MeshCollider>();
        frontDetection.sharedMesh = detectionMesh;
    }

    private void OnDrawGizmos()
    {
        SetControlPoints();
        Bezier debugBezier = new Bezier(controlPoints, curveLengths);
        for (float t = 0; t <= 1; t += (1f / numDebugPoints))
        {
            Gizmos.DrawSphere(debugBezier.GenPoint(t), 0.25f);
        }
    }

    private void SetControlPoints()
    {
        int numControlPoints = transform.childCount;
        controlPoints = new Transform[numControlPoints];
        for (int i = 0; i < controlPoints.Length; i++)
        {
            controlPoints[i] = transform.GetChild(i);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        GameObject gameObject = collider.gameObject;
        if (gameObject.GetComponent<Move>() != null)
        {
            collidedObjects.Add(gameObject.GetComponent<Move>().curvePosition, gameObject);
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        GameObject gameObject = collider.gameObject;
        if (gameObject.GetComponent<Move>() != null)
        {
            int index = collidedObjects.IndexOfValue(gameObject);
            collidedObjects.RemoveAt(index);
            collidedObjects.Add(gameObject.GetComponent<Move>().curvePosition, gameObject);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        GameObject gameObject = collider.gameObject;
        if (gameObject.GetComponent<Move>() != null)
        {
            int index = collidedObjects.IndexOfValue(gameObject);
            collidedObjects.RemoveAt(index);
        }
    }
}