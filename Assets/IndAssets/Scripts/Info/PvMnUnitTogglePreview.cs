using IndAssets.Scripts.Units;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace IndAssets.Scripts.Info
{
    public class PvMnUnitTogglePreview : MonoBehaviour
    {
        [SerializeField] private PvMnUnitMeshPreview preview;
        [SerializeField] private TextMeshProUGUI selfNameText;
        [SerializeField] private bool updateOnStart = true;

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI classText;
        [SerializeField] private Slider[] sliders = new Slider[4];

        [SerializeField] private TextMeshProUGUI passiveNameText;
        [SerializeField] private TextMeshProUGUI passiveDescText;

        private Button _button;
        private readonly List<TextMeshProUGUI[]> _personalitySideSymbols = new();

        private const float FadeValue = 0.4f;

        void Start()
        {
            _button = GetComponentInChildren<Button>();
            foreach (var slider in sliders)
            {
                if (slider)
                {
                    var sideSymbols = slider.GetComponentsInChildren<TextMeshProUGUI>();
                    _personalitySideSymbols.Add(sideSymbols);
                }
            }

            if (updateOnStart)
            {
                var unitData = preview.UnitData;
                UpdateSelfPanel();
            }
        }

        private void UpdateSelfPanel()
        {
            var unitData = preview.UnitData;
            
            if (unitData)
            {
                selfNameText.text = unitData.GetCharacterName();
                _button.interactable = true;
            }
            else
            {
                selfNameText.text = "UNKNOWN";
                _button.interactable = false;
            }
        }

        public void UpdateTargetPanel()
        {
            var unitData = preview.UnitData;

            if (unitData)
            {
                nameText.text = selfNameText.text;
                classText.text = unitData.GetClassName();
                UpdateSlider(sliders[0], 0, unitData.GetPersonalityLevel(EPvPersonalityName.Energy));
                UpdateSlider(sliders[1], 1, unitData.GetPersonalityLevel(EPvPersonalityName.Information));
                UpdateSlider(sliders[2], 2, unitData.GetPersonalityLevel(EPvPersonalityName.Decisions));
                UpdateSlider(sliders[3], 3, unitData.GetPersonalityLevel(EPvPersonalityName.Style));

                passiveNameText.text = unitData.GetPersonalitySpecialDescription(out var desc);
                passiveDescText.text = desc;
            }
        }

        private void UpdateSlider(Slider slider, int index, int value)
        {
            var symbols = _personalitySideSymbols[index];
            if (symbols.Length > 1)
            {
                if (value == 0)
                {
                    symbols[0].alpha = 1;
                    symbols[1].alpha = 1;
                }
                else if (value > 0)
                {
                    symbols[0].alpha = FadeValue;
                    symbols[1].alpha = 1;
                }
                else
                {
                    symbols[0].alpha = 1;
                    symbols[1].alpha = FadeValue;
                }
            }

            slider.value = value;
        }
    }
}
