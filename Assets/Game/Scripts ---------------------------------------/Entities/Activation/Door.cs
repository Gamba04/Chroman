using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private ParticleSystem openVFX;
    [SerializeField]
    private ParticleSystem closeVFX;

    [Header("Settings")]
    [SerializeField]
    private byte amountOfPointsToCall = 1;

    [ReadOnly, SerializeField]
    private bool open;

    private byte points;

    private void Start()
    {
        anim.SetBool("Open", open);
    }

    public void SetOpen(bool value)
    {
        if (++points >= amountOfPointsToCall)
        {
            open = value;
            anim.SetBool("Open", value);

            points = 0;

            if (value)
            {
                if (openVFX) openVFX.Play();
            }
            else
            {
                if (closeVFX) closeVFX.Play();
            }
        }
    }

    public void PlaySound()
    {
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Door, transform.position);
    }
}
 