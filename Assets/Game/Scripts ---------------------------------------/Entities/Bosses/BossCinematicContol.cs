using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCinematicContol : MonoBehaviour
{
    [SerializeField]
    private Boss2 boss;

    public void ActivateRoomZoom(int area)
    {
        boss.ActivateRoomZoom(area);
    }

    public void EndCinematic()
    {
        boss.EndCinematic();
    }
}