using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Occlusion : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Camera renderCamera;
    [SerializeField]
    private RenderTexture lightsRender;
    [SerializeField]
    private Material occlusionShader;
    [SerializeField]
    private RawImage output;

    private RenderTexture renderTexture;
    private Texture2D texture;

    private void Start()
    {
        output.gameObject.SetActive(true);

        Setup();
    }

    private void Setup()
    {
        texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);

        lightsRender = new RenderTexture(renderTexture);
        renderCamera.targetTexture = lightsRender;
    }

    void Update()
    {
        PassInfoToShader();
        UpdateTexture();
    }

    private void PassInfoToShader()
    {
        occlusionShader.SetTexture("_CamTex", lightsRender);
    }

    private void UpdateTexture()
    {
        Graphics.Blit(renderTexture, renderTexture, occlusionShader);
        Graphics.CopyTexture(renderTexture, texture);

        output.texture = texture;
    }

}
