using System.Collections;
using UnityEngine;

public class ExplosionRepeater : MonoBehaviour
{
    [SerializeField]
    private Explosion explosion;
    [SerializeField]
    private float interval;

    private void Start()
    {
        StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            explosion.Play();
        }
    }
}