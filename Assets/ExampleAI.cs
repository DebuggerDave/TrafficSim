using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleAI : MonoBehaviour
{

    private Vehicle vehicle = null;

    private float secondTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        vehicle = gameObject.GetComponent<Vehicle>();
    }

    // Update is called once per frame
    void Update()
    {
        secondTimer += Time.deltaTime;
        if (secondTimer >= 1)
        {
            secondTimer--;

            if (vehicle.Speed >= 25)
            {
                vehicle.Acceleration = 0;
            }
            else
            {
                vehicle.Acceleration = vehicle.Acceleration + 1;
            }

            if (vehicle.GetNextVehicleDist() != float.PositiveInfinity)
            {
                if ((vehicle.Lane == 1) && vehicle.IsRightLaneOpen())
                {
                    vehicle.MoveRight();
                }
                else if ((vehicle.Lane == 0) && vehicle.IsLeftLaneOpen())
                {
                    vehicle.MoveLeft();
                }
            }
        }
    }
}
