using ProjectCI.CoreSystem.DependencyInjection;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.Utilities.Runtime.Modifiers;
using ProjectCI.Utilities.Runtime.Modifiers.Concrete;
using UnityEngine;
using AttributeModifier = ProjectCI.Utilities.Runtime.Modifiers.AttributeModifier;

namespace ProjectCI.CoreSystem.Runtime.Passives
{
    /// <summary>
    /// This passive will apply on Unit one by one, thus no general binding required
    /// </summary>
    [StaticInjectableTarget]
    [CreateAssetMenu(fileName = "New EnhanceAttribute Passive", menuName = "ProjectCI Passives/EnhanceAttribute", order = 1)]
    public sealed class PvSoPassiveEnhanceAttribute : PvSoPassiveIndividual
    {
        [Header("Parameters"), SerializeField] 
        private AttributeType targetAttribute;

        [Header("Modifier Details"), SerializeField] 
        private AttributeModifier attributeModifier;

        [Inject] private static readonly PvSoModifiersManager ModifiersManager;

        protected override void InstallPassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            ModifiersManager.RegisterModifier(targetAttribute, ModifyAttribute);
        }

        protected override void DisposePassiveGenerally(PvMnBattleGeneralUnit unit)
        {
            ModifiersManager.UnregisterModifier(targetAttribute, ModifyAttribute);
        }

        protected override void InstallPassivePersonally(PvMnBattleGeneralUnit unit)
        {
            // Empty
#if UNITY_EDITOR
            Debug.Log($"Install Passive <{name}> to {unit.name}");
#endif
        }

        protected override void DisposePassivePersonally(PvMnBattleGeneralUnit unit)
        {
            // Empty
#if UNITY_EDITOR
            Debug.Log($"Uninstall Passive <{name}> to {unit.name}");
#endif
        }

        private void ModifyAttribute(IAttributeOwner attributeOwner, IAttributeModifierContainer container)
        {
            if (IsOwner(attributeOwner.ID))
            {
                container.AddModifier(attributeModifier);
            }
        }
    }
}