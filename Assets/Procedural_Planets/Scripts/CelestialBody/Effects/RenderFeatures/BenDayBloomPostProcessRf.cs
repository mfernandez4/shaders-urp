using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class BenDayBloomPostProcessRf : ScriptableRendererFeature
{
    

    [SerializeField] private Shader bloomShader;
    [SerializeField] private Shader compositeShader;
    
    private Material _bloomMaterial;
    private Material _compositeMaterial;
    private BenDayBloomRenderPass _pass;


    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.camera.cameraType == CameraType.SceneView)
        {
            return;
        }

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            _pass.ConfigureInput(ScriptableRenderPassInput.Depth);
            _pass.ConfigureInput(ScriptableRenderPassInput.Color);
            // _pass.
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass); // Add the pass to the renderer queue
    }
    
    
    public override void Create()
    {
        if (bloomShader != null && compositeShader != null)
        {
            _bloomMaterial = 
                CoreUtils.CreateEngineMaterial(bloomShader); // Create a new material from the shader
            _compositeMaterial =
                CoreUtils.CreateEngineMaterial(compositeShader); // Create a new material from the shader
        }
        
        
        _pass = new BenDayBloomRenderPass(_bloomMaterial, _compositeMaterial); // Create a new instance of the render pass
    }


    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_bloomMaterial); // Destroy the material
        CoreUtils.Destroy(_compositeMaterial); // Destroy the material
    }
}
