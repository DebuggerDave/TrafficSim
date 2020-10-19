using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    // m/s
    private float speed = 10;

    [SerializeField]
    private GameObject[] routes = new GameObject[0];

    [SerializeField]
    private int lane = 0;

    [SerializeField]
    private float nextVehicleDist = float.PositiveInfinity;

    [SerializeField]
    [Min(1)]
    private int numApproxPoints = 100;

    [Range(0, 1)]
    public float bezierParam = 0;

    // Start is called before the first frame update
    private void Start()
    {
        if (routes.Length == 0)
        {
            Debug.LogError("Vehicle requires a positive amount of routes");
        }
        else
        {
            lane = Mathf.Clamp(lane, 0, routes.Length);
            if (GetCurrentBezier() != null)
            {
                transform.position = GetCurrentBezier().GenPoint(bezierParam);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (routes.Length == 0)
        {
            Debug.LogError("Vehicle requires a positive amount of routes");
        }
        else
        {
            UpdateTransform();
            UpdateNextVehicleDist();
        }
    }

    private Route GetRoute(int i)
    {
        return routes[i].GetComponent<Route>();
    }

    private Route GetBezier(int i)
    {
        return GetRoute(i).GetComponent<Route>();
    }

    private Route GetCurrentRoute()
    {
        return routes[lane].GetComponent<Route>();
    }

    private Bezier GetCurrentBezier()
    {
        return GetCurrentRoute().bezier;
    }

    private void UpdateNextVehicleDist()
    {
        float step = 1f / (numApproxPoints - 1);
        Vector3 currentPosition = transform.position;

        if (GetCurrentBezier() != null)
        {
            for (float t = (bezierParam + step); t <= (bezierParam + 1); t += step)
            {
                Vector3 lastPosition = currentPosition;
                currentPosition = GetCurrentBezier().GenPoint(t);
                Vector3 direction = currentPosition - lastPosition;
                float distance = Vector3.Distance(lastPosition, currentPosition);

                Ray ray = new Ray(lastPosition, direction);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, distance))
                {
                    Vehicle hitVehicle = hit.collider.gameObject.GetComponent<Vehicle>();
                    if (hitVehicle == null)
                    {
                        Debug.LogError("Cannot calculate distance, Offending object has no \"Vehicle\" component");
                    }
                    else
                    {
                        float nextBezierParam = hitVehicle.bezierParam;
                        if (nextBezierParam < bezierParam)
                        {
                            nextBezierParam++;
                        }

                        nextVehicleDist = GetCurrentBezier().ArcLengthApproximation(bezierParam, nextBezierParam, numApproxPoints);

                        return;
                    }
                }
            }
        }

        nextVehicleDist = float.PositiveInfinity;
    }

    public void MoveRight()
    {
        lane = Mathf.Clamp(lane + 1, 0, routes.Length - 1);
    }

    public void MoveLeft()
    {
        lane = Mathf.Clamp(lane - 1, 0, routes.Length - 1);
    }

    private void UpdateTransform()
    {
        Vector3 lastPosition = transform.position;
        UpdatePosition();
        UpdateRotation(lastPosition);
    }

    private void UpdatePosition()
    {
        float distance = Time.deltaTime * speed;
        if (GetCurrentBezier() != null)
        {
            bezierParam = GetCurrentBezier().GenDistanceT(bezierParam, distance, numApproxPoints);
            Vector3 newPosition = GetCurrentBezier().GenPoint(bezierParam);
            transform.position = newPosition;
        }
    }

    private void UpdateRotation(Vector3 lastPosition)
    {
        if (transform.Find("Front") == null)
        {
            Debug.LogError("Vehicle must have \"Front\" child to update transform rotation");
        }
        else
        {
            Vector3 currentPosition = transform.position;
            Vector3 currentDirection = transform.Find("Front").position - currentPosition;

            Vector3 newDirection = currentPosition - lastPosition;
            float movementAngle = Vector3.SignedAngle(currentDirection, newDirection, Vector3.up);
            transform.Rotate(new Vector3(0, movementAngle, 0), Space.World);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log(gameObject.name +  " AND " + collider.gameObject.name + " COLLIDED");
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