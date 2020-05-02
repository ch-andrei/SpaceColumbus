using UnityEngine;

public class GraphicWithRectModifier : ShaderControlModifier
{
    private static string _sizeXField = "_SizeX";
    private static string _sizeYField = "_SizeY";

    protected Rect Rect;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        Rect = shaderControl.GetComponent<RectTransform>().rect;
        shaderControl.SetFloat(_sizeXField, Rect.width);
        shaderControl.SetFloat(_sizeYField, Rect.height);
    }
}
