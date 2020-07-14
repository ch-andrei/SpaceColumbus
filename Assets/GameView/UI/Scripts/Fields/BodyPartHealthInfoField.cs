using Entities.Bodies;
using Entities.Damageables;

using UI.Utils;
using UI.Components;

namespace UI.Fields
{
    public class BodyPartHealthInfoField : UiTwoTextField
    {
        public string HpSystemToRichString(HpSystem hpSystem)
        {
            return RichStrings.WithColor(
                $"[{hpSystem.HpCurrent}/{hpSystem.HpBase}]",
                DamageStates.DamageStateToColor(hpSystem.GetDamageState()));
        }

        public void Initialize(BodyPart bodyPart)
        {
            this.textLeft.Text = bodyPart.NameCustom;
            this.textRight.Text = HpSystemToRichString(bodyPart.HpSystem); // assume BodyPart has hpSystem

            TriggerUpdateLayoutSize();
        }
    }
}
