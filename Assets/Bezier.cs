using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

// Represents curveLenths.Length Bezier curves connected together
// Uses numCurveControlPoints[i] control points from controlPoints to generate curve i
public class Bezier
{

    private Vector3[] controlPoints = null;
    private int[] curveSizes = null;

    // for floating point approximation
    private static float epsilon = .0001f;

    public Bezier(Vector3[] controlPoints, int[] numCurveControlPoints)
    {
        this.controlPoints = controlPoints;
        this.curveSizes = numCurveControlPoints;

        if (controlPoints.Length < 2)
        {
            throw new System.ArgumentException("ControlPoints must contain at least 2 points");
        }
        if (numCurveControlPoints.Sum() != controlPoints.Length)
        {
            throw new System.ArgumentException("Sum of numCurveControlPoints must equal total controlPoint");
        }
    }

    public Vector3 GenPoint(float t)
    {
        float tGlobal = Mathf.Abs(t - Mathf.Floor(t));
        int curveNum = 0;
        float aggPercent = 0;
        float percentWholeCurve = 0;

        // Find curve associated with global t value
        for (int i = 0; i < curveSizes.Length; i++)
        {
            percentWholeCurve = curveSizes[i] / (float)controlPoints.Length;
            aggPercent += percentWholeCurve;
            if (tGlobal <= aggPercent)
            {
                aggPercent -= percentWholeCurve;
                curveNum = i;
                break;
            }
        }

        float  tLocal = (tGlobal - aggPercent) / percentWholeCurve;
        return BezierFormula(curveNum, tLocal);
    }

    // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Explicit_definition
    private Vector3 BezierFormula(int curveNum, float t)
    {
        int controlPointStart = 0;
        for (int i = 0; i < curveNum; i++)
        {
            controlPointStart += curveSizes[i];
        }

        Vector3 point = new Vector3();
        int index = controlPointStart;
        for (int i = 0; i < curveSizes[curveNum]; i++)
        {
            point += BinomCoef(curveSizes[curveNum] - 1, i) * Mathf.Pow(1 - t, curveSizes[curveNum] - 1 - i) * Mathf.Pow(t, i) * controlPoints[index];
            index++;
        }

        return point;
    }

    // Generate numPoints to create a bezier curve
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

    // Get Bezier curve arc length between two t values
    // Uses numPoints to approximate curve
    public float ArcLengthApproximation(float tStart, float tStop, int numPoints)
    {
        float length = 0;
        Vector3 lastPoint;
        Vector3 currentPoint = GenPoint(tStart);

        float step = (tStop - tStart) / (Mathf.Max(numPoints - 1, 1));

        if (step < 0)
        {
            for (float t = (tStart + step); t >= tStop; t += step)
            {
                lastPoint = currentPoint;
                currentPoint = GenPoint(t);
                length -= Vector3.Distance(lastPoint, currentPoint);
            }
        }
        else if (step > 0)
        {
            for (float t = (tStart + step); t <= tStop; t += step)
            {
                lastPoint = currentPoint;
                currentPoint = GenPoint(t);
                length += Vector3.Distance(lastPoint, currentPoint);
            }
        }
        else
        {
            return 0;
        }

        return length;
    }

    // WARNING low epsilon values can cause infinite loops
    // Get t value distance meters from tStart
    // uses numPoints to approximate bezier curve
    public float GenDistanceT(float tStart, float distance, int numPoints)
    {
        // binary like search when overstepping distance
        bool overStep = false;

        float aggDistance = 0;
        float t = tStart;
        float step = 1f / (Mathf.Max(numPoints, 1));

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

                lastPoint = currentPoint;
                t += step;
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

        // return decimal only
        return (t - Mathf.Floor(t));
    }

    // Floating point equality approximation
    // True if floating points are within epsilon of each other
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

    // Binomial Coefficient
    private int BinomCoef(int total, int choose)
    {
        return Fact(total) / (Fact(choose) * Fact(total - choose));
    }

    // Factorial
    // Could be implemented with array storage for speedup?
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
