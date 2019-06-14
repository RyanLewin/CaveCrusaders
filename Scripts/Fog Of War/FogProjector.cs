using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class FogProjector : MonoBehaviour
{
    public Material projectorMaterial;
    public float blendSpeed;
    public int textureScale;

    public RenderTexture fogTexture;

    private RenderTexture prevTexture;
    private RenderTexture currTexture;
    private DecalProjectorComponent projector;

    private float blendAmount;

    private void Awake()
    {
        projector = GetComponent<DecalProjectorComponent>();
        projector.enabled = true;

        prevTexture = GenerateTexture();
        currTexture = GenerateTexture();

        // Projector materials aren't instanced, resulting in the material asset getting changed.
        // Instance it here to prevent us from having to check in or discard these changes manually.
        projector.m_Material = new Material(projectorMaterial);

        projector.m_Material.SetTexture("_PrevTexture", prevTexture);
        projector.m_Material.SetTexture("_CurrTexture", currTexture);

        StartNewBlend();
    }

    RenderTexture GenerateTexture()
    {
        RenderTexture rt = new RenderTexture(
            fogTexture.width * textureScale,
            fogTexture.height * textureScale,
            0,
            fogTexture.format)
        { filterMode = FilterMode.Bilinear };
        rt.antiAliasing = fogTexture.antiAliasing;

        return rt;
    }

    public void StartNewBlend()
    {
        StopCoroutine(BlendFog());
        blendAmount = 0;
        // Swap the textures
        Graphics.Blit(currTexture, prevTexture);
        Graphics.Blit(fogTexture, currTexture);

        StartCoroutine(BlendFog());
    }

    IEnumerator BlendFog()
    {
        while (blendAmount < 1)
        {
            // increase the interpolation amount
            blendAmount += Time.deltaTime * blendSpeed;
            // Set the blend property so the shader knows how much to lerp
            // by when checking the alpha value
            projector.m_Material.SetFloat("_Blend", blendAmount);
            yield return null;
        }
        // once finished blending, swap the textures and start a new blend
        StartNewBlend();
    }
}