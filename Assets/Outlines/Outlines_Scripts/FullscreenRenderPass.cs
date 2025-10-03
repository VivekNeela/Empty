using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class FulscreenRenderPass : ScriptableRenderPass
{

    //outline shader stuff
    private static readonly int outlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int depthSensitivityID = Shader.PropertyToID("_DepthSensitivity");
    private static readonly int normalSensitivityID = Shader.PropertyToID("_NormalSensitivity");
    private static readonly int outlineColorID = Shader.PropertyToID("_OutlineColor");

    //blit texture name
    private const string k_FullscreenTextureName = "_FullscreenTexture";

    //outline pass name
    private const string k_OutlinesPassName = "OutlinesPassFinal";

    private OutlineSettings defaultSettings_outlines;
    private Material material;

    private TextureDesc fullscreenTextureDescriptor;

    public ScriptableCullingParameters scriptableCullingParameters;


    public FulscreenRenderPass(Material material, OutlineSettings outlineSettings)
    {
        this.material = material;
        this.defaultSettings_outlines = outlineSettings;

    }


    public void UpdateOutlineSettings()
    {
        if (material == null) return;

        var outlineVolumeComponent = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();
        float thickness = outlineVolumeComponent.thickness.overrideState ? outlineVolumeComponent.thickness.value : defaultSettings_outlines.thickness;
        float depthSens = outlineVolumeComponent.depthSens.overrideState ? outlineVolumeComponent.depthSens.value : defaultSettings_outlines.depthSens;
        float normalSens = outlineVolumeComponent.normalSens.overrideState ? outlineVolumeComponent.normalSens.value : defaultSettings_outlines.normalSens;
        Color color = outlineVolumeComponent.color.overrideState ? outlineVolumeComponent.color.value : defaultSettings_outlines.color;

        material.SetFloat(outlineThicknessID, thickness);
        material.SetFloat(depthSensitivityID, depthSens);
        material.SetFloat(normalSensitivityID, normalSens);
        material.SetVector(outlineColorID, color);

    }



    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        // UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

        // UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // UniversalLightData lightData = frameData.Get<UniversalLightData>();


        //-----Outlines fullscreen render pass

        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // RenderTargetIdentifier src = renderingData.cameraData.renderer.cameraColorTargetHandle;
        TextureHandle srcCamColor = resourceData.activeColorTexture;

        // var tex = Shader.GetGlobalTexture("_Mask");
        // renderGraph.ImportTexture();


        // ConfigureInput(ScriptableRenderPassInput.Normal);

        fullscreenTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
        fullscreenTextureDescriptor.name = k_FullscreenTextureName;
        fullscreenTextureDescriptor.depthBufferBits = 0;
        var dst = renderGraph.CreateTexture(fullscreenTextureDescriptor);


        // Update the outlines settings in the material
        UpdateOutlineSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;


        //need to do this adddin blit pass bullshit twice cuz thats how it works
        RenderGraphUtils.BlitMaterialParameters outlines_first = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(outlines_first, "OutlinePassInitial");

        RenderGraphUtils.BlitMaterialParameters outlines = new(dst, srcCamColor, material, 0);
        renderGraph.AddBlitPass(outlines, k_OutlinesPassName);

    }




}
