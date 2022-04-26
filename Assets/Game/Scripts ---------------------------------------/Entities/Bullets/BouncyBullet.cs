using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyBullet : Bullet
{
    [GambaHeader("Bouncy Bullet -----------------------------------------------------------------------------------------------------------------------------------------------------------", 0.7f)]
    [SerializeField]
    private float maxBounces;
    [ReadOnly, SerializeField]
    private float bounces;

    CollisionInfo lastCollision;

    protected override void OnOtherCollisionEnter(Collision2D collision)
    {
        if (bounces >= maxBounces || kineticState == KineticState.Grabbed)
        {
            Crash();
        }
        else
        {
            bounces++;

            // Debug
            lastCollision.inputDir = direction;
            lastCollision.point = collision.GetContact(0).point;
            lastCollision.normal = collision.GetContact(0).normal;

            Bounce(collision);

            // Debug
            lastCollision.outputDir = direction;
        }
    }

    protected override void OnOtherCollisionStay(Collision2D collision)
    {
        if (kineticState == KineticState.Grabbed)
        {
            Crash();
        }
        else
        {
            Bounce(collision);
        }
    }

    private void Bounce(Collision2D collision)
    {
        direction = GambaFunctions.Bounce(collision.GetContact(0).normal, direction);

        UpdateVelocity();
    }

    public override void Grab()
    {
        base.Grab();

        bounces = 0;
    }

    private void OnDrawGizmos()
    {
        lastCollision.DebugCollision();
    }
}