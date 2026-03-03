using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace ProjectCI.CoreSystem.Runtime.UI
{
    public class PvMnUltVisualButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private const string VISUAL_BLOCK_SYMBOL = "UltVisual";
        internal event Action<bool> OnPointerEnterDelegate;
        internal event Action OnPointerClickDelegate;

        [SerializeField]
        private Image content;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnPointerClickDelegate?.Invoke();
        }

        internal void OnUltStatusChanged(bool isOn)
        {
            content.color = isOn ? Color.green : Color.white;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PvMnGameController.SetVisualBlock(true, VISUAL_BLOCK_SYMBOL);
            OnPointerEnterDelegate?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PvMnGameController.SetVisualBlock(false, VISUAL_BLOCK_SYMBOL);
            OnPointerEnterDelegate?.Invoke(false);
        }
    }
}