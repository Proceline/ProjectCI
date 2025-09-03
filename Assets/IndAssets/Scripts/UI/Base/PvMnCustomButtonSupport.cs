using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectCI.Runtime.GUI.Battle
{
    public class PvMnCustomButtonSupport : MonoBehaviour
    {
        private Button _button;

        [SerializeField] 
        protected Text buttonContentText;

        public int ButtonIndex { get; internal set; }

        internal Action<int> OnButtonClickedAsIndex;
        public UnityEvent<int> onButtonClickedAsIndexPostEvent;
        public UnityEvent<int> onButtonHoveredAsIndexPostEvent;

        protected internal Button Button
        {
            get
            {
                if (!_button)
                {
                    _button = TryGetComponent<Button>(out var button) ? button : GetComponentInChildren<Button>();
                }

                return _button;
            }
        }

        public string ButtonContentText
        {
            get => buttonContentText.text;
            set => buttonContentText.text = value;
        }

        /// <summary>
        /// Normally assigned to Button Component
        /// </summary>
        public void ApplyOnButtonClickedWithIndex()
        {
            OnButtonClickedAsIndex?.Invoke(ButtonIndex);
            onButtonClickedAsIndexPostEvent.Invoke(ButtonIndex);
        }
        
        /// <summary>
        /// Normally assigned to Hover Event Trigger
        /// </summary>
        public void ApplyOnButtonHoveredWithIndex()
        {
            onButtonHoveredAsIndexPostEvent.Invoke(ButtonIndex);
        }
    }
}