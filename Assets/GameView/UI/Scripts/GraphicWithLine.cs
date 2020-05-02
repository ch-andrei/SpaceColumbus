using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GraphicWithLine : GraphicShaderControl
{
    [FormerlySerializedAs("Mask")] public GraphicWithMaskModifier mask = new GraphicWithMaskModifier();
    [FormerlySerializedAs("Line")] public GraphicWithLineModifier line = new GraphicWithLineModifier();
    [FormerlySerializedAs("Dither")] public GraphicWithDitherModifier dither = new GraphicWithDitherModifier();
    [FormerlySerializedAs("Blend")] public GraphicWithBlendModeModifier blend = new GraphicWithBlendModeModifier();

    public override void Initialize()
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

    [FormerlySerializedAs("LineAngle")] [Range(0, 360)] public float lineAngle = 0;
    [FormerlySerializedAs("LineSize")] [Range(0, 1000)] public int lineSize = 5;
    [FormerlySerializedAs("LineThickness")] [Range(0, 1000)] public int lineThickness = 0;

    [FormerlySerializedAs("LineColor")] public Color lineColor = new Color(1, 1, 1, 1);

    [FormerlySerializedAs("ApplyRepeat")] public bool applyRepeat = false;
    [FormerlySerializedAs("RepeatDistance")] [Range(0, 1000)] public int repeatDistance = 1;

    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        float angle = Mathf.Deg2Rad * lineAngle;
        Vector2 lineDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        // equation: a*x + b*y + c = 0, where b = -1
        float a = (Math.Abs(lineDir.x) < 0.001f) ? 100000f: lineDir.y / lineDir.x;
        float c = 0.5f - 0.5f * a;
        float d = Mathf.Sqrt(a * a + c * c);
        Vector3 lineDir2 = new Vector3(a, c, d);

        shaderControl.SetVector(_lineDirectionField, lineDir2);
        shaderControl.SetFloat(_lineSizeField, lineSize);
        shaderControl.SetFloat(_lineThicknessField, lineThickness);
        shaderControl.SetColor(_lineColorField, lineColor);

        shaderControl.SetBool(_applyRepeatField, applyRepeat);
        shaderControl.SetFloat(_repeatFrequencyField, repeatDistance + 1);
    }
}

