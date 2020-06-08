using Entities.Bodies;
using Entities.Health;

using UI.Utils;
using UI.Components;

namespace UI.Fields
{
    public class UiFieldVitalsLog : UiTwoTextField
    {
        public string HpSystemToRichString(HpSystem hpSystem)
        {
            return RichStrings.WithColor(
                $"[{hpSystem.HpCurrent}/{hpSystem.HpBase}]",
                HpSystemDamageStates.DamageStateToColor(hpSystem.GetDamageState()));
        }

        public void Initialize(BodyPart bodyPart)
        {
            this.textLeft.Text = bodyPart.NameCustom;
            this.textRight.Text = HpSystemToRichString(bodyPart.HpSystem); // assume BodyPart has hpSystem

            TriggerUpdateLayoutSize();
        }
    }
}
