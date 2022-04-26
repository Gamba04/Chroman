using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Donchim2 : MonoBehaviour
{
    [SerializeField]
    private float speed;

    Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        Movement();
    }

    private void Movement()
    {
        Vector2 dir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        rb.velocity += dir * 100;

        if (rb.velocity.magnitude > speed)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }

        if (dir.magnitude < 0.1f)
        {
            rb.velocity = rb.velocity * (1 - Time.deltaTime - 0.1f);
        }
    }
}
