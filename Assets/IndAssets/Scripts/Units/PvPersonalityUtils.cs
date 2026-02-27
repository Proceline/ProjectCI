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
        Style
    }

    [Serializable]
    public struct PvPersonalityRedirectionPair
    {
        public EPvPersonalityName personalityName;
        public AttributeType leftSideAttribute;
        public AttributeType rightSideAttribute;
    }
}
