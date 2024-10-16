using UnityEngine;

public class USBSimpleColorController : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Texture texture;
    int textureSize = 512;
    Renderer rend;


    void Start()
    {
        // initialize the render texture
        renderTexture = new RenderTexture(
            textureSize, 
            textureSize, 
            0,
            RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        
        // Get the renderer component
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        // send the texture to the compute shader
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetTexture(0, "col_tex", texture);
        // set the texture to the material
        rend.material.SetTexture("_MainTex", renderTexture);
        
        // Generate the thread group to run the compute shader
        int threadGroups = textureSize / 8;
        computeShader.Dispatch(0, threadGroups, threadGroups, 1);
    }
}
