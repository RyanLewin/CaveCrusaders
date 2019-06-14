using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(OutlineBliterRenderer), PostProcessEvent.AfterStack, "Custom/OutlineBliter")]
public sealed class OutlineBliter : PostProcessEffectSettings
{
    [Tooltip("Render target")]
    public TextureParameter RT = new TextureParameter();
    [Tooltip("Depth Buffer")]
    public TextureParameter DepthRT = new TextureParameter();
    [Range(0f, 1f), Tooltip("Render target")]
    public IntParameter Debug = new IntParameter();
}

public sealed class OutlineBliterRenderer : PostProcessEffectRenderer<OutlineBliter>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/OutlineBliter"));
        if (settings.RT.value != null)
        {
            sheet.properties.SetTexture("_RT", settings.RT);
            sheet.properties.SetInt("_Debug", settings.Debug);
        }
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}