using UnityEngine;
using UnityEngine.Serialization;

public class GraphicWithGaussianBlur : GraphicShaderControl
{
    public GraphicWithGaussianBlurModifier blur = new GraphicWithGaussianBlurModifier();
    public GraphicWithDitherModifier dither = new GraphicWithDitherModifier();
    public GraphicWithBlendModeModifier blend = new GraphicWithBlendModeModifier();

    protected override void Initialize()
    {
        this.AddModifier(new GraphicWithRectModifier());
        this.AddModifier(blend);
        this.AddModifier(dither);
        this.AddModifier(blur);
        base.Initialize();
    }
}

[System.Serializable]
public class GraphicWithGaussianBlurModifier : ShaderControlModifier
{
    private static string _blurSigmaField = "_GaussBlurSigma";
    private static string _blurSizeField = "_GaussBlurSize";
    private static string _blurSamplesField = "_GaussBlurSamples";
    private static string _useMainTexField = "_Radial";

    [Range(0.1f, 20)] public float blurSigma = 3;
    [Range(0, 10)] public float blurSize = 1;
    [Range(0, 100)] public int blurSamples = 5;
    public bool useMainTexture = true;
    
    public override void ApplyModifier(GraphicShaderControl shaderControl)
    {
        shaderControl.SetFloat(_blurSigmaField, blurSigma);
        shaderControl.SetFloat(_blurSizeField, blurSize);
        shaderControl.SetFloat(_blurSamplesField, blurSamples);
        shaderControl.SetBool(_useMainTexField, useMainTexture);
    }
}
