using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    private float speed;

    [SerializeField]
    [Range(0, 1)]
    private float curvePosition;

    [SerializeField]
    private GameObject route;

    private Bezier bezier;

    private Transform[] controlPoints;

    private int[] curveLengths;


    // Start is called before the first frame update
    void Start()
    {
        int numControlPoints = route.transform.childCount;
        controlPoints = new Transform[numControlPoints];
        curveLengths = route.GetComponent<Route>().curveLengths;
        for (int i = 0; i < controlPoints.Length; i++)
        {
            controlPoints[i] = route.GetComponent<Route>().transform.GetChild(i);
        }
        bezier = new Bezier(controlPoints, curveLengths);
    }

    // Update is called once per frame
    void Update()
    {
        float updatedPosition = curvePosition + (Time.deltaTime * speed / 10000);
        curvePosition = updatedPosition - Mathf.Floor(updatedPosition);
        transform.position = bezier.GenPoint(curvePosition);
    }
}