using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private new ParticleSystem particleSystem;
    [SerializeField]
    private new SpriteRenderer light;
    [SerializeField]
    private SpriteRenderer distortion;

    [Header("Settings")]
    [SerializeField]
    private Transition lightTransition;
    [SerializeField]
    private Transition distortionTransition;

    private void Start()
    {
        light.color = new Color(light.color.r, light.color.g, light.color.b, 0);
        distortion.color = new Color(0, 0, 0, 0);
    }

    private void Update()
    {
        LightUpdate();
    }

    private void LightUpdate()
    {
        lightTransition.UpdateTransitionValue();

        if (lightTransition.IsOnTransition)
        {
            light.color = new Color(light.color.r, light.color.g, light.color.b, lightTransition.value);
        }

        distortionTransition.UpdateTransitionValue();

        if (distortionTransition.IsOnTransition)
        {
            distortion.color = new Color(0, 0, 0, distortionTransition.value);
        }
    }

    public void Play()
    {
        particleSystem.Play();

        lightTransition.value = 0;
        lightTransition.StartTransition(1);

        distortionTransition.value = 0;
        distortionTransition.StartTransition(1);

        CameraController.Shake();
    }
}