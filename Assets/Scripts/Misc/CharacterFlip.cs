using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFlip : MonoBehaviour
{
    public void Dance()
    {
        StartCoroutine(Move(transform.position.y + (Random.value * -0.2f), transform.position.y + (Random.value * 0.2f), 0.5f + (Random.value * 0.2f)));
    }

    private IEnumerator Move(float startPos, float endPos, float time)
    {
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i <= 1.0)
        {
            i += Time.deltaTime * rate;
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(startPos, endPos, i), transform.position.z);
            yield return null;
        }
        StartCoroutine(Move(endPos, startPos, 0.5f + (Random.value * 0.2f)));
    }
}
