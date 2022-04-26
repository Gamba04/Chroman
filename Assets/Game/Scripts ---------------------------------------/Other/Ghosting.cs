using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghosting : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Transform poolParent;
    [SerializeField]
    private Transform targetTransform;
    [Header("Settings")]
    [SerializeField]
    private float frecuency = 5;
    [SerializeField]
    private float lifeTime = 0.5f;

    [GambaHeader("Info ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [ReadOnly, SerializeField]
    private bool isEnabled;
    [ReadOnly, SerializeField]
    private bool allOff;
    [ReadOnly, SerializeField]
    private List<SpriteRenderer> pool = new List<SpriteRenderer>();

    private int poolSize;

    private Sprite currentSprite;
    private Color currentColor;

    private Transform poolParentParent;

    public bool IsEnabled => isEnabled;

    public Sprite CurrentSprite => currentSprite;

    public Color CurrentColor => currentColor;

    void Start()
    {
        Setup();
    }

    // Note frecuency and lifetime of the effect must not change later
    private void Setup()
    {
        if (frecuency <= 0)
        {
            frecuency = 1;
        }

        if (poolParent == null)
        {
            poolParent = new GameObject($"Ghosting_{gameObject.name}").transform;
        }

        pool = new List<SpriteRenderer>();

        poolSize = Mathf.FloorToInt(frecuency * lifeTime) + 1;

        for (int i = 0; i < poolSize; i++)
        {
            pool.Add(new GameObject($"{gameObject.name}_{i}", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>());

            pool[i].transform.SetParent(poolParent);
        }

        if (poolParentParent != null)
        {
            poolParent.SetParent(poolParentParent);
        }

        StartCoroutine(Loop());
    }

    void Update()
    {
        if (!allOff)
        {
            UpdateSprites();
        }
    }

    private void UpdateSprites() 
    {
        bool anyActive = false;

        if (pool.Count == 0)
        {
            Debug.LogError("NO SPRITES");
        }

        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null)
            {
                if (pool[i].gameObject.activeInHierarchy)
                {
                    pool[i].color -= new Color(0, 0, 0, (Time.deltaTime / lifeTime) / currentColor.a);

                    if (pool[i].color.a <= 0)
                    {
                        pool[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        anyActive = true;
                    }
                }
            }
        }

        if (!anyActive)
        {
            allOff = true;
        }
    }

    private void CallSprite(int index)
    {
        if (pool != null && index < pool.Count)
        {
            if (pool[index] != null)
            {
                pool[index].gameObject.SetActive(true);
                pool[index].color = currentColor;
                pool[index].sprite = currentSprite;

                if (targetTransform != null)
                {
                    pool[index].transform.position = targetTransform.position;
                    pool[index].transform.rotation = targetTransform.rotation;
                }
                else
                {
                    Debug.LogWarning("No target transform setup");
                }
            }
        }
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (isEnabled)
                {
                    CallSprite(i);
                    allOff = false;
                }
                yield return new WaitForSeconds(1f / frecuency);
            }
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary> Turn On Ghosting Effect, setup the target sprite of the effect and target color. </summary>
    public void Enable(Sprite targetSprite, Color targetColor)
    {
        currentSprite = targetSprite;
        currentColor = targetColor;

        isEnabled = true;
    }

    /// <summary> Turn Off Ghosting Effect. </summary>
    public void Disable()
    {
        isEnabled = false;
    }

    /// <summary> Only set the target Sprite. </summary>
    public void SetSprite(Sprite targetSprite)
    {
        currentSprite = targetSprite;
    }

    /// <summary> Only set the target Color. </summary>
    public void SetColor(Color targetColor)
    {
        currentColor = targetColor;
    }

    public void SetParent(Transform parent)
    {
        poolParentParent = parent;

        if (poolParent != null)
        {
            poolParent.SetParent(parent);
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void OnDestroy()
    {
        for (int i = pool.Count - 1; i >= 0 ; i--)
        {
            if (pool[i] != null)
            {
                Destroy(pool[i].gameObject);
            }
            pool.RemoveAt(i);
        }
    }
}
