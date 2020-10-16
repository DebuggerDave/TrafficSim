using UnityEngine;

public class Route : MonoBehaviour
{
    [SerializeField]
    private Transform[] controlPoints;

    [Min(1)]
    public int numPositions;

    [HideInInspector]
    public Vector3[] points;

    void Start()
    {
        points = new Vector3[numPositions];
        int counter = 0;
        for (float t = 0; t <= 1; t += (1f / numPositions))
        {
            for (int i = 0; i < controlPoints.Length; i++)
            {
                points[counter] += BinomCoef(controlPoints.Length - 1, i) * Mathf.Pow(1 - t, controlPoints.Length - 1 - i) * Mathf.Pow(t, i) * controlPoints[i].position;
            }
            counter++;
        }
    }

    void OnDrawGizmos()
    {
        Start();
        for (int i = 0; i < numPositions; i++)
        {
            Gizmos.DrawSphere(points[i], 0.25f);
        }
    }

    private int BinomCoef(int total, int choose)
    {
        return Fact(total) / (Fact(choose) * Fact(total - choose));
    }

    private int Fact(int n)
    {
        int result = 1;
        for (int i = 1; i <= n; i++)
        {
            result *= i;
        }

        return result;
    }
}