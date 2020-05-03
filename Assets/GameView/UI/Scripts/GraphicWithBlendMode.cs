using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class GraphicWithBlendModeModifier : ShaderControlModifier
{
    private static string _srcBlendField = "_SrcBlend";
    private static string _dstBlendField = "_DstBlend";

    public UnityEngine.Rendering.BlendMode srcBlend = UnityEngine.Rendering.BlendMode.SrcAlpha;
    public UnityEngine.Rendering.BlendMode dstBlend = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        shaderControl.SetInt(_srcBlendField, (int)srcBlend);
        shaderControl.SetInt(_dstBlendField, (int)dstBlend);
    }
}
