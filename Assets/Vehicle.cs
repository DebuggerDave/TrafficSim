using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Vehicle : MonoBehaviour
{

    [SerializeField]
    private GameObject[] routes = new GameObject[0];

    [SerializeField]
    [Min(1)]
    private int numApproxPoints = 100;

    [SerializeField]
    [Min(1)]
    private float laneWidth = 3;

    [SerializeField]
    [Range(0, 1)]
    private float bezierParam = 0;

    [SerializeField]
    private int lane = 0;

    [SerializeField]
    private float speed = 0;

    [SerializeField]
    private float acceleration = 0;

    [SerializeField]
    private bool visualizeVision = false;

    public GameObject[] Routes { get => routes; }
    public int NumApproxPoints { get => numApproxPoints; }
    public float LaneWidth { get => laneWidth; }
    public float BezierParam { get => bezierParam; private set => bezierParam = value;  }
    public int Lane { get => lane; private set => lane = value; }
    public float Speed { get => speed; private set => speed = value; }
    public float Acceleration { get => acceleration; set => acceleration = value; }


    // Start is called before the first frame update
    private void Start()
    {
        if (Routes.Length == 0)
        {
            Debug.LogError("Vehicle requires a positive amount of routes");
        }
        else
        {
            Lane = Mathf.Clamp(Lane, 0, Routes.Length);
            if (GetCurrentBezier() != null)
            {
                transform.position = GetCurrentBezier().GenPoint(BezierParam);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Routes.Length == 0)
        {
            Debug.LogError("Vehicle requires a positive amount of routes");
        }
        else
        {
            Speed += Acceleration * Time.deltaTime;
            UpdateTransform();
        }
    }

    private Route GetRoute(int i)
    {
        return Routes[i].GetComponent<Route>();
    }

    private Route GetBezier(int i)
    {
        return GetRoute(i).GetComponent<Route>();
    }

    private Route GetCurrentRoute()
    {
        return Routes[Lane].GetComponent<Route>();
    }

    private Bezier GetCurrentBezier()
    {
        return GetCurrentRoute().bezier;
    }

    public float GetNextVehicleDist()
    {
        float step = 1f / (NumApproxPoints - 1);
        Vector3 currentPosition = transform.position;

        if (GetCurrentBezier() != null)
        {
            for (float t = (BezierParam + step); t <= (BezierParam + 1); t += step)
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
                        float nextBezierParam = hitVehicle.BezierParam;
                        if (nextBezierParam < BezierParam)
                        {
                            nextBezierParam++;
                        }

                        return GetCurrentBezier().ArcLengthApproximation(BezierParam, nextBezierParam, NumApproxPoints);
                    }
                }
            }
        }

        return float.PositiveInfinity;
    }

    public void MoveRight()
    {
        if (!IsRightLaneOpen())
        {
            Debug.Log("Can't turn right");
        }
        else
        {
            Lane = Mathf.Clamp(Lane - 1, 0, Routes.Length - 1);
        }
    }

    public bool IsRightLaneOpen()
    {
        Transform frontTransform = transform.Find("Front");
        Transform backTransform = transform.Find("Back");
        if ((frontTransform == null) || (backTransform == null))
        {
            Debug.LogError("Cannot Lane check, Vehicle must have \"Front\" and \"Back\" children");
            return false;
        }
        else
        {
            Vector3 direction = Vector3.Cross(-(frontTransform.position - transform.position), Vector3.up);
            if (
                Physics.Raycast(frontTransform.position, direction, LaneWidth, LayerMask.GetMask("Vehicle"), QueryTriggerInteraction.Collide) ||
                Physics.Raycast(backTransform.position, direction, LaneWidth, LayerMask.GetMask("Vehicle"), QueryTriggerInteraction.Collide) ||
                Physics.Raycast(transform.position, direction, LaneWidth, LayerMask.GetMask("Vehicle"), QueryTriggerInteraction.Collide)
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public void MoveLeft()
    {
        if (!IsLeftLaneOpen())
        {
            Debug.Log("Can't turn right");
        }
        else
        {
            Lane = Mathf.Clamp(Lane + 1, 0, Routes.Length - 1);
        }
    }

    public bool IsLeftLaneOpen()
    {
        Transform frontTransform = transform.Find("Front");
        Transform backTransform = transform.Find("Back");
        if ((frontTransform == null) || (backTransform == null))
        {
            Debug.LogError("Cannot Lane check, Vehicle must have \"Front\" and \"Back\" children");
            return false;
        }
        else
        {
            Vector3 direction = Vector3.Cross(frontTransform.position - transform.position, Vector3.up);
            if (
                Physics.Raycast(frontTransform.position, direction, LaneWidth, LayerMask.GetMask("Vehicle"), QueryTriggerInteraction.Collide) ||
                Physics.Raycast(backTransform.position, direction, LaneWidth, LayerMask.GetMask("Vehicle"), QueryTriggerInteraction.Collide) ||
                Physics.Raycast(transform.position, direction, LaneWidth, LayerMask.GetMask("Vehicle"), QueryTriggerInteraction.Collide)
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    private void UpdateTransform()
    {
        Vector3 lastPosition = transform.position;
        UpdatePosition();
        UpdateRotation(lastPosition);
    }

    private void UpdatePosition()
    {
        float distance = Time.deltaTime * Speed;
        if (GetCurrentBezier() != null)
        {
            BezierParam = GetCurrentBezier().GenDistanceT(BezierParam, distance, NumApproxPoints);
            Vector3 newPosition = GetCurrentBezier().GenPoint(BezierParam);
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

    private void OnDrawGizmos()
    {
        if (visualizeVision)
        {
            Transform frontTransform = transform.Find("Front");
            Transform backTransform = transform.Find("Back");

            if ((frontTransform != null) && (backTransform != null))
            {
                Vector3 direction = Vector3.Cross(frontTransform.position - transform.position, Vector3.up);

                // Left lane vision
                Debug.DrawLine(frontTransform.position, frontTransform.position + (direction * LaneWidth), Color.yellow);
                Debug.DrawLine(backTransform.position, backTransform.position + (direction * LaneWidth), Color.yellow);
                Debug.DrawLine(transform.position, transform.position + (direction * LaneWidth), Color.yellow);

                direction = Vector3.Cross(-(frontTransform.position - transform.position), Vector3.up);

                // Left lane vision
                Debug.DrawLine(frontTransform.position, frontTransform.position + (direction * LaneWidth), Color.yellow);
                Debug.DrawLine(backTransform.position, backTransform.position + (direction * LaneWidth), Color.yellow);
                Debug.DrawLine(transform.position, transform.position + (direction * LaneWidth), Color.yellow);
            }
        }
    }

}