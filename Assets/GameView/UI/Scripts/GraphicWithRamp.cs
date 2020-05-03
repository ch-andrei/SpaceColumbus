using UnityEngine;
using UnityEngine.Serialization;

public class GraphicWithRamp : GraphicShaderControl
{
    public GraphicWithRampModifier ramp = new GraphicWithRampModifier();
    public GraphicWithDitherModifier dither = new GraphicWithDitherModifier();
    public GraphicWithBlendModeModifier blend = new GraphicWithBlendModeModifier();

    protected override void Initialize()
    {
        this.AddModifier(new GraphicWithRectModifier());
        this.AddModifier(blend);
        this.AddModifier(ramp);
        this.AddModifier(dither);
        base.Initialize();
    }
}

[System.Serializable]
public class GraphicWithRampModifier : ShaderControlModifier
{
    private static string _rampPowerField = "_RampPower";
    private static string _rampScaleField = "_RampScale";
    private static string _rampDirectionField = "_RampDirection";
    private static string _radialField = "_Radial";
    private static string _invertField = "_Invert";
    private static string _alphaRampField = "_ApplyAlpha";
    private static string _color1Field = "_Color1";
    private static string _color2Field = "_Color2";

    public Vector2 rampDirection = new Vector2(1, 0);

    public Color color1 = new Color(1, 1, 1, 1);
    public Color color2 = new Color(1, 1, 1, 1);

    [Range(0.01f, 10)] public float power = 1;
    [Range(0.01f, 10)] public float scale = 1;

    public bool radial = true;
    public bool invert = false;
    public bool applyAlpha = true;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        shaderControl.SetColor(_color1Field, color1);
        shaderControl.SetColor(_color2Field, color2);
        shaderControl.SetFloat(_rampPowerField, power);
        shaderControl.SetFloat(_rampScaleField, scale);
        shaderControl.SetBool(_radialField, radial);
        shaderControl.SetBool(_invertField, invert);
        shaderControl.SetBool(_alphaRampField, applyAlpha);
        shaderControl.SetVector(_rampDirectionField, rampDirection.normalized);
    }
}
