using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[ExecuteInEditMode]
public class PauseController : MonoBehaviour
{
    [Serializable]
    private class PauseOption
    {
        [SerializeField, HideInInspector] private string name;

        public GameObject prefab;

        public float height;

        public void SetName(string name)
        {
            this.name = name;
        }
    }

    [Serializable]
    private class Page
    {
        [SerializeField, HideInInspector] private string name;

        public Transform root;
        public Transform optionsParent;

        public List<OptionTag> options;

        public List<Graphic> additionalGraphics;
        [GambaHeader("-----------------------------------------------------------------------------------------------------------", 0.3f)]
        [ReadOnly]
        public Vector2 position;
        [ReadOnly]
        public bool interactable;
        [Space()]
        [HideInInspector]
        public List<GameObject> pageElements;
        [HideInInspector]
        public List<ButtonSetup> setups;
        [HideInInspector]
        public List<Graphic> capturedGraphics;

        private List<OptionTag> lastOptions = new List<OptionTag>();

        public void RuntimeUpdate()
        {
            for (int i = 0; i < setups.Count; i++)
            {
                setups[i].SetInteractable(interactable);
            }
        }

#if UNITY_EDITOR
        public void Refresh(List<PauseOption> optionTypes)
        {
            if (root != null)
            {
                position = root.localPosition;
            }

            if (optionsParent != null)
            {
                OptionsChangeUpdate();

                // Reload GameObjects -------------------------------------------
                int targetOptions = options.Count;
                int graphicsCount = 0;

                GambaFunctions.ResizeListEmpty(ref pageElements, targetOptions);
                setups.Clear();

                for (int i = 0; i < targetOptions; i++)
                {
                    int optionId = (int)options[i];

                    if (i >= optionsParent.childCount)
                    {
                        if (optionId < optionTypes.Count)
                        {
                            // Instantiate new GameObject
                            GameObject element = PrefabUtility.InstantiatePrefab(optionTypes[optionId].prefab, optionsParent) as GameObject;

                            // Assign Page Element
                            pageElements[i] = element;
                        }
                    }
                    else
                    {
                        // Assign Page Element
                        if (pageElements[i] == null)
                        {
                            pageElements[i] = optionsParent.GetChild(i).gameObject;
                        }
                    }

                    // Add Graphics to capturedGraphics
                    ButtonSetup setup = GetSetup(pageElements[i]);
                    setup.SetHeight(optionTypes[optionId].height);
                    setups.Add(setup);

                    for (int g = 0; g < setup.Graphics.Count; g++)
                    {
                        if (graphicsCount < capturedGraphics.Count)
                        {
                            capturedGraphics[graphicsCount] = setup.Graphics[g];
                        }
                        else
                        {
                            capturedGraphics.Add(setup.Graphics[g]);
                        }

                        graphicsCount++;
                    }
                }

                if (optionsParent.childCount >= targetOptions)
                {
                    // Destroy optionsParent child excess
                    for (int i = optionsParent.childCount - 1; i >= targetOptions; i--)
                    {
                        GambaFunctions.DestroyInEditor(optionsParent.GetChild(i).gameObject);
                    }
                }

                if (capturedGraphics.Count > graphicsCount)
                {
                    // Remove capturedGraphics excess
                    for (int i = capturedGraphics.Count - 1; i >= graphicsCount; i--)
                    {
                        capturedGraphics.RemoveAt(i);
                    }
                }

                // Organize Elements -------------------------------------------
                float height = 0;
                for (int i = 0; i < options.Count; i++)
                {
                    pageElements[i].transform.localPosition = Vector3.down * height;

                    int optionId = (int)options[i];

                    if (optionId < optionTypes.Count)
                    {
                        height += optionTypes[(int)options[i]].height;
                    }
                }
            }
        }

        private void OptionsChangeUpdate()
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (i < lastOptions.Count)
                {
                    if (lastOptions[i] != options[i])
                    {
                        for (int c = optionsParent.childCount - 1; c >= 0; c--)
                        {
                            GambaFunctions.DestroyInEditor(optionsParent.GetChild(c).gameObject);
                        }
                        break;
                    }
                }
            }

