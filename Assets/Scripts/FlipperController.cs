using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class FlipperController : MonoBehaviour
{
    public HingeJoint hinge;
    public KeyCode flipperKey = KeyCode.Z;
    public float motorStrength = 800f;
    public float activeSpeed = 1000f;
    public float restSpeed = -200f;

    void Start()
    {
        if (hinge == null) hinge = GetComponent<HingeJoint>();
        // ensure motor exists
        JointMotor motor = hinge.motor;
        motor.force = motorStrength;
        motor.targetVelocity = restSpeed;
        hinge.motor = motor;
        hinge.useMotor = true;
    }

    void Update()
    {
        JointMotor motor = hinge.motor;
        motor.force = motorStrength;

        if (Input.GetKey(flipperKey))
        {
            motor.targetVelocity = activeSpeed;
        }
        else
        {
            motor.targetVelocity = restSpeed;
        }

        hinge.motor = motor;
    }
}