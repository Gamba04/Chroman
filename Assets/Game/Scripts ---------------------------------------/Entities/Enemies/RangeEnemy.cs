using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : Enemy
{
    [GambaHeader("Range Enemy ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private GameObject cannon;

    protected override void Hit()
    {
        GameObject bullet;
        Vector2 direction = (player.transform.position - transform.position).normalized;
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.RangeEnemy, transform.position);

        if (GameManager.ParentBullets != null)
        {
            bullet = Instantiate(bulletPrefab, transform.position + (Vector3)direction.normalized * 0.2f, transform.rotation, GameManager.ParentBullets);
        }
        else
        {
            bullet = Instantiate(bulletPrefab, transform.position + (Vector3)direction.normalized * 0.2f, transform.rotation);
        }

        bullet.GetComponent<Bullet>().SetUp(direction, damage);
    }

    protected override void Die()
    {
        base.Die();

        cannon.SetActive(false);
    }

}
