using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class LightingPostProcess : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Camera lightsCamera;
    [Header("Settings")]
    [SerializeField]
    private Color occlusionColor;
    [SerializeField]
    [Range(0, 1)]
    private float lightIntensity;

    private Material occlusionShader;
    private Material postProcessShader;
    private Material blurShader;
    private Material bloomShader;
    private Material inverseShader;

    private RenderTexture lightsRender;
    private RenderTexture occlusionTexture;
    private RenderTexture resultImage;
    private RenderTexture resultCopy;

    private Camera mainCamera;

    private bool pauseShaders;

    private bool isOnTransition;
    private bool generateBlur;
    private int blurKernelSize;

    [HideInInspector]
    public bool intoPause;

    [HideInInspector]
    public float renderResolutionScale = 1;

    private void Start()
    {
        renderResolutionScale = 1;

        mainCamera = GetComponent<Camera>();
        mainCamera.allowHDR = true;
        mainCamera.depthTextureMode = DepthTextureMode.DepthNormals;

        MaterialsSetup();

        StartCoroutine(LateSetup());
    }

    private void MaterialsSetup()
    {
        string path = "Gamba" + "/";

        occlusionShader = new Material(Shader.Find(path + "Occlusion"));
        postProcessShader = new Material(Shader.Find(path + "LightsPostProcess"));
        blurShader = new Material(Shader.Find(path + "Blur"));
        bloomShader = new Material(Shader.Find(path + "Bloom"));
        inverseShader = new Material(Shader.Find(path + "InverseScreen"));
    }

    private IEnumerator LateSetup()
    {
        yield return new WaitForEndOfFrame();

        TexturesSetup();
    }

    private void TexturesSetup()
    {
        occlusionTexture = new RenderTexture(Screen.width, Screen.height, 0);
        occlusionTexture.format = RenderTextureFormat.DefaultHDR;

        resultImage = new RenderTexture(occlusionTexture);
        resultCopy = new RenderTexture(occlusionTexture);
        lightsRender = new RenderTexture(occlusionTexture);
            
        lightsCamera.targetTexture = lightsRender;
    }

    private void Update()
    {
        occlusionShader?.SetColor("_OcclusionColor", occlusionColor);

        lightsCamera.orthographicSize = mainCamera.orthographicSize;
    }

    private void RenderResolution()
    {
        if (renderResolutionScale > 0 && renderResolutionScale <= 1)
        {
            ScalableBufferManager.ResizeBuffers(renderResolutionScale, renderResolutionScale);
            //print($"buffers resized to {renderResolutionScale}");
            //print(ScalableBufferManager.heightScaleFactor);
        }
    }

    private void OnPreRender()
    {
        GlobalData();

        PassInfoToOcclusion();
        UpdateOcclusion();

        PassInfoToPostProcess();

        if (isOnTransition)
        {
            PassInfoToBlur();
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region PreRenderPasses

    private void GlobalData()
    {
        Shader.SetGlobalFloat("_XRatio", (float)Screen.width / (float)Screen.height);
        if (mainCamera) Shader.SetGlobalFloat("_CameraZoom", mainCamera.orthographicSize);
    }

    private void PassInfoToOcclusion()
    {
        occlusionShader?.SetTexture("_CamTex", lightsRender);
    }

    private void UpdateOcclusion()
    {
        if (occlusionShader != null)
        {
            Graphics.Blit(occlusionTexture, occlusionTexture, occlusionShader);
        }
    }

    private void PassInfoToPostProcess()
    {
        postProcessShader?.SetTexture("_OcclusionTex", occlusionTexture);
        postProcessShader?.SetFloat("_LightIntensity", lightIntensity);
    }

    private void PassInfoToBlur()
    {
        blurShader?.SetFloat("_ScrWidth", Screen.width);
        blurShader?.SetFloat("_ScrHeight", Screen.height);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region OnRenderPasses

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!pauseShaders)
        {
            if (isOnTransition)
            {
                isOnTransition = false;

                if (generateBlur)
                {
                    generateBlur = false;

                    // Blur
                    Graphics.Blit(source, resultImage, postProcessShader);
                    Graphics.Blit(resultImage, destination, blurShader);
                    Graphics.Blit(destination, resultCopy);
                }
                else
                {
                    RenderPause(ref destination);
                }
            }
            else
            {
                // Main Behaviour
                Graphics.Blit(source, destination, postProcessShader);
                Graphics.Blit(destination, resultCopy);
            }
        }
        else
        {
            RenderPause(ref destination);
        }
    }

    private void RenderPause(ref RenderTexture destination)
    {
        if (GameManager.TargetPlatform != Platform.WebGL)
        {
            Graphics.Blit(resultCopy, destination, inverseShader);
        }
        else
        {
            Graphics.Blit(resultCopy, destination);
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    public void PauseShaders(bool value)
    {
        pauseShaders = value;
    }

    public void SetBlurAmount(float amount)
    {
        isOnTransition = amount > 0.01f;

        int targetKernelSize = Mathf.RoundToInt(amount);

        if (targetKernelSize != blurKernelSize || !intoPause)
        {
            blurKernelSize = targetKernelSize;

            blurShader.SetInt("_KernelSize", blurKernelSize);

            generateBlur = true;
        }
    }

    public void SetTexturesSize(Vector2 newSize)
    {
        occlusionTexture = new RenderTexture(Mathf.RoundToInt(newSize.x), Mathf.RoundToInt(newSize.y), 0);

        resultImage = new RenderTexture(occlusionTexture);
        resultCopy = new RenderTexture(occlusionTexture);
        lightsRender = new RenderTexture(occlusionTexture);

        lightsCamera.targetTexture = lightsRender;
    }

    #endregion

}