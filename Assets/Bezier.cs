using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Bezier
{

    private Transform[] controlPoints;
    private int[] curveLengths;
    private float[] percentCurve;

    public Bezier(Transform[] controlPoints, int[] curveLengths)
    {
        this.controlPoints = controlPoints;
        this.curveLengths = curveLengths;
        this.percentCurve = new float[curveLengths.Length];

        int total = curveLengths.Sum();
        if (total != controlPoints.Length)
        {
            throw new System.Exception("Sum of curveLengths must equal total controlPoints");
        }

        for (int i = 0; i < curveLengths.Length; i++)
        {
            percentCurve[i] = curveLengths[i] / (float)total;
        }
    }

    public Vector3 GenPoint(float t)
    {
        if (curveLengths.Length == 0)
        {
            return new Vector3();
        }
        
        float tGlobal = Mathf.Clamp(t, 0, 1);
        float tLocal = 0;
        int curveNum = 0;
        float totalPercent = 0;
        int controlPointStart = 0;

        for (int i = 0; i < curveLengths.Length; i++)
        {
            totalPercent += percentCurve[i];

            if (tGlobal < totalPercent)
            {
                curveNum = i;
                tLocal = (tGlobal - (totalPercent - percentCurve[i])) / percentCurve[i];
                break;
            }

            controlPointStart += curveLengths[i];
        }

        Vector3 point = new Vector3();
        for (int i = controlPointStart; i < (controlPointStart + curveLengths[curveNum]); i++)
        {
            int iLocal = i - controlPointStart;
            point += BinomCoef(curveLengths[curveNum] - 1, iLocal) * Mathf.Pow(1 - tLocal, curveLengths[curveNum] - 1 - iLocal) * Mathf.Pow(tLocal, iLocal) * controlPoints[i].position;
        }

        return point;
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
