using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Bezier
{

    private Transform[] controlPoints = null;
    private int[] curveLengths = null;
    private float[] percentCurve = null;

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

            if (tGlobal <= totalPercent)
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

    public Vector3[] GenAllPoints(int numPoints)
    {
        Vector3[] curvePoints = new Vector3[numPoints];
        float step = 1f / (numPoints - 1);

        int counter = 0;
        for (float t = 0; t <= 1; t += step)
        {
            curvePoints[counter] = GenPoint(t);
            counter++;
        }

        return curvePoints;
    }

    public float ArcLengthApproximation(float tStart, float tStop, int numPoints)
    {
        tStop = Mathf.Clamp(tStop, 0, 1);
        float length = 0;
        Vector3 lastPoint;
        Vector3 currentPoint = GenPoint(tStart);
        float step = 1f / (numPoints - 1);

        if ((tStop < tStart) && (step < tStop))
        {
            int numPointsRecursive = (int)Mathf.Floor(tStop / step);
            length += ArcLengthApproximation(0, tStop, numPointsRecursive);
            tStop = 1;
        }

        for (float t = (tStart + step); t <= tStop; t += step)
        {
            lastPoint = currentPoint;
            currentPoint = GenPoint(t);
            lastPoint = currentPoint;
            length += Vector3.Distance(lastPoint, currentPoint);
        }
        length += Vector3.Distance(currentPoint, GenPoint(tStop));

        return length;
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
