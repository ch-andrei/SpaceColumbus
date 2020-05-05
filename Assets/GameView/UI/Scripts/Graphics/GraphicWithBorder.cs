using UnityEngine;
using UnityEngine.Serialization;

public class GraphicWithBorder : GraphicShaderControl
{
    public GraphicWithMaskModifier mask = new GraphicWithMaskModifier();
    public GraphicWithBorderModifier border = new GraphicWithBorderModifier();
    public GraphicWithDitherModifier dither = new GraphicWithDitherModifier();
    public GraphicWithBlendModeModifier blend = new GraphicWithBlendModeModifier();

    protected override void Initialize()
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

    [Range(0, 1000)] public int borderSize = 5;
    [Range(0, 1000)] public int borderThickness = 0;

    public Color borderColor = new Color(1, 1, 1, 1);

    public bool renderBorderOnly = true;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        shaderControl.SetFloat(_borderSizeField, borderSize);
        shaderControl.SetFloat(_borderThicknessField, borderThickness);
        shaderControl.SetColor(_borderColorField, borderColor);
        shaderControl.SetBool(_renderBorderOnlyField, renderBorderOnly);
    }
}

