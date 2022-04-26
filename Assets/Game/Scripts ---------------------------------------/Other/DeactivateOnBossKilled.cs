using UnityEngine;

public class DeactivateOnBossKilled : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.bossKilled)
        {
            gameObject.SetActive(false);
        }
    }
}
