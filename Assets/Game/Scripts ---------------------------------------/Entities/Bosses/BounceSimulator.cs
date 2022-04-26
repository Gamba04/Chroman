using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceSimulator : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private float speed;

    private Vector2 direction;

    private void Update()
    {
        rb.velocity = direction.normalized * speed;
    }

    public void ResetProbe(Vector2 position)
    {
        transform.position = position;
        direction = Vector2.zero;
    }

    public void StartSimulation()
    {
        direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }

    public void StartSimulation(Vector2 direction)
    {
        this.direction = direction;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        direction = GambaFunctions.Bounce(collision.GetContact(0).normal, direction);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        direction = GambaFunctions.Bounce(collision.GetContact(0).normal, direction);
    }

}