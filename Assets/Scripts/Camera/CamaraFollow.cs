using UnityEngine;

public class CamaraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 0.125f;
    private Vector3 offset;
    private Vector3 velocity;

    void Start()
    {
        velocity = Vector3.one;
        offset = transform.position - target.position;
    }

    void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + (target.rotation * offset);
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target, target.up);
    }
}
