using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayscaleRenderPass : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {

        public Material effectMaterial;
        private RTHandle source;
        private RTHandle tempTexture;

        public CustomRenderPass(Material material)
        {
            this.effectMaterial = material;
        }

        

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        
        }



    }


    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        // m_ScriptablePass = new CustomRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


