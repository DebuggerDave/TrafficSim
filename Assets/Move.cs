using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    private float speed = 1;

    [SerializeField]
    [Range(0, 1)]
    private float curvePosition = 0;

    [SerializeField]
    private GameObject route = null;

    private Bezier bezier = null;

    // Start is called before the first frame update
    void Start()
    {
        bezier = route.GetComponent<Route>().bezier;
    }

    // Update is called once per frame
    void Update()
    {
        float updatedPosition = curvePosition + (Time.deltaTime * speed);
        curvePosition = updatedPosition - Mathf.Floor(updatedPosition);
        transform.position = bezier.GenPoint(curvePosition);
    }
}