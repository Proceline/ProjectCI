using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.Interfaces;
using System;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Weapons
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "ProjectCI/Weapon")]
    public class SoWeaponData : ScriptableObject, IIdentifier
    {
        [SerializeField] private int m_WeaponNumberIdentifier;
        [NonSerialized] private string m_WeaponID = string.Empty;
        
        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(m_WeaponID))
                {
                    GenerateNewID();
                }
                return m_WeaponID;
            }
        }

        public void GenerateNewID()
        {
            m_WeaponID = m_WeaponNumberIdentifier.ToString("X6");
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