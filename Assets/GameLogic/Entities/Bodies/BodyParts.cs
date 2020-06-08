using System;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Common;

using Entities.Health;

using Utilities.Misc;
using Utilities.Events;

namespace Entities.Bodies
{
    // public class BodyPartEvent : DamageableEvent
    // {
    //     public BodyPart bodyPart { get; private set; }
    //     public HpSystemEvent hpSystemEvent { get; private set; }
    //
    //     public BodyPartEvent(BodyPart bodyPart, HpSystemEvent hpSystemEvent) : base(hpSystemEvent.Damages)
    //     {
    //         this.bodyPart = bodyPart;
    //         this.hpSystemEvent = hpSystemEvent;
    //     }
    // }

    // public class BodyPartEventGenerator :
    //     EventGenerator<BodyPartEvent>,
    //     IEventListener<HpSystemEvent>, // from HpSystem
    //     IEventListener<BodyPartEvent> // from other BodyParts
    // {
    //     BodyPart _bodyPart;
    //
    //     public BodyPartEventGenerator(BodyPart bodyPart) : base() { this._bodyPart = bodyPart; }
    //
    //     // from the bodypart's own HP system
    //     public bool OnEvent(HpSystemEvent hpSystemEvent)
    //     {
    //         StringBuilder sb = new StringBuilder();
    //         sb.Append(
    //             $"{_bodyPart.NameCustom} HpSystemEvent: {hpSystemEvent.HpSystem.HpPrev}->{hpSystemEvent.HpSystem.HpCurrent}HP:");
    //         foreach (var damage in hpSystemEvent.Damages)
    //             sb.Append(
    //                 $"\t{Damages.DamageType2Str(damage.DamageType)} damage with {damage.Amount} total damage amount;");
    //         Debug.Log(sb.ToString());
    //
    //         // for its BodyPartContainer or external system, e.g. UI
    //         Notify(new BodyPartEvent(this._bodyPart, hpSystemEvent));
    //
    //         return true;
    //     }
    //
    //     // from other bodyparts
    //     public bool OnEvent(BodyPartEvent bodyPartEvent)
    //     {
    //         // forward this event
    //         Notify(bodyPartEvent);
    //
    //         return true;
    //     }
    // }


}
