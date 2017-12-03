using UnityEngine;
using System.Collections;

public class RearWheelDrive : MonoBehaviour
{

    private WheelCollider[] wheels;

    public float maxAngle = 30;
    public float maxTorque = 300;
    public float brakeTorque = 300;
    public float gasForce = 900;
    public float maxVelocity = 900;
    public float antiRoll;
    public float windResistance;
    public GameObject wheelShape;

    private Rigidbody rb;

    private WheelCollider fr;
    private WheelCollider fl;
    private WheelCollider rr;
    private WheelCollider rl;

    // here we find all the WheelColliders down in the hierarchy
    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<WheelCollider>();

        for (int i = 0; i < wheels.Length; ++i)
        {
            var wheel = wheels[i];

            if (wheel.transform.localPosition.z > 0)
            {
                if (wheel.transform.localPosition.x > 0)
                {
                    fr = wheel;
                }
                else
                {
                    fl = wheel;
                }
            }
            else
            {
                if (wheel.transform.localPosition.x > 0)
                {
                    rr = wheel;
                }
                else
                {
                    rl = wheel;
                }
            }

            // create wheel shapes only when needed
            if (wheelShape != null)
            {
                var ws = GameObject.Instantiate(wheelShape);
                ws.transform.parent = wheel.transform;
            }
        }
    }

    // this is a really simple approach to updating wheels
    // here we simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero
    // this helps us to figure our which wheels are front ones and which are rear
    public void Update()
    {
        float angle = maxAngle * Input.GetAxis("Horizontal");
        float torque = maxTorque * Input.GetAxis("Vertical");

        DoRollBar(fr, fl);
        DoRollBar(rr, rl);

        fr.steerAngle = angle;
        fl.steerAngle = angle;
        rr.steerAngle = -angle/10f;
        rl.steerAngle = -angle/10f;

        rr.motorTorque = torque;
        rl.motorTorque = torque;
        fr.motorTorque = torque;
        fl.motorTorque = torque;

        foreach (WheelCollider wheel in wheels)
        {
            // update visual wheels if any
            if (wheelShape)
            {
                Quaternion q;
                Vector3 p;
                wheel.GetWorldPose(out p, out q);

                // assume that the only child of the wheelcollider is the wheel shape
                Transform shapeTransform = wheel.transform.GetChild(0);
                shapeTransform.position = p;
                shapeTransform.rotation = q;
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            rr.brakeTorque = brakeTorque;
            rl.brakeTorque = brakeTorque;
            fr.brakeTorque = brakeTorque;
            fl.brakeTorque = brakeTorque;
        }
        else
        {
            rr.brakeTorque = 0;
            rl.brakeTorque = 0;
            fr.brakeTorque = 0;
            fl.brakeTorque = 0;
        }

        Debug.Log("Velocity: " + rb.velocity.sqrMagnitude);
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.LeftShift) && rb.velocity.sqrMagnitude < maxVelocity && angle == 0)
        {
            rb.AddForce(transform.forward * gasForce, ForceMode.Acceleration);
        }
        if (rb.velocity.sqrMagnitude > maxVelocity)
        {
            Vector3 direction = - rb.velocity.normalized;
            rb.AddForce(direction * (rb.velocity.sqrMagnitude - maxVelocity) * windResistance);
        }
    }

    void DoRollBar(WheelCollider WheelL, WheelCollider WheelR)
    {
        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = WheelL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;

        bool groundedR = WheelR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL)
            rb.AddForceAtPosition(WheelL.transform.up * -antiRollForce, WheelL.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(WheelR.transform.up * antiRollForce, WheelR.transform.position);
    }
}
