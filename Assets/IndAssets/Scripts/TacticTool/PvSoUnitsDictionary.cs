using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndAssets.Scripts.TacticTool
{
    [CreateAssetMenu(fileName = "UnitsDictionaryCenter", menuName = "ProjectCI Tools/GameRules/Create UnitsDictionaryCenter", order = 1)]
    public class PvSoUnitsDictionary : ScriptableObject, IDictionary<string, PvMnBattleGeneralUnit>
    {
        private readonly Dictionary<string, PvMnBattleGeneralUnit> _innerCollection = new();

        public PvMnBattleGeneralUnit this[string key] 
        {
            get => _innerCollection[key];
            set => _innerCollection[key] = value;
        }

        public ICollection<string> Keys => _innerCollection.Keys;

        public ICollection<PvMnBattleGeneralUnit> Values => _innerCollection.Values;

        public int Count => _innerCollection.Count;

        public bool IsReadOnly => true;

        public void Add(string key, PvMnBattleGeneralUnit value)
        {
            _innerCollection.Add(key, value);
        }

        public void Add(KeyValuePair<string, PvMnBattleGeneralUnit> item)
        {
            _innerCollection.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _innerCollection.Clear();
        }

        public bool Contains(KeyValuePair<string, PvMnBattleGeneralUnit> item)
        {
            var isKeyCorrect = _innerCollection.ContainsKey(item.Key);
            var isValueCorrect = _innerCollection[item.Key] == item.Value;
            return isKeyCorrect && isValueCorrect;
        }

        public bool ContainsKey(string key) => _innerCollection.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, PvMnBattleGeneralUnit>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, PvMnBattleGeneralUnit>> GetEnumerator()
        {
            return _innerCollection.GetEnumerator();
        }

        public bool Remove(string key) => _innerCollection.Remove(key);

        public bool Remove(KeyValuePair<string, PvMnBattleGeneralUnit> item) => _innerCollection.Remove(item.Key);

        public bool TryGetValue(string key, out PvMnBattleGeneralUnit value)
        {
            return _innerCollection.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() => _innerCollection.GetEnumerator();
    }
}
