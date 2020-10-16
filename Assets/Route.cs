using UnityEngine;

public class Route : MonoBehaviour
{
    [SerializeField]
    private Transform[] controlPoints;

    public int[] curveLengths;

    [Min(1)]
    public int numPositions;

    void Start()
    {

    }

    void OnDrawGizmos()
    {
        Bezier bezier = new Bezier(controlPoints, curveLengths);
        for (float t = 0; t <= 1; t += (1f / numPositions))
        {
            Gizmos.DrawSphere(bezier.GenPoint(t), 0.25f);
        }
    }

}