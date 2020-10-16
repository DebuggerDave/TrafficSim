using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    [Min(1)]
    public float speed;

    [SerializeField]
    [Min(0)]
    private int curveIndex;

    [SerializeField]
    private GameObject[] routes;

    private Vector3[] curve;

    private float leftoverTime = 0;


    // Start is called before the first frame update
    void Start()
    {
        int totalPoints = 0;
        Vector3[][] curves = new Vector3[routes.Length][];

        for (int i = 0; i < routes.Length; i++)
        {
            curves[i] = routes[i].GetComponent<Route>().points;
            totalPoints += curves[i].Length;
        }

        curve = new Vector3[totalPoints];

        int counter = 0;
        for (int i = 0; i < routes.Length; i++)
        {
            for (int j = 0; j < curves[i].Length; j++)
            {
                curve[counter] = curves[i][j];
                counter++;
            }
        }

        curveIndex = curveIndex % curve.Length;
        transform.position = curve[curveIndex];
    }

    // Update is called once per frame
    void Update()
    {
        float advancement = (Time.deltaTime + leftoverTime) * speed;
        leftoverTime = (advancement - Mathf.Floor(advancement)) / speed;

        curveIndex = (curveIndex + (int)Mathf.Floor(advancement)) % curve.Length;
        transform.position = curve[curveIndex];
    }
}
