using UnityEngine;

public class WheelDrift : MonoBehaviour
{

    private WheelCollider wheel;
    public Rigidbody bus;
    public Transform smoke;
    public float speedFactor = 1f;

    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.VelocityOverLifetimeModule velocity;

    void Start ()
    {
        wheel = GetComponent<WheelCollider>();
        ParticleSystem ps = smoke.gameObject.GetComponent<ParticleSystem>();
        emission = ps.emission;
        velocity = ps.velocityOverLifetime;
	}

	void FixedUpdate ()
    {
        WheelHit hit = new WheelHit();
        if (wheel.GetGroundHit(out hit))
        {
            float hitY = wheel.transform.InverseTransformPoint(hit.point).y;
            float speed = bus.velocity.sqrMagnitude;
            Vector3 direction = bus.velocity.normalized;          
            if (Mathf.Abs(hit.forwardSlip) >= wheel.forwardFriction.extremumSlip || Mathf.Abs(hit.sidewaysSlip) >= wheel.sidewaysFriction.extremumSlip)
            {
                emission.rateOverTime = 20 + (50 * Random.value);
                velocity.xMultiplier = - direction.x * speed * speedFactor;
                velocity.zMultiplier = - direction.z * speed * speedFactor;
            }
            else
            {
                emission.rateOverTime = 0;
            }
        }
    }
}
