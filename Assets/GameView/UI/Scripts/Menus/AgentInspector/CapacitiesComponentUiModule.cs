using Entities.Capacities;
using UI.Components;
using UI.Fields;
using UnityEngine;

namespace UI.Menus
{
    public class CapacitiesComponentUiModule : UiModule
    {
        private const string FloatFormat = "0";

        public CapacitiesComponent CapacitiesComponent { get; set; }

        public DigitInfoField Field1;
        public DigitInfoField Field2;
        public DigitInfoField Field3;
        public DigitInfoField Field4;
        public DigitInfoField Field5;
        public DigitInfoField Field6;

        public void Start()
        {
            Field1.title = CapacityTypes.PainName;
            Field2.title = CapacityTypes.CognitionName;
            Field3.title = CapacityTypes.MovementName;
            Field4.title = CapacityTypes.ManipulationName;
            Field5.title = CapacityTypes.CommunicationName;
            Field6.title = CapacityTypes.HealingName;

            Field1.valueSuffix = "%";
            Field2.valueSuffix = "%";
            Field3.valueSuffix = "%";
            Field4.valueSuffix = "%";
            Field5.valueSuffix = "%";
            Field6.valueSuffix = "%";
        }

        public void OnNeedUpdate()
        {
            var capacities = CapacitiesComponent.capacityInfoCurrent;

            SetFields(new []
                {
                    (100 * capacities.Pain).ToString(FloatFormat),
                    (100 * capacities.Cognition).ToString(FloatFormat),
                    (100 * capacities.Movement).ToString(FloatFormat),
                    (100 * capacities.Manipulation).ToString(FloatFormat),
                    (100 * capacities.Communication).ToString(FloatFormat),
                    (100 * capacities.Healing).ToString(FloatFormat),
                }
            );
        }

        public void SetDefaultView()
        {
            SetFields(new []
                {
                    "100",
                    "100",
                    "100",
                    "100",
                    "100",
                    "100",
                }
            );
        }

        private void SetFields(string[] values)
        {
            Field1.value = values[0];
            Field2.value = values[1];
            Field3.value = values[2];
            Field4.value = values[3];
            Field5.value = values[4];
            Field6.value = values[5];

            UpdateFields();
        }

        private void UpdateFields()
        {
            Field1.UpdateTextFields();
            Field2.UpdateTextFields();
            Field3.UpdateTextFields();
            Field4.UpdateTextFields();
            Field5.UpdateTextFields();
            Field6.UpdateTextFields();
        }
    }
}
