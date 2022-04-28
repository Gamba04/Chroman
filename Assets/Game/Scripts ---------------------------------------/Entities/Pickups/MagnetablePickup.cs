using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetablePickup : Pickup, IMagnetable
{
    [Header("Magnetable Pickup")]
    [SerializeField]
    private Rigidbody2D rb;

    [Space]
    [SerializeField]
    private float attractionForce;

    public Rigidbody2D mRigidBody => rb;

    public float mAttractionForce => attractionForce;
}