            lastOptions = new List<OptionTag>(options);
        }

#endif
        public void SetName(int index)
        {
            name = $"Page {index}";
        }
    }

    [Serializable]
    private class PauseGraphic
    {
        [SerializeField, HideInInspector] private string name;

        [SerializeField]
        private string text;
        public Graphic graphic;
        [ReadOnly]
        public float defaultGraphicAlpha;

        private int page;

        public void Initializate()
        {
            Text text = graphic as Text;
            this.text = (text != null) ? text.text : "Default";
        }

        public void SetPage(int page)
        {
            this.page = page;
        }

        public void UpdateAlpha()
        {
            if (graphic == null) return;

            defaultGraphicAlpha = graphic.color.a;
        }

        public void SetAlpha(float value)
        {
            if (graphic == null) return;

            Color color = graphic.color;

            color.a = value * defaultGraphicAlpha;

            graphic.color = color;
        }

        public void SetName()
        {
            string page = (this.page >= 0) ? $"Page {this.page}: " : "Additional graphic: ";

            Text text = graphic as Text;

            if (text != null)
            {
                text.text = this.text;
            }

            string title = (text != null) ? (this.text == "") ? " " : this.text : (graphic != null) ? graphic.gameObject.name : "None";

            name = page + title;
        }
    }

    public enum OptionTag
    {
        TextButton,
        Slider,
        Selector,
    }

    [Header("Components")]
    [SerializeField]
    private List<PauseOption> optionsSetup = new List<PauseOption>();
    [SerializeField]
    private List<PauseGraphic> pauseGraphics = new List<PauseGraphic>();
    [Header("Settings")]
    [SerializeField]
    private List<Page> pages = new List<Page>();
    [Space()]
    [SerializeField]
    public List<Graphic> additionalGraphics;
    [SerializeField]
    private Transition pauseTransition;
    [Header("Info")]
    [ReadOnly, SerializeField]
    private int currentPage;
    [ReadOnly, SerializeField]
    private Vector2 targetPosition;
    [ReadOnly, SerializeField]
    private bool buttonsInteractable;

    private float lastWidth;

    private bool isActivated = true;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            buttonsInteractable = true;

            for (int i = 0; i < pauseGraphics.Count; i++)
            {
                if (pauseGraphics[i] != null) pauseGraphics[i].SetName();
            }

            foreach (Page p in pages) 
            {
                UpdateButtonScale(p);
                lastWidth = Screen.width;
            }
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (isActivated)
            {
                PositionUpdate();

                PagesUpdate();
            }
        }
        else
        {
            EditorUpdate();
        }
    }

    private void LateUpdate()
    {
        if (Application.isPlaying)
        {
            PauseTransitionLateUpdate();
        }
    }

    private void PauseTransitionLateUpdate()
    {
        pauseTransition.UpdateTransitionValue();

        if (!GameManager.GamePaused || pauseTransition.IsOnTransition)
        {
            for (int i = 0; i < pauseGraphics.Count; i++)
            {
                if (pauseGraphics[i] != null) pauseGraphics[i].SetAlpha(pauseTransition.value);
            }
        }
    }

    #region Public Methods

    public void SetActivated(bool value)
    {
        if (isActivated == !value)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(value);
            }

            isActivated = value;
        }
    }

    public void SetPauseTransition(bool value)
    {
        if (value)
        {
            SetActivated(true);

            ChangePage(0);
            transform.localPosition = targetPosition;

            pauseTransition.StartTransitionUnscaled(1);
        }
        else
        {
            pauseTransition.StartTransition(0, ()=>
            {
                SetActivated(false);
            });
        }
    }

    public void ChangePage(int page)
    {
        currentPage = page;
        targetPosition = -pages[page].position;
    }

    public void Resume()
    {
        GameManager.SetPause(false);
    }

    public void SetInteractable(bool value)
    {
        buttonsInteractable = value;
    }
    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void PositionUpdate()
    {
        transform.localPosition += ((Vector3)targetPosition - transform.localPosition) * 0.1f * Time.unscaledDeltaTime / 0.02f;
    }

    private void PagesUpdate()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            Page p = pages[i];
            {
                if (buttonsInteractable)
                {
                    if (GameManager.GamePaused && !pauseTransition.IsOnTransition)
                    {
                        if (i == currentPage)
                        {
                            p.interactable = true;
                        }
                        else
                        {
                            p.interactable = false;
                        }
                    }
                    else
                    {
                        p.interactable = false;
                    }
                }
                else
                {
                    p.interactable = false;
                }

                if (lastWidth != Screen.width)
                {
                    UpdateButtonScale(p);
                    lastWidth = Screen.width;
                }

                p.RuntimeUpdate();
            }
        }
    }

    private void UpdateButtonScale(Page page)
    {
        for (int i = 0; i < page.options.Count; i++)
        {
            int optionID = (int)page.options[i];

            ButtonSetup setup = GetSetup(page.pageElements[i]);

            setup.SetHeight(optionsSetup[optionID].height);
        }
    }

    private static ButtonSetup GetSetup(GameObject gameObject)
    {
        if (gameObject != null)
        {
            ButtonSetup setup = gameObject.GetComponent<ButtonSetup>();

            return setup;
        }

        return null;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Editor

    private void EditorUpdate()
    {
#if UNITY_EDITOR
        #region Options Setup

        OptionTag[] optionTags = (OptionTag[])Enum.GetValues(typeof(OptionTag));

        GambaFunctions.ResizeList(ref optionsSetup, optionTags.Length);

        for (int i = 0; i < optionTags.Length; i++)
        {
            optionsSetup[i].SetName(optionTags[i].ToString());
        }

        #endregion

        #region Main Update

        int targetGraphicsAmount = 0;

        for (int i = 0; i < pages.Count; i++)
        {
            Page p = pages[i];

            p.Refresh(optionsSetup);

            List<Graphic> pageGraphics = new List<Graphic>(p.capturedGraphics);
            GambaFunctions.AddListElements(ref pageGraphics, p.additionalGraphics);

            for (int g = 0; g < pageGraphics.Count; g++)
            {
                if (targetGraphicsAmount < pauseGraphics.Count)
                {
                    if (pauseGraphics[targetGraphicsAmount].graphic != pageGraphics[g])
                    {
                        PauseGraphic newGraphic = new PauseGraphic();
                        newGraphic.graphic = pageGraphics[g];
                        newGraphic.Initializate();

                        pauseGraphics[targetGraphicsAmount] = newGraphic;
                    }
                }
                else
                {
                    PauseGraphic newGraphic = new PauseGraphic();
                    newGraphic.graphic = pageGraphics[g];
                    newGraphic.Initializate();

                    pauseGraphics.Add(newGraphic);
                }

                pauseGraphics[targetGraphicsAmount].SetPage(i);

                targetGraphicsAmount++;
            }
        }

        // Remove pauseGraphics excess
        for (int i = pauseGraphics.Count - 1; i >= 0; i--)
        {
            if (i > targetGraphicsAmount - 1)
            {
                pauseGraphics.RemoveAt(i);
            }
            else
            {
                break;
            }
        }

        // Add additional graphics
        for (int i = 0; i < additionalGraphics.Count; i++)
        {
            PauseGraphic newGraphic = new PauseGraphic();
            newGraphic.graphic = additionalGraphics[i];
            newGraphic.Initializate();
            newGraphic.SetPage(-1);

            pauseGraphics.Add(newGraphic);
        }

        #endregion

        for (int i = 0; i < pauseGraphics.Count; i++)
        {
            if (pauseGraphics[i] != null)
            {
                pauseGraphics[i].SetName();
                pauseGraphics[i].UpdateAlpha();
            }
        }

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetName(i);
        }
#endif
    }

    private void OnValidate()
    {
       
    }

    #endregion

}
