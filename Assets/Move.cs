using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    private float speed = 1;

    [SerializeField]
    private GameObject[] routeObjects = null;

    [SerializeField]
    private int lane = 0;

    private Route[] routes = null;

    public float nextObjectDistance = float.PositiveInfinity;

    [Range(0, 1)]
    public float curvePosition = 0;

    [Min(1)]
    public int numApproximationPoints = 100;

    // Start is called before the first frame update
    private void Start()
    {
        routes = new Route[routeObjects.Length];
        lane = Mathf.Clamp(lane, 0, routes.Length);

        for (int i = 0; i < routeObjects.Length; i++)
        {
            routes[i] = routeObjects[i].GetComponent<Route>();
        }

        transform.position = routes[lane].bezier.GenPoint(curvePosition);
    }

    // Update is called once per frame
    private void Update()
    {
        if (routes != null)
        {
            UpdateTransform();
            GetNextObjectDistance();
        }
    }

    private void GetNextObjectDistance()
    {
        SortedList<float, GameObject> routeCollisions = routes[lane].collidedObjects;
        if ((routeCollisions.ContainsValue(gameObject)) & (routeCollisions.Count > 1))
        {
            int nextObjectIndex = (routeCollisions.IndexOfValue(gameObject) + 1) % routeCollisions.Count;
            float nextObjectPosition = routeCollisions.Values[nextObjectIndex].GetComponent<Move>().curvePosition;
            if (nextObjectPosition < curvePosition)
            {
                nextObjectPosition++;
            }
            nextObjectDistance = routes[lane].bezier.ArcLengthApproximation(curvePosition, nextObjectPosition, numApproximationPoints);
        }
        else
        {
            nextObjectDistance = float.PositiveInfinity;
        }
    }

    public void Right()
    {
        lane = Mathf.Clamp(lane + 1, 0, routes.Length - 1);
    }

    public void Left()
    {
        lane = Mathf.Clamp(lane - 1, 0, routes.Length - 1);
    }

    private void UpdateTransform()
    {
        Vector3 currentPosition = transform.position;
        Vector3 currentDirection = transform.Find("Front").position - currentPosition;

        float distance = Time.deltaTime * speed;
        curvePosition = routes[lane].bezier.GenDistanceT(curvePosition, distance, numApproximationPoints);
        Vector3 newPosition = routes[lane].bezier.GenPoint(curvePosition);
        transform.position = newPosition;

        Vector3 newDirection = newPosition - currentPosition;
        float movementAngle = Vector3.SignedAngle(currentDirection, newDirection, Vector3.up);
        transform.Rotate(new Vector3(0, movementAngle, 0), Space.World);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "vehicle")
        {
            Debug.Log(gameObject.name +  " AND " + collider.gameObject.name + " COLLIDED");
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        //Debug.Log("HEY");
    }

    private void OnTriggerExit(Collider collider)
    {
        //Debug.Log("HEY");
    }

}