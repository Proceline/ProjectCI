using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Modifiers;
using UnityEngine;
using UnityEngine.Events;

namespace IndAssets.Scripts.Passives.Imprints
{
    /// <summary>
    /// Only used for Attribute Modifier, thus, no general effect, only personal effect.
    /// The events are designed to be used for PvSoModifierSupportObject, 
    /// which will be added to the unit personally when the relic is settled, and removed when the relic is withdrawn.
    /// </summary>
    [CreateAssetMenu(fileName = "SoRelic_SimpleEventTBD", menuName = "ProjectCI Passives/Relics/SimpleEvent", order = 1)]
    public class PvSoPassiveImprintBoostAttribute_WrathLike : PvSoPassiveImprintBoostAttribute
    {
        protected override void ModifyAttribute(IAttributeOwner attributeOwner, IAttributeModifierContainer container)
        {
            if (IsOwner(attributeOwner.ID))
            {
                var thresholdPersonalityValue = Mathf.RoundToInt(attributeOwner.GetAttributeValue(thresholdPersonality.Value) * valueThreshold);
                var modifier = new AttributeModifier
                {
                    flatValue = thresholdPersonalityValue + flatBasicAddon
                };
                container.AddModifier(modifier);
            }
        }
    }
}