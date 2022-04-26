using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawBullet : BouncyBullet
{
    [GambaHeader("Saw Bullet -----------------------------------------------------------------------------------------------------------------------------------------------------------", 0.7f)]
    [ReadOnly, SerializeField]
    private float sawSpeed;

    private int rotDir;

    public override void SetUp(Vector2 dir, float damage = -1, Vector2 momentum = new Vector2())
    {
        base.SetUp(dir, damage, momentum);

        int randomDir = Random.Range(0, 2);
        switch (randomDir)
        {
            case 0:
                rotDir = -1;
                break;
            case 1:
                rotDir = 1;
                break;
        }
    }

    protected override void AliveUpdate()
    {
        skin.sr.transform.Rotate(0, 0, Time.deltaTime * sawSpeed * rotDir);

        if (!GameManager.IsBossAwakened())
        {
            Die();
        }
    }
}