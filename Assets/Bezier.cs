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
    private float epsilon = .0001f;

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

        float tGlobal = Mathf.Abs(t - Mathf.Floor(t));
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
        tStart = Mathf.Clamp(tStart, 0, 1);
        tStop = Mathf.Clamp(tStop, 0, 1);
        if (tStart == tStop)
        {
            return 0;
        }
        else if (tStop < tStart)
        {
            tStop++;
        }
        float length = 0;

        Vector3 lastPoint;
        Vector3 currentPoint = GenPoint(tStart);
        float step = (tStop - tStart) / (Mathf.Max(numPoints - 1, 1));
        for (float t = (tStart + step); t <= tStop; t += step)
        {
            lastPoint = currentPoint;
            currentPoint = GenPoint(t);
            length += Vector3.Distance(lastPoint, currentPoint);
        }

        return length;
    }

    public float GenDistanceT(float tStart, float distance, int numPoints)
    {
        float aggDistance = 0;
        float t = tStart;
        float step = 1f / (Mathf.Max(numPoints, 1));
        bool overStep = false;
        Vector3 lastPoint;
        Vector3 currentPoint = GenPoint(t);

        while (!isApproxEqual(aggDistance, distance, epsilon))
        {
            if (aggDistance < distance)
            {
                if (overStep)
                {
                    step /= 2;
                    overStep = false;
                }
                t += step;
                lastPoint = currentPoint;
                currentPoint = GenPoint(t);
                aggDistance += Vector3.Distance(lastPoint, currentPoint);
            }
            if (aggDistance > distance)
            {
                if (!overStep)
                {
                    step /= 2;
                    overStep = true;
                }
                t -= step;
                lastPoint = GenPoint(t);
                aggDistance -= Vector3.Distance(lastPoint, currentPoint);
                currentPoint = lastPoint;
            }
        }

        return t - Mathf.Floor(t);
    }

    bool isApproxEqual(float a, float b, float epsilon)
    {
        if (a >= b - epsilon && a <= b + epsilon)
        {
            return true;
        }
        else
        {
            return false;
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
