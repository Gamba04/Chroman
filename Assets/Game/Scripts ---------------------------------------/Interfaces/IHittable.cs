using System.Collections;
using UnityEngine;

public interface IHittable
{
    void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer);

    bool AvoidingLayer(int layer);

    float LifeRegenMultiplier { get; }
}
