using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public abstract class GraphicShaderControl : MonoBehaviour
{
    public Material material;

    protected Image Graphic;

    List<ShaderControlModifier> _modifiers;

    protected virtual void Awake()
    {
        this._modifiers = new List<ShaderControlModifier>();
    }

    protected void AddModifier(ShaderControlModifier modifier)
    {
        this._modifiers.Add(modifier);
    }

    protected virtual void Start()
    {
        Initialize();
    }

    protected virtual void OnValidate()
    {
        Awake();
        Start();
    }

    protected virtual void Initialize()
    {
        if (material == null)
            Debug.Log($"{this.name} has no Material assigned.");

        Graphic = this.GetComponent<Image>();
        Graphic.material = Instantiate(material);

        //Debug.Log("Shader control has " + modifiers.Count + " modifiers.");
        foreach (var modifier in _modifiers)
            modifier.ApplyModifier(this);
    }

    public void SetInt(string name, int value) { Graphic.material.SetInt(name, value); }
    public void SetFloat(string name, float value) { Graphic.material.SetFloat(name, value); }
    public void SetTexture(string name, Texture value) { Graphic.material.SetTexture(name, value); }
    public void SetColor(string name, Color value) { Graphic.material.SetColor(name, value); }
    public void SetBool(string name, bool value) { SetFloat(name, value ? 1 : 0); }
    public void SetVector(string name, Vector4 value) { Graphic.material.SetVector(name, value); }
    public void SetVector(string name, Vector3 value) { SetVector(name, new Vector4(value.x, value.y, value.z, 0)); }
    public void SetVector(string name, Vector2 value) { SetVector(name, new Vector4(value.x, value.y, 0, 0)); }
}

// decorator
[System.Serializable]
public abstract class ShaderControlModifier
{
    public abstract void ApplyModifier(GraphicShaderControl shaderControl);
}
