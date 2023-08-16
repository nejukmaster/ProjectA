using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ProfilingScope = UnityEngine.Rendering.ProfilingScope;

public class CharacterOutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CustomPassSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material mat;
        public Material outlineMapping;
    }
    class CustomRenderPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier colorBuffer, shaderBuffer;
        private int shaderBufferID = Shader.PropertyToID("_ShaderBuffer");


        private Material material;
        private Material outlineMapping;
        private RenderTexture outlineMap;
        public CustomRenderPass(CustomPassSettings settings) : base()
        {
            this.renderPassEvent = settings.renderPassEvent;
            this.material = settings.mat;
            this.outlineMapping = settings.outlineMapping;

            ConfigureInput(ScriptableRenderPassInput.Normal);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            renderingData.cameraData.camera.depthTextureMode = DepthTextureMode.DepthNormals;
            colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            cmd.GetTemporaryRT(shaderBufferID, descriptor, FilterMode.Point);
            shaderBuffer = new RenderTargetIdentifier(shaderBufferID);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            if (outlineMap == null) outlineMap = new CustomRenderTexture(renderingData.cameraData.camera.pixelWidth, renderingData.cameraData.camera.pixelHeight);

            if (renderingData.cameraData.camera.CompareTag("MainCamera"))
            {
                using (new ProfilingScope(cmd, new ProfilingSampler("Character Outline Pass")))
                {
                    material.SetTexture("_Outline", outlineMap);
                    cmd.Blit(colorBuffer, shaderBuffer, material);
                    cmd.Blit(shaderBuffer, colorBuffer);
                } 
            }
            else if (renderingData.cameraData.camera.CompareTag("CharacterCam"))
            {
                using (new ProfilingScope(cmd, new ProfilingSampler("Outline Mapping")))
                {
                    cmd.Blit(colorBuffer, shaderBuffer, outlineMapping);
                    cmd.Blit(shaderBuffer, colorBuffer);
                    cmd.Blit(colorBuffer, outlineMap);
                }
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null) throw new System.ArgumentNullException("cmd");
            cmd.ReleaseTemporaryRT(shaderBufferID);
        }
    }

    private CustomRenderPass m_ScriptablePass;
    [SerializeField] CustomPassSettings m_Settings;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(m_Settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


