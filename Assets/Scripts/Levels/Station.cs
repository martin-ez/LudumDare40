using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour
{
    public GameObject[] charPrefabs;

    public Material[] hairMats;
    public Material[] skinMats;
    public Material[] dressMats;

    public Transform startAnimation;
    public Transform endAnimation;

    public int numPassengers = 10;

    private bool active = false;

    public System.Action OnPickup;

    public void SetActive(bool a)
    {
        active = a;
    }

    void OnTriggerStay(Collider other)
    {
        if (active && other.gameObject.CompareTag("Bus"))
        {
            Bus b = other.gameObject.GetComponent<Bus>();
            if (b.GetState() == Bus.BusState.Drive && b.GetVelocity() < 0.005f)
            {
                if (OnPickup != null) OnPickup();
                b.EnterStation();
                StartCoroutine(PickupAnimation(b));
            }
        }
    }

    private IEnumerator PickupAnimation(Bus b)
    {
        endAnimation.position = new Vector3(b.transform.position.x, 2f, b.transform.position.z);
        for (int i = 0; i < numPassengers; i++)
        {
            GameObject pass = Instantiate(charPrefabs[Random.Range(0, charPrefabs.Length)]);
            Material hair = hairMats[Random.Range(0, hairMats.Length )];
            Material body = skinMats[Random.Range(0, skinMats.Length)];
            Material dress = dressMats[Random.Range(0, dressMats.Length)];
            foreach (Transform child in pass.transform)
            {
                Renderer rend = child.gameObject.GetComponent<Renderer>();
                if (child.name == "Torso")
                {
                    rend.material = dress;
                }
                else if (child.name == "Hair")
                {
                    rend.material = hair;
                }
                else if (child.name == "Head")
                {
                    rend.material = body;
                }
            }
            StartCoroutine(PassangerAnimation(pass, b));
            yield return new WaitForSeconds(0.075f);
        }
        yield return new WaitForSeconds(3f);
        b.ExitStation();
    }

    private IEnumerator PassangerAnimation(GameObject pass, Bus b)
    {
        pass.transform.position = startAnimation.position;
        pass.transform.eulerAngles = Vector3.zero;
        float time = 0;
        float t = 0;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 2.25f, time);
            pass.transform.position = Vector3.Lerp(startAnimation.position, endAnimation.position, t);
            yield return null;
        }
        Destroy(pass);
        b.AddPassenger();
    }
}
