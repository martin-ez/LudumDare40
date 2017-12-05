using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{
    public Transform[] wayPoints;
    public GameObject explosion;

    public void SetWayPoints(Transform[] wp)
    {
        wayPoints = wp;
        StartCoroutine(Move());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bus"))
        {
            StopAllCoroutines();
            other.gameObject.GetComponent<Bus>().TakeDamage(0.075f);
            GameObject exp = Instantiate(explosion);
            exp.transform.position = transform.position;
            Destroy(exp, 1);
            Destroy(gameObject);
        }
    }

    public IEnumerator Move()
    {
        for(int i = 0; i<wayPoints.Length; i++)
        {
            Vector3 start = transform.position;
            Vector3 end = wayPoints[i].position;
            transform.LookAt(end);

            float time = 0;
            float t = 0;
            while (t<1)
            {
                time += Time.deltaTime;
                t = Mathf.InverseLerp(0, 6f, time);
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            transform.position = end;
        }

        Destroy(gameObject);          
    }
}
