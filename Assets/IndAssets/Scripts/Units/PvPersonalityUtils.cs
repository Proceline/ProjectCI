using System;
using ProjectCI.CoreSystem.Runtime.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace IndAssets.Scripts.Units
{
    public enum EPvPersonalityName
    {
        Energy,
        Information,
        Decisions,
        Style,
        Identity
    }

    [Serializable]
    public class PvPersonalitiesCombination
    {
        [Header("I <---> E")]
        [Range(-5, 5)] public int energyLevel;
        [Header("N <---> S")]
        [Range(-5, 5)] public int informationLevel;
        [Header("F <---> T")]
        [Range(-5, 5)] public int decisionLevel;
        [Header("P <---> J")]
        [Range(-5, 5)] public int styleLevel;
        [Header("A <---> T")]
        [Range(-5, 5)] public int identityLevel;

        public int GetBasicLevel(EPvPersonalityName element)
        {
            switch (element)
            {
                case EPvPersonalityName.Energy:
                    return energyLevel;
                case EPvPersonalityName.Information:
                    return informationLevel;
                case EPvPersonalityName.Decisions:
                    return decisionLevel;
                case EPvPersonalityName.Style:
                    return styleLevel;
                case EPvPersonalityName.Identity:
                    return identityLevel;
                default:
                    return 0;
            }
        }
    }

    [Serializable]
    public struct PvPersonalityRedirectionPair
    {
        public EPvPersonalityName personalityName;
        public AttributeType redirectToAttribute;
    }
}
