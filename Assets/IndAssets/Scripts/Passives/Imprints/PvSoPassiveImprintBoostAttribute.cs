using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Modifiers;
using ProjectCI.Utilities.Runtime.Modifiers.Concrete;
using UnityEngine;

namespace IndAssets.Scripts.Passives.Imprints
{
    [CreateAssetMenu(fileName = "New EnhanceAttribute Imprint", menuName = "ProjectCI Imprints/PvSoPassiveImprintBoostAttribute", order = 1)]
    [StaticInjectableTarget]
    public class PvSoPassiveImprintBoostAttribute : PvSoPassiveImprint
    {
        [Header("Boost Target"), SerializeField]
        private AttributeType boostAttribute;

        [Header("Related Personality"), SerializeField]
        protected AttributeType thresholdPersonality;
        [SerializeField] protected float valueThreshold = 1f;
        [SerializeField] protected int flatBasicAddon = 0;

        [Inject] protected static readonly PvSoModifiersManager ModifiersManager;

        protected override void InstallPassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            ModifiersManager.RegisterModifier(boostAttribute, ModifyAttribute);
        }

        protected override void DisposePassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            ModifiersManager.UnregisterModifier(boostAttribute, ModifyAttribute);
        }

        protected virtual void ModifyAttribute(IAttributeOwner attributeOwner, IAttributeModifierContainer container)
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

        protected override void InstallPassivePersonally(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }

        protected override void DisposePassivePersonally(PvMnBattleGeneralUnit unit)
        {
            // Empty
        }
    }
}