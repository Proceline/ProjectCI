using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectCI.Runtime.Utilities
{
    public class PvSimpleObjectPool<T> where T : Object
    {
        private readonly List<T> _poolObjects = new();
        private readonly Func<T> _poolObjectSpawner;

        public PvSimpleObjectPool(Func<T> spawner)
        {
            _poolObjectSpawner = spawner;
        }
        
        // TODO: Finish this Pool
    }

    public class PvSimpleWidgetsContainer<T> where T : MonoBehaviour
    {
        private readonly List<T> _widgetsCollection = new();

        public T GetWidget(int index) => _widgetsCollection[index];

        public void DoSomethingForEach(Action<T> action)
        {
            _widgetsCollection.ForEach(action.Invoke);
        }
        
        public void TrimOrExtendCollection(T origin, Transform parent, int requiredCount)
        {
            while (_widgetsCollection.Count < requiredCount)
            {
                var newObject = Object.Instantiate(origin, parent);
                _widgetsCollection.Add(newObject);
            }

            for (int i = requiredCount; i < _widgetsCollection.Count; i++)
            {
                _widgetsCollection[i].gameObject.SetActive(false);
            }
        }
    }
}
