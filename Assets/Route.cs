using UnityEngine;

public class Route : MonoBehaviour
{
   
    [SerializeField]
    private int[] curveLengths = null;

    [SerializeField]
    [Min(1)]
    private int numDebugPoints = 0;

    private Transform[] controlPoints = null;

    public Bezier bezier = null;

    void Start()
    {
        SetControlPoints();
        bezier = new Bezier(controlPoints, curveLengths);
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

}