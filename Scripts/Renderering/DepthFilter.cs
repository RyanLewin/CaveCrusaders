using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(DepthFilterRenderer), PostProcessEvent.BeforeStack, "Custom/DepthFilter")]
public sealed class DepthFilter : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("Render target")]
    public IntParameter Debug = new IntParameter();
    [Range(0f, 1f), Tooltip("Render target")]
    public TextureParameter RT = new TextureParameter();
    [Range(0f, 1f), Tooltip("DepthBuffer")]
    public TextureParameter DepthB = new TextureParameter();
    [Range(0f, 1f), Tooltip("Render target")]
    public IntParameter EnableFilter = new IntParameter();
}

public sealed class DepthFilterRenderer : PostProcessEffectRenderer<DepthFilter>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/DepthFilter"));
        if (settings.RT.value != null)
        {
            sheet.properties.SetTexture("_RT", settings.RT);
        }
        sheet.properties.SetInt("_Debug", settings.Debug);
        sheet.properties.SetInt("_DFilter", settings.EnableFilter);
        if (settings.DepthB.value != null)
        {
            sheet.properties.SetTexture("_DepthB", settings.DepthB);
        }
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}