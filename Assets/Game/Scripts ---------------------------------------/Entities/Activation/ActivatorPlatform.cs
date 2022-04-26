using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivatorPlatform : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Collider2D collider;
    [SerializeField]
    private SpriteRenderer colorFill;
    [SerializeField]
    private ParticleSystem vfx;

    [Header("Settings")]
    [SerializeField]
    private Color color = Color.white;
    [SerializeField]
    private UnityEvent onPress;
    [SerializeField]
    private bool deactivateOnRelease;
    [SerializeField]
    private UnityEvent onRelease;
    [SerializeField]
    private LayerMask detectionLayers;
    [SerializeField]
    private AudioPlayer.SFXTag pressSound = AudioPlayer.SFXTag.Restart;
    [SerializeField]
    private AudioPlayer.SFXTag releaseSound = AudioPlayer.SFXTag.Restart;

    [Header("Info")]
    [ReadOnly, SerializeField]
    private bool activated;

    // Detection
    private ContactFilter2D filter = new ContactFilter2D();
    private List<Collider2D> targets = new List<Collider2D>();

    private List<Collider2D> previousTargets = new List<Collider2D>();

    private void Start()
    {
        anim.SetBool("Activated", activated);

        if (colorFill != null)
        {
            colorFill.color = new Color(color.r, color.g, color.b, 0);
        }

        SetFilter();
    }

    private void SetFilter()
    {
        filter.layerMask = detectionLayers;
        filter.useLayerMask = true;
    }

    private void Update()
    {
        PressDetection();
    }

    private void PressDetection()
    {
        collider.OverlapCollider(filter, targets);

        if (!activated)
        {
            if (targets.Count > 0)
            {
                OnPress();
                SendInfoToTargets(targets, true);

                previousTargets = new List<Collider2D>(targets);
            }
        }
        else if (deactivateOnRelease)
        {
            if (targets.Count == 0)
            {
                OnRelease();
                SendInfoToTargets(previousTargets, false);

                previousTargets.Clear();
            }
        }
    }

    private void OnPress()
    {
        activated = true;
        anim.SetBool("Activated", true);

        onPress?.Invoke();

        AudioPlayer.PlaySFX(pressSound);
        if (vfx) vfx.Play();
    }

    private void OnRelease()
    {
        activated = false;
        anim.SetBool("Activated", false);

        onRelease?.Invoke();

        AudioPlayer.PlaySFX(releaseSound);
    }

    public void ResetPlatform()
    {
        activated = false;
        anim.SetBool("Activated", false);
    }

    private void SendInfoToTargets(List<Collider2D> targets, bool activate)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            ElectricBox target = targets[i].attachedRigidbody?.GetComponent<ElectricBox>();

            if (target != null)
            {
                target.SetState(activate);
            }
        }
    }

    private void OnValidate()
    {
        if (colorFill != null)
        {
            colorFill.color = new Color(color.r, color.g, color.b, colorFill.color.a);
        }
    }
}
