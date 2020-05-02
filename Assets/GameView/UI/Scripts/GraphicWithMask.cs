using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class GraphicWithMaskModifier : ShaderControlModifier
{
    private static string _maskField = "_MaskTex";
    private static string _maskWeightField = "_MaskWeight";

    [FormerlySerializedAs("Mask")] public Texture2D mask;
    [FormerlySerializedAs("MaskWeight")] [Range(0, 1)] public float maskWeight = 0.1f;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        shaderControl.SetTexture(_maskField, mask);
        shaderControl.SetFloat(_maskWeightField, maskWeight);
    }
}
