using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KineticState
{
    None,
    Grabbed,
    Thrown
}

public interface IKinetic
{
    KineticState kineticState { get; set; }

    bool isDead { get; }

    Rigidbody2D kRigidbody { get; }

    Transform kTransform { get; }

    Vector2 kineticCloudScale { get; }

    Vector2 kineticCloudOffset { get; }

    float kineticMomentumMagnitude { get; }

    bool lookAtPlayer { get; }

    bool rotateCloud { get; }

    void Unleash();

    void Grab();

    void Throw(Vector2 direction, float force);

}