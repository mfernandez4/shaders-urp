using UnityEngine;


public abstract class PostProcessEffect
{
    
    
    protected Material Material;
    
    
    public abstract Material GetMaterial();
    
    public virtual void ReleaseBuffers()
    {
        
    }
    
    
}
