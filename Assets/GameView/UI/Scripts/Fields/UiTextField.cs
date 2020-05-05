using System;
using System.Collections.Generic;

using UnityEngine.UI;

using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Serialization;

namespace UI.Fields
{
    [System.Serializable]
    public class UiTextField
    {
        public GameObject gameObject;

        private TextMeshProUGUI _textMesh;

        public string defaultText = "Text Field";

        public string Text
        {
            set { this._textMesh.text = value; }
            get { return this._textMesh.text; }
        }

        public float FontSize { get { return this._textMesh.fontSize; } }
        public int NumLines { get { return this._textMesh.textInfo.lineCount; } }

        public void Initialize()
        {
            this._textMesh = this.gameObject.GetComponent<TextMeshProUGUI>();
            this.Text = defaultText;
        }
    }

    [System.Serializable]
    public class UiTwoTextField : MonoBehaviour
    {
        public UiTextField textLeft = new UiTextField();
        public UiTextField textRight = new UiTextField();

        private LayoutElement _layoutElement;

        private bool _updatingLayoutSize = false;

        public virtual void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            this._layoutElement = this.GetComponent<LayoutElement>();

            textLeft.Initialize();
            textRight.Initialize();
        }

        public void TriggerUpdateLayoutSize()
        {
            if (!_updatingLayoutSize)
            {
                try
                {
                    if (this.gameObject.activeSelf)
                    {
                        _updatingLayoutSize = true;
                        StartCoroutine(UpdateLayoutSize());
                    }
                }
                catch (MissingReferenceException) { Debug.Log("Tried to update layout size of an inactive UiTwoTextField."); }
            }
        }

        public IEnumerator UpdateLayoutSize()
        {
            // need to wait until next Frame for textmesh to render once so that NumLines is updated
            yield return new WaitForEndOfFrame();

            // try/catch in case object is destroyed while waiting
            try
            {
                int numLines = Mathf.Max(textLeft.NumLines, textRight.NumLines) - 1;
                float fontSize = Mathf.Max(textLeft.FontSize, textRight.FontSize);
                this._layoutElement.preferredHeight = Mathf.Max(
                    this._layoutElement.minHeight,
                    this._layoutElement.minHeight + numLines * fontSize);

                _updatingLayoutSize = false;
            }
            catch (MissingReferenceException) { Debug.Log("Tried to update layout size of a null UiTwoTextField."); }
        }

        // Update is called once per frame
        void OnValidate()
        {
            Awake();
        }

        private void OnEnable()
        {
            TriggerUpdateLayoutSize();
        }
    }

}
