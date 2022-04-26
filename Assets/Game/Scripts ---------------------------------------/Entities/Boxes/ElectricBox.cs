using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBox : DynamicBox
{
    [GambaHeader("Electric Box ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [SerializeField]
    private SpriteRenderer boxCore;
    [SerializeField]
    private Transition electricTransition;

    protected override void AliveUpdate()
    {
        base.AliveUpdate();

        electricTransition.UpdateTransitionValue();

        if (electricTransition.IsOnTransition)
        {
            boxCore.color = new Color(boxCore.color.r, boxCore.color.g, boxCore.color.b, electricTransition.value);
            
        }
    }

    public void SetState(bool active)
    {
        if (active)
        {
            electricTransition.StartTransition(1);
        }
        else
        {
            electricTransition.StartTransition(0);
        }
    }
}