using System;
using UnityEngine;
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
        }
    }
}