using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;


[System.Serializable]
public class BenDayBloomRenderPass : ScriptableRenderPass
{


    private Material _bloomMaterial;
    private Material _compositeMaterial;
    
    private RenderTextureDescriptor _bloomTextureDescriptor;
    
    
    private static readonly int BloomThreshold = Shader.PropertyToID("_BloomThreshold");
    
    
    public BenDayBloomRenderPass(Material bloomMaterial, Material compositeMaterial)
    {
        _bloomMaterial = bloomMaterial;
        _compositeMaterial = compositeMaterial;
        
        _bloomTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
    }
    
    
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData  rendererData = frameData.Get<UniversalResourceData>();
    }
    
    
    // public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    // {
    //     
    // }
}
