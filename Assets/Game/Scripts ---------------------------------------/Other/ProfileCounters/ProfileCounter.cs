using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileCounter : MonoBehaviour
{
    [SerializeField]
    protected Text text;
    [SerializeField]
    protected float updateDelay = 0.1f;

    private float counter;

    private void Update()
    {
        counter += Time.unscaledDeltaTime;

        if (counter >= updateDelay)
        {
            counter = 0;

            UpdateText();
        }
    }

    protected virtual void UpdateText() { }

#if UNITY_EDITOR

    private void OnValidate()
    {
        Text text = GetComponent<Text>();

        if (text)
        {
            this.text = text;
        }
    }

#endif
}
