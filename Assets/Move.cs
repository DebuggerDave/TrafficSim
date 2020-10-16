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
    private GameObject[] routes = null;

    [SerializeField]
    private int lane = 0;

    private Bezier[] beziers = null;

    // Start is called before the first frame update
    private void Start()
    {
        beziers = new Bezier[routes.Length];
        lane = Mathf.Clamp(lane, 0, routes.Length);

        for (int i = 0; i < routes.Length; i++)
        {
            beziers[i] = routes[i].GetComponent<Route>().bezier;
        }

        transform.position = beziers[lane].GenPoint(curvePosition);
    }

    // Update is called once per frame
    private void Update()
    {
        float updatedPosition = curvePosition + (Time.deltaTime * speed);
        curvePosition = updatedPosition - Mathf.Floor(updatedPosition);
        transform.position = beziers[lane].GenPoint(curvePosition);
    }

    public void Right()
    {
        lane = Mathf.Clamp(lane + 1, 0, beziers.Length - 1);
    }

    public void Left()
    {
        lane = Mathf.Clamp(lane - 1, 0, beziers.Length - 1);
    }
}