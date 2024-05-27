using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ShaderExtension
{
    public class WeaponOverHitRenderFeature : ScriptableRendererFeature
    {
        class CustomRenderPass : ScriptableRenderPass
        {
            private RenderTargetHandle renderTextureHandle;
            private Material material;

            public CustomRenderPass(Material material)
            {
                this.material = material;
                renderTextureHandle.Init("_RenderTexture");
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                cmd.GetTemporaryRT(renderTextureHandle.id, cameraTextureDescriptor);
                ConfigureTarget(renderTextureHandle.Identifier());
                ConfigureClear(ClearFlag.All, Color.black);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("CustomRenderPass");
                Blit(cmd, renderingData.cameraData.renderer.cameraColorTarget, renderTextureHandle.Identifier(), material);
                Blit(cmd, renderTextureHandle.Identifier(), renderingData.cameraData.renderer.cameraColorTarget);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(renderTextureHandle.id);
            }
        }

        public Material material;
        CustomRenderPass renderPass;

        public override void Create()
        {
            renderPass = new CustomRenderPass(material)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(renderPass);
        }
    }
}