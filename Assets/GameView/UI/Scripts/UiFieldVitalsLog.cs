using System.Text;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using Entities;
using Entities.Bodies;
using Entities.Bodies.Health;
using Entities.Bodies.Damages;

using Utilities.Events;

using UI.Utils;

namespace UI.Menus
{
    public class UiFieldVitalsLog : UiTwoTextField
    {
        public override void Awake()
        {
            base.Awake();
        }

        public string HpSystemToRichString(HpSystem hpSystem)
        {
            return RichStrings.WithColor($"[{hpSystem.HpCurrent}/{hpSystem.HpBase}]", DamageStates.DamageStateToColor(hpSystem.GetDamageState()));
        }

        public void Initialize(BodyPart bodyPart)
        {
            this.textLeft.Text = bodyPart.NameCustom;
            this.textRight.Text = HpSystemToRichString(bodyPart.HpSystem); // assume BodyPart has hpSystem

            TriggerUpdateLayoutSize();
        }
    }
}