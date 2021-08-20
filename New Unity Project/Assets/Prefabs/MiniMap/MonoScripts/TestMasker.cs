using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering;

public class TestMasker : RawImage
{
    public override Material materialForRendering
    {
        get
        {
            Material material = new Material(base.materialForRendering);
            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return material;
        }
    }
    
}
