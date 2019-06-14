using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public Material mat;
    public RenderTexture dest;

    private void OnRenderImage(RenderTexture src, RenderTexture destination)
    {
        Graphics.Blit(src, dest, mat);
    }
}
