using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{

    [SerializeField]
    private int[] curveSizes = new int[0];

    [SerializeField]
    [Min(1)]
    private int numDebugPoints = 25;

    private Vector3[] controlPoints = null;

    public Bezier bezier = null;

    void Start()
    {
        SetControlPoints();
        bezier = new Bezier(controlPoints, curveSizes);
    }

    private void OnDrawGizmos()
    {
        SetControlPoints();
        if (curveSizes.Length > 0)
        {
            Bezier debugBezier = new Bezier(controlPoints, curveSizes);
            for (float t = 0; t <= 1; t += (1f / numDebugPoints))
            {
                Gizmos.DrawSphere(debugBezier.GenPoint(t), 0.25f);
            }
        }
    }

    private void SetControlPoints()
    {
        int numControlPoints = transform.childCount;
        controlPoints = new Vector3[numControlPoints];
        for (int i = 0; i < numControlPoints; i++)
        {
            controlPoints[i] = transform.GetChild(i).position;
        }
    }

}