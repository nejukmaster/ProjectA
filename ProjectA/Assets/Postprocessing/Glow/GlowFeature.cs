using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Unity.Burst.Intrinsics.X86.Avx;

public class GlowFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CustomPassSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material glowMat;
        public Material glowMaskMat;
    }
    class CustomRenderPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier colorBuffer, shaderBuffer, glowBuffer;
        private int shaderBufferID = Shader.PropertyToID("_GlowShader");
        private int glowBufferID = Shader.PropertyToID("_GlowBuffer");

        Material glowMat;
        Material glowMaskMat;
        public CustomRenderPass(CustomPassSettings settings)
        {
            glowMat = settings.glowMat;
            glowMaskMat = settings.glowMaskMat;
            renderPassEvent = settings.renderPassEvent;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTargetIdentifier[] rb = new RenderTargetIdentifier[]
            {
                shaderBufferID, glowBufferID
            };
            cmd.GetTemporaryRT(shaderBufferID, cameraTextureDescriptor.width, cameraTextureDescriptor.height, cameraTextureDescriptor.depthBufferBits, FilterMode.Bilinear, RenderTextureFormat.Default);
            cmd.GetTemporaryRT(glowBufferID, cameraTextureDescriptor.width, cameraTextureDescriptor.height, cameraTextureDescriptor.depthBufferBits, FilterMode.Bilinear, RenderTextureFormat.Default);

            ConfigureTarget(rb);
            ConfigureClear(ClearFlag.None, Color.clear);
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            shaderBuffer = new RenderTargetIdentifier(shaderBufferID);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("Glowing")))
            {
                cmd.Blit(colorBuffer, glowBuffer);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null) throw new System.ArgumentNullException("cmd");
            cmd.ReleaseTemporaryRT(shaderBufferID);
            cmd.ReleaseTemporaryRT(glowBufferID);
        }
    }

    [SerializeField] private CustomPassSettings settings;
    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


