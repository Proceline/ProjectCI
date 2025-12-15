using System;
using System.Collections.Generic;
using ProjectCI_Animation.Runtime;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Animation;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Interfaces;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Saving.Interfaces;

namespace IndAssets.Scripts.Weapons
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "ProjectCI Tools/Weapon")]
    public class PvSoWeaponData : ScriptableObject, IPvSaveEntry
    {
#if UNITY_EDITOR
        
        /// <summary>
        /// Cannot be accessed through Runtime
        /// </summary>
        public static string AnimatorPropertyName => nameof(animator);
        
        /// <summary>
        /// Cannot be accessed through Runtime
        /// </summary>
        public static string BindingAbilityPropertyName => nameof(bindingAbility);
#endif
        
        [SerializeField]
        private PvSoAnimationSupportAsset animator;

        [SerializeField]
        private PvSoUnitAbility bindingAbility;

        public GameObject weaponPrefab;
        public Quaternion prefabLocalRotation;
        public Vector3 prefabLocalPosition;
        public Vector3 prefabLocalScale = Vector3.one;

        public AnimationPlayableSupportBase Animator => animator;
        public PvSoUnitAbility DefaultAttackAbility => bindingAbility;
        
        public string EntryId => name;

        [Serializable]
        public class AttributePair
        {
            public AttributeType type;
            public int value;
        }

        [Header("Basic Info")]
        public string weaponName;
        public string description;
        
        [Header("Attributes")]
        public List<AttributePair> attributes = new List<AttributePair>();
    }
} 