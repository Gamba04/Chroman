using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivatorButton : MonoBehaviour, IHittable
{
    [Header("Components")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private ParticleSystem vfx;

    [Header("Settings")]
    [SerializeField]
    private UnityEvent onTrigger;

    [Header("Info")]
    [ReadOnly, SerializeField]
    private bool activated;

    private void Start()
    {
        if (activated)
        {
            anim.SetBool("Activated", true);
            anim.SetTrigger("Hit");
        }
    }

    public void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        anim.SetTrigger("Hit");
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Button, transform.position);

        if (!activated)
        {
            activated = true;
            onTrigger?.Invoke();

            if (vfx) vfx.Play();
        }
        else
        {
            anim.SetBool("Activated", true);
        }
    }

    public bool AvoidingLayer(int layer)
    {
        return false;
    }

    public void ResetButton()
    {
        activated = false;
        anim.SetBool("Activated", false);
    }

    public float LifeRegenMultiplier { get => 0; }
}
