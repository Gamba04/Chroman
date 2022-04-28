using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMagnetable
{
    Rigidbody2D mRigidBody { get; }

    float mAttractionForce { get; }
}