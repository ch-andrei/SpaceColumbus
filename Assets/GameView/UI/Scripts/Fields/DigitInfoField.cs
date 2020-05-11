using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Entities;
using UI.Menus;
using UnityEngine.Serialization;
using Utilities.Events;

namespace UI.Fields
{
    public class DigitInfoField : MonoBehaviour
    {
        private static string SizeRichText(float ratio) { return $"<size={(int) (ratio * 100 % 101)}%>"; }

        public GameObject titleText;
        public GameObject infoText;

        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _infoText;

        public string title;
        public string value;
        public string valueSuffix;
        [Range(0.1f, 1)] public float valueSuffixScale = 0.5f;

        // Start is called before the first frame update
        void Start()
        {
            _titleText = titleText.GetComponent<TextMeshProUGUI>();
            _infoText = infoText.GetComponent<TextMeshProUGUI>();

            UpdateTextFields();
        }

        public void UpdateTextFields()
        {
            _titleText.text = title;
            _infoText.text = value + SizeRichText(valueSuffixScale) + valueSuffix;
        }

        // Update is called once per frame
        void OnValidate()
        {
            Start();
        }
    }
}
