using System;
using System.Collections.Generic;

using UnityEngine.UI;

using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Serialization;

namespace UI.Components
{
    [System.Serializable]
    public class UiTextField : UiModule
    {
        private TextMeshProUGUI _textMesh;

        public string Text
        {
            set { this._textMesh.text = value; }
            get { return this._textMesh.text; }
        }

        public float FontSize { get { return this._textMesh.fontSize; } }
        public int NumLines { get { return this._textMesh.textInfo.lineCount; } }

        public new void Awake()
        {
            base.Awake();

            this._textMesh = this.GetComponent<TextMeshProUGUI>();
            this.Text = label;
        }

        public void OnValidate()
        {
            Awake();
        }
    }
}
