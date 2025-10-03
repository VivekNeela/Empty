using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullscreenRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private MaskSettings maskSettings;
    [SerializeField] private OutlineSettings outlineSettings;

    [SerializeField] private Shader shader;
    private Material material;

    private MaskRenderPass maskRenderPass;
    private FulscreenRenderPass fullscreenRenderPass;


    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);

        maskRenderPass ??= new MaskRenderPass(material, maskSettings);

        fullscreenRenderPass ??= new FulscreenRenderPass(material, outlineSettings);


        //before
        maskRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;

        //after
        fullscreenRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }


    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Don't render for some views.
        if (renderingData.cameraData.cameraType == CameraType.Preview
            || renderingData.cameraData.cameraType == CameraType.Reflection
            || UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
            return;

        if (material == null)
        {
            material = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Edge Detection"));
            if (material == null)
            {
                Debug.LogError("Not all required materials could be created. Edge Detection will not render.");
                return;
            }
        }

        fullscreenRenderPass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Color);

        if (fullscreenRenderPass == null || maskRenderPass == null)
        {
            return;
        }

        if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderer.EnqueuePass(maskRenderPass);
            renderer.EnqueuePass(fullscreenRenderPass);
        }
    }



    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }
}



[Serializable]
public class OutlineSettings
{
    public float thickness;
    public float depthSens;
    public float normalSens;
    public Color color;
}

[Serializable]
public class MaskSettings
{

    public LayerMask showOnLayer;
}


