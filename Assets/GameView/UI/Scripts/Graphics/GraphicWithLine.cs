using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GraphicWithLine : GraphicShaderControl
{
    public GraphicWithMaskModifier mask = new GraphicWithMaskModifier();
    public GraphicWithLineModifier line = new GraphicWithLineModifier();
    public GraphicWithDitherModifier dither = new GraphicWithDitherModifier();
    public GraphicWithBlendModeModifier blend = new GraphicWithBlendModeModifier();

    protected override void Initialize()
    {
        this.AddModifier(new GraphicWithRectModifier());
        this.AddModifier(blend);
        this.AddModifier(mask);
        this.AddModifier(line);
        this.AddModifier(dither);
        base.Initialize();
    }
}

[System.Serializable]
public class GraphicWithLineModifier : ShaderControlModifier
{
    private static string _lineDirectionField = "_LineDirection";
    private static string _lineSizeField = "_LineSize";
    private static string _lineThicknessField = "_LineThickness";
    private static string _lineColorField = "_LineColor";
    private static string _applyRepeatField = "_ApplyRepeat";
    private static string _repeatFrequencyField = "_RepeatFrequency";

    [Range(0, 360)] public float lineAngle = 0;
    [Range(0, 1000)] public int lineSize = 5;
    [Range(0, 1000)] public int lineThickness = 0;

    public Color lineColor = new Color(1, 1, 1, 1);

    public bool applyRepeat = false;
    [Range(0, 1000)] public int repeatDistance = 1;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        float angle = Mathf.Deg2Rad * lineAngle;
        Vector2 lineDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        // equation: a*x + b*y + c = 0, where b = -1
        float a = (Math.Abs(lineDir.x) < 0.001f) ? 100000f: lineDir.y / lineDir.x;
        float c = 0.5f - 0.5f * a;
        float d = Mathf.Sqrt(a * a + c * c); // TODO: remove the sqrt?
        Vector3 lineDir2 = new Vector3(a, c, d);

        shaderControl.SetVector(_lineDirectionField, lineDir2);
        shaderControl.SetFloat(_lineSizeField, lineSize);
        shaderControl.SetFloat(_lineThicknessField, lineThickness);
        shaderControl.SetColor(_lineColorField, lineColor);

        shaderControl.SetBool(_applyRepeatField, applyRepeat);
        shaderControl.SetFloat(_repeatFrequencyField, repeatDistance + 1);
    }
}

