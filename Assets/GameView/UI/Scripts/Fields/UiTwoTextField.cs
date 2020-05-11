using System.Collections;
using UnityEngine.UI;

using UI.Components;
using UnityEngine;

namespace UI.Fields
{
    [System.Serializable]
    public class UiTwoTextField : UiModule
    {
        public UiTextField textLeft;
        public UiTextField textRight;

        private LayoutElement _layoutElement;

        private bool _updatingLayoutSize = false;

        public new void Awake()
        {
            base.Awake();

            this._layoutElement = this.GetComponent<LayoutElement>();
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
                    this._layoutElement.preferredHeight + numLines * fontSize);

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
