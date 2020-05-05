using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class GraphicWithDitherModifier : ShaderControlModifier
{
    private static string _applyDitherField = "_ApplyDither";
    private static string _ditherStrengthField = "_DitherStrength";


    public static bool GlobalAllowDither = true;

    public bool applyDither = false;
    [Range(1, 1024)] public float ditherStrengthInverse = 128;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        shaderControl.SetBool(_applyDitherField, applyDither && GlobalAllowDither);
        shaderControl.SetFloat(_ditherStrengthField, 1f / ditherStrengthInverse);
    }
}