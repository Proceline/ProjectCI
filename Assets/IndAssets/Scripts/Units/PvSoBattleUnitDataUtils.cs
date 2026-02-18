using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Localization.Settings;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete
{
    public static class PvSoBattleUnitDataUtils
    {
        private static readonly Dictionary<string, string> _classNamesCache = new();
        private static readonly Dictionary<string, string> _unitNamesCache = new();
        private const string TABLE_FOR_ALL_CLASS = "AllClass";
        private const string TABLE_FOR_ALL_CHARACTERS = "CharacterNames";

        static PvSoBattleUnitDataUtils()
        {
            LocalizationSettings.SelectedLocaleChanged += 
                _ => 
                { 
                    _classNamesCache.Clear();
                    _unitNamesCache.Clear();
                };
        }

        public static string GetClassName(this SoUnitData data)
        {
            var classKey = data.m_UnitClass;

            if (string.IsNullOrEmpty(classKey))
                return "N/A";

            if (_classNamesCache.TryGetValue(classKey, out string cachedName))
            {
                return cachedName;
            }

            var translatedName = LocalizationSettings.StringDatabase.GetLocalizedString(TABLE_FOR_ALL_CLASS, classKey);
            _classNamesCache[classKey] = translatedName;
            return translatedName;
        }

        public static string GetCharacterName(this SoUnitData data)
        {
            var unitNameKey = data.m_UnitName;

            if (string.IsNullOrEmpty(unitNameKey))
                return "N/A";

            if (_unitNamesCache.TryGetValue(unitNameKey, out string cachedName))
            {
                return cachedName;
            }

            var translatedName = LocalizationSettings.StringDatabase.GetLocalizedString(TABLE_FOR_ALL_CHARACTERS, unitNameKey);
            _unitNamesCache[unitNameKey] = translatedName;
            return translatedName;
        }
    }
}