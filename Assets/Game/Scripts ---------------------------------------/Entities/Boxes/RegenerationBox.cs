using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenerationBox : DynamicBox
{
    [GambaHeader("Regeneration Box------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [SerializeField]
    private int regenerationAmount = 1;
    protected override void Die()
    {
        base.Die();

        GameManager.Player.AddHealth(regenerationAmount);
    }
}