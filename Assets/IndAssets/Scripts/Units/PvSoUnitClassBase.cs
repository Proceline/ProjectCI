using ProjectCI.CoreSystem.Runtime.Interfaces;
using UnityEngine;
using System;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Classes
{
    public abstract class PvSoUnitClassBase : ScriptableObject, IIdentifier
    {
        [SerializeField]
        private string classId = string.Empty;

        public string ID => classId;
        
        /// <summary>
        /// Not required to be implemented in this class
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void GenerateNewID()
        {
            throw new NotImplementedException("Exception: Class Id should be set through Scriptable Object Asset");
        }
    }
}