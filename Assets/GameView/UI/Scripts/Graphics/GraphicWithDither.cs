using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class GraphicWithDitherModifier : ShaderControlModifier
{
    private static string _applyDitherField = "_ApplyDither";
    private static string _ditherStrengthField = "_DitherStrength";
    private static string _ditherScaleField = "_DitherScale";
    
    public static bool GlobalAllowDither = true;

    public bool applyDither = false;
    [Range(1, 1024)] public float ditherStrengthInverse = 128;
    [Range(0.1f, 10000f)] public float ditherScale = 1;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        shaderControl.SetBool(_applyDitherField, applyDither && GlobalAllowDither);
        shaderControl.SetFloat(_ditherStrengthField, 1f / ditherStrengthInverse);
        shaderControl.SetFloat(_ditherScaleField, ditherScale);
    }
}