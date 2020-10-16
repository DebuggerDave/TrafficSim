using UnityEngine;

public class Route : MonoBehaviour
{
   
    [SerializeField]
    private int[] curveLengths = null;

    [SerializeField]
    [Min(1)]
    private int numPositions = 0;

    private Transform[] controlPoints = null;

    public Bezier bezier = null;

    void Start()
    {
        int numControlPoints = transform.childCount;
        controlPoints = new Transform[numControlPoints];
        for (int i = 0; i < controlPoints.Length; i++)
        {
            controlPoints[i] = transform.GetChild(i);
        }

        bezier = new Bezier(controlPoints, curveLengths);
    }

    void OnDrawGizmos()
    {
        Start();
        for (float t = 0; t <= 1; t += (1f / numPositions))
        {
            Gizmos.DrawSphere(bezier.GenPoint(t), 0.25f);
        }
    }

}