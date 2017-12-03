using UnityEngine;
using System.Collections;

public class TireToWheel : MonoBehaviour {

	public WheelCollider wheelCollider;
    ParticleSystem.EmissionModule particlesEmmiter;

	void Start() {
        particlesEmmiter = GetComponent<ParticleSystem>().emission;
        particlesEmmiter.rateOverTime = 500;
	}

	void FixedUpdate () {
	//	transform.position = wheelCollider.su

		UpdateWheelHeight(this.transform, wheelCollider);
	}


	void UpdateWheelHeight(Transform wheelTransform, WheelCollider collider) {
		
		Vector3 localPosition = wheelTransform.localPosition;
		
		WheelHit hit = new WheelHit();
		
		// see if we have contact with ground
		
		if (collider.GetGroundHit(out hit)) {

			float hitY = collider.transform.InverseTransformPoint(hit.point).y;

			localPosition.y = hitY + collider.radius;

			//wheelCollider.GetComponent<ParticleSystem>().enableEmission = true;
			if(
					Mathf.Abs(hit.forwardSlip) >= wheelCollider.forwardFriction.extremumSlip || 
					Mathf.Abs(hit.sidewaysSlip) >= wheelCollider.sidewaysFriction.extremumSlip
				) {
                particlesEmmiter.enabled = true;
			}
			else {
                particlesEmmiter.enabled = false;
			}


		} else {
			
			// no contact with ground, just extend wheel position with suspension distance
			
			localPosition = Vector3.Lerp (localPosition, -Vector3.up * collider.suspensionDistance, .05f);
            particlesEmmiter.enabled = false;

		}
		
		// actually update the position
		
		wheelTransform.localPosition = localPosition;

		wheelTransform.localRotation = Quaternion.Euler(0, collider.steerAngle, 90);
		
	}


}
