using UnityEngine;
using UnityEngine.Serialization;

public class GraphicWithBorder : GraphicShaderControl
{
    [FormerlySerializedAs("Mask")] public GraphicWithMaskModifier mask = new GraphicWithMaskModifier();
    [FormerlySerializedAs("Border")] public GraphicWithBorderModifier border = new GraphicWithBorderModifier();
    [FormerlySerializedAs("Dither")] public GraphicWithDitherModifier dither = new GraphicWithDitherModifier();
    [FormerlySerializedAs("Blend")] public GraphicWithBlendModeModifier blend = new GraphicWithBlendModeModifier();

    public override void Initialize()
    {
        this.AddModifier(new GraphicWithRectModifier());
        this.AddModifier(blend);
        this.AddModifier(mask);
        this.AddModifier(border);
        this.AddModifier(dither);
        base.Initialize();
    }
}

[System.Serializable]
public class GraphicWithBorderModifier : ShaderControlModifier
{
    private static string _borderSizeField = "_BorderSize";
    private static string _borderThicknessField = "_BorderThickness";
    private static string _borderColorField = "_BorderColor";
    private static string _renderBorderOnlyField = "_BorderOnly";

    [FormerlySerializedAs("BorderSize")] [Range(0, 1000)] public int borderSize = 5;
    [FormerlySerializedAs("BorderThickness")] [Range(0, 1000)] public int borderThickness = 0;

    [FormerlySerializedAs("BorderColor")] public Color borderColor = new Color(1, 1, 1, 1);

    [FormerlySerializedAs("RenderBorderOnly")] public bool renderBorderOnly = true;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        shaderControl.SetFloat(_borderSizeField, borderSize);
        shaderControl.SetFloat(_borderThicknessField, borderThickness);
        shaderControl.SetColor(_borderColorField, borderColor);
        shaderControl.SetBool(_renderBorderOnlyField, renderBorderOnly);
    }
}

