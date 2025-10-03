using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class MaskRenderPass : ScriptableRenderPass
{

    private MaskSettings maskSettings;
    private Material material;


    public ScriptableCullingParameters scriptableCullingParameters;
    private LayerMask m_LayerMask;


    public MaskRenderPass(Material material, MaskSettings maskSettings)
    {
        this.material = material;
        this.maskSettings = maskSettings;
    }


    public void UpdateMaskSettings()
    {
        if (material == null) return;

        m_LayerMask = maskSettings.showOnLayer;
    }



    class PassData
    {
        public RendererListHandle rendererList;
        public TextureHandle maskTexture;
        public TextureHandle maskDepthTexture;
        public TextureHandle normalsTexture;
    }

    static readonly ShaderTagId[] k_ShaderTags = new ShaderTagId[]
    {
        new ShaderTagId("UniversalForward"),
        new ShaderTagId("UniversalForwardOnly"),
        new ShaderTagId("SRPDefaultUnlit"),
    };

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // UniversalLightData lightData = frameData.Get<UniversalLightData>();

        UpdateMaskSettings();


        // Make a single-channel mask target
        var desc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
        desc.colorFormat = GraphicsFormat.R8_UNorm;
        desc.filterMode = FilterMode.Point;
        desc.depthBufferBits = 0;
        desc.name = "_LayerMaskTexture";
        var maskTexture = renderGraph.CreateTexture(desc);

        // Make a single-channel mask target for depth
        var desc2 = resourceData.activeDepthTexture.GetDescriptor(renderGraph);
        desc2.colorFormat = GraphicsFormat.None;
        desc2.depthBufferBits = DepthBits.Depth32;
        // desc2.filterMode = FilterMode.Point;
        // desc2.depthBufferBits = 0;
        desc2.name = "_LayerMaskDepthTexture";
        var maskDepthTexture = renderGraph.CreateTexture(desc2);

        // Make a single-channel mask target for depth
        var desc3 = resourceData.cameraNormalsTexture.GetDescriptor(renderGraph);
        desc3.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
        // desc3.depthBufferBits = DepthBits.Depth32;
        // desc3.filterMode = FilterMode.Point;
        desc3.depthBufferBits = 0;
        desc3.name = "_MaskedNormalsTexture";
        var normalsTexture = renderGraph.CreateTexture(desc3);



        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Random Ahh Mask Pass lmao", out var passData))
        {
            //correct version
            // Get the data needed to create the list of objects to draw

            UniversalLightData lightData = frameData.Get<UniversalLightData>();
            SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
            RenderQueueRange renderQueueRange = RenderQueueRange.opaque;

            // int includes = m_LayerMask;

            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all, m_LayerMask);

            // Redraw only objects that have their LightMode tag set to UniversalForward 
            // ShaderTagId shadersToOverride = new ShaderTagId("UniversalForward");

            // Create drawing settings
            DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(k_ShaderTags.ToList(), renderingData, cameraData, lightData, sortFlags);


            // Create the list of objects to draw
            var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
            // Convert the list to a list handle that the render graph system can use
            passData.rendererList = renderGraph.CreateRendererList(rendererListParameters);

            passData.maskTexture = maskTexture;

            passData.maskDepthTexture = maskDepthTexture;

            passData.normalsTexture = normalsTexture;

            // Set the render target as the color and depth textures of the active camera texture
            // UniversalResourceData resourceData2 = frameData.Get<UniversalResourceData>();
            // builder.SetRenderAttachment(resourceData.activeColorTexture, 1);

            builder.SetRenderAttachment(maskTexture, 0, AccessFlags.Write);

            builder.SetRenderAttachment(normalsTexture, 1, AccessFlags.Write);

            builder.SetRenderAttachmentDepth(maskDepthTexture, AccessFlags.Write);

            builder.UseRendererList(passData.rendererList);

            builder.SetGlobalTextureAfterPass(maskTexture, Shader.PropertyToID("_Mask"));

            builder.SetGlobalTextureAfterPass(maskDepthTexture, Shader.PropertyToID("_MaskDepth"));

            builder.SetGlobalTextureAfterPass(normalsTexture, Shader.PropertyToID("_NormalsTexture"));

            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }

    }


    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        // Clear the render target to black
        context.cmd.ClearRenderTarget(true, true, Color.black);

        // Draw the objects in the list
        context.cmd.DrawRendererList(data.rendererList);
    }


}
