using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour
{
    [Header("Pickup Zone")]
    public Transform[] zoneBars;

    [Header("Passangers")]
    public GameObject[] charPrefabs;

    public Material[] hairMats;
    public Material[] skinMats;
    public Material[] dressMats;

    public Transform startAnimation;
    public Transform endAnimation;

    public int minPassangers = 25;
    public int maxPassangers = 75;

    private bool active = false;

    public System.Action OnPickup;

    public void SetActive(bool a)
    {
        active = a;
        if (active)
        {
            foreach (Transform bar in zoneBars)
            {
                bar.gameObject.SetActive(true);
                bar.localScale = Vector3.one;
            }
            StartCoroutine(ZoneAnimation());
        }
        else
        {
            StopAllCoroutines();
            foreach (Transform bar in zoneBars)
            {
                bar.gameObject.SetActive(false);
            }
        }
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
        int numPassengers = Random.Range(minPassangers, maxPassangers + 1);
        endAnimation.position = new Vector3(b.transform.position.x, 2f, b.transform.position.z);
        for (int i = 0; i < numPassengers; i++)
        {
            GameObject pass = Instantiate(charPrefabs[Random.Range(0, charPrefabs.Length)]);
            Material hair = hairMats[Random.Range(0, hairMats.Length)];
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

    private IEnumerator ZoneAnimation()
    {
        while (true)
        {
            float start = 1f;
            float end = 0.8f;
            for (int i = 0; i < 4; i++)
            {
                float time = 0;
                float t = 0;
                while (t < 1)
                {
                    time += Time.deltaTime;
                    t = Mathf.InverseLerp(0, 0.25f, time);
                    float pos = Mathf.Lerp(start, end, t);
                    for (int j = 0; j < zoneBars.Length; j++)
                    {
                        if (j > i)
                        {
                            zoneBars[j].localScale = new Vector3(pos, 1, pos);
                        }
                    }
                    yield return null;
                }
                for (int j = 0; j < zoneBars.Length; j++)
                {
                    if (j > i)
                    {
                        zoneBars[j].localScale = Vector3.one * end;
                    }
                }
                start -= 0.2f;
                end -= 0.2f;
            }
            yield return new WaitForSeconds(1);
            foreach (Transform bar in zoneBars)
            {
                bar.localScale = Vector3.one;
            }
        }
    }
}
