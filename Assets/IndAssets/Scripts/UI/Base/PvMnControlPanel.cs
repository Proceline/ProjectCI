using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.Runtime.GUI.Battle
{
    public abstract class PvMnControlPanel : MonoBehaviour
    {
        public abstract List<PvMnCustomButtonSupport> ControlButtons { get; }
        public abstract int NumOfSlots { get; set; }

        [SerializeField] 
        protected RectTransform buttonsContainer;
    }
}
