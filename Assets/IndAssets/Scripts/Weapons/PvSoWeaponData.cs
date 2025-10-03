using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Abilities;
using ProjectCI.CoreSystem.Runtime.Animation;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Interfaces;
using UnityEngine;

namespace IndAssets.Scripts.Weapons
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "ProjectCI Tools/Weapon")]
    public class PvSoWeaponData : ScriptableObject, IIdentifier
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
        
        [SerializeField] private int weaponIdentifier;
        [NonSerialized] private string _weaponId = string.Empty;
        
        [SerializeField]
        private PvSoAnimationSupportAsset animator;

        [SerializeField]
        private PvSoUnitAbility bindingAbility;

        public GameObject weaponPrefab;
        public Quaternion prefabLocalRotation;
        public Vector3 prefabLocalPosition;
        
        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(_weaponId))
                {
                    GenerateNewID();
                }
                return _weaponId;
            }
        }

        public void GenerateNewID()
        {
            _weaponId = weaponIdentifier.ToString("X6");
        }

        [Serializable]
        public class AttributePair
        {
            public AttributeType type;
            public float value;
        }

        [Header("Basic Info")]
        public string weaponName;
        public string description;
        
        [Header("Attributes")]
        public List<AttributePair> attributes = new List<AttributePair>();
    }
} 