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
    [CreateAssetMenu(fileName = "PvSoImprintWrathLike", menuName = "ProjectCI Imprints/PvSoPassiveImprintWrathLike", order = 1)]
    public class PvSoPassiveImprintBoostAttribute_WrathLike : PvSoPassiveImprintBoostAttribute
    {
        [Header("Extra HitPoint Info"), SerializeField]
        private bool isPercentage = true;

        [SerializeField]
        private bool isRemained;

        [SerializeField]
        private float thresholdForHitpoint = 1f;

        protected override void ModifyAttribute(IAttributeOwner attributeOwner, IAttributeModifierContainer container)
        {
            if (IsOwner(attributeOwner.ID))
            {
                var currentHp = attributeOwner.GetCurrentHitPoint();
                var maximumHp = attributeOwner.GetMaximumHitPoint();
                var boostAmount = 0;
                if (isRemained)
                {
                    boostAmount = isPercentage ? currentHp * 100 / maximumHp : currentHp;
                }
                else
                {
                    boostAmount = isPercentage ? 100 - (currentHp * 100 / maximumHp) : maximumHp - currentHp;
                }

                var maxBoostAmount = Mathf.RoundToInt(attributeOwner.GetAttributeValue(thresholdPersonality.Value) * valueThreshold) + flatBasicAddon;
                var realBoostAmount = Mathf.RoundToInt(boostAmount * thresholdForHitpoint);

                if (realBoostAmount > maxBoostAmount)
                {
                    realBoostAmount = maxBoostAmount;
                }

                var modifier = new AttributeModifier
                {
                    flatValue = realBoostAmount
                };

                container.AddModifier(modifier);
            }
        }
    }
}