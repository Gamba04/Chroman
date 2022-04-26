using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayLaser : Laser
{
    [GambaHeader("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [GambaHeader("Ray Laser", 0, 0, 0, 0.4f)]
    [ReadOnly, SerializeField]
    protected Vector2 origin;
    [ReadOnly, SerializeField]
    protected Vector2 direction = new Vector2(1,0);
    [ReadOnly, SerializeField]
    protected float maxDistance = 100;
    [SerializeField]
    private List<int> worldCollisionLayers = new List<int>();
    [SerializeField]
    private Transform hitPoint;

    public override void UpdatePositions()
    {
        posA = origin;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance);

        bool checkHit = false;

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].collider.isTrigger)
            {
                int hitLayer = hits[i].collider.gameObject.layer;

                bool checkLayer = false;

                for (int l = 0; l < worldCollisionLayers.Count; l++)
                {
                    if (worldCollisionLayers[l] == hitLayer)
                    {
                        checkLayer = true;
                        break;
                    }
                }

                if (checkLayer)
                {
                    posB = hits[i].point;
                    checkHit = true;
                    OnWorldCollision(hits[i].point, hitLayer);
                    break;
                }
            }
        }

        if (!checkHit)
        {
            posB = origin + direction.normalized * maxDistance;
            hitPoint.gameObject.SetActive(false);
        }
        else
        {
            hitPoint.gameObject.SetActive(true);
        }
    }

    protected virtual void OnWorldCollision(Vector2 point, int layer)
    {
        if (hitPoint != null)
        {
            hitPoint.position = point;
            hitPoint.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }
    }

    public void SetRay(Vector2 origin, Vector2 direction, float maxDistance = -1)
    {
        this.origin = origin;
        this.direction = direction;

        if (maxDistance >= 0)
        {
            this.maxDistance = maxDistance;
        }
    }
}
