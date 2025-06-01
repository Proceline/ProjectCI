using System;
using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.TacticTool.Formula.Concrete;
using UnityEngine;

namespace ProjectCI.TacticTool.Formula
{
    /// <summary>
    /// A dictionary that calculates its values dynamically through a single calculator function
    /// </summary>
    public class FormulaAttributeDictionary : IDictionary<AttributeType, int>
    {
        private Func<AttributeType, int> _calculator;
        private Func<AttributeType, bool> _isRegistered;

        public FormulaAttributeDictionary(FormulaAttributeContainer formulaAttributeContainer)
        {
            _calculator = formulaAttributeContainer.GetAttributeValue;
            _isRegistered = formulaAttributeContainer.IsRegistered;
        }

        private int GetCalculatedValue(AttributeType type)
        {
            if (_calculator != null && _isRegistered(type))
            {
                return _calculator.Invoke(type);
            }

            return 0;
        }

        #region IDictionary Implementation
        public int this[AttributeType key]
        {
            get => GetCalculatedValue(key);
            set => throw new NotSupportedException("Direct value assignment is not supported. Use SetCalculator and RegisterType instead.");
        }

        /// <summary>
        /// Keys collection is not supported for read only dictionary.
        /// </summary>
        public ICollection<AttributeType> Keys => throw new NotSupportedException("Keys collection is not supported for read only dictionary.");

        /// <summary>
        /// Values collection is not supported for read only dictionary.
        /// </summary>
        public ICollection<int> Values => throw new NotSupportedException("Values collection is not supported for read only dictionary.");

        /// <summary>
        /// Count is not supported for read only dictionary.
        /// </summary>
        public int Count => throw new NotSupportedException("Count is not supported for read only dictionary.");

        public bool IsReadOnly => true;

        /// <summary>
        /// Direct addition is not supported. Use RegisterType instead.
        /// </summary>
        public void Add(AttributeType key, int value)
        {
            throw new NotSupportedException("Direct addition is not supported.");
        }

        /// <summary>
        /// Direct addition is not supported. Use RegisterType instead.
        /// </summary>
        public void Add(KeyValuePair<AttributeType, int> item)
        {
            throw new NotSupportedException("Direct addition is not supported.");
        }

        /// <summary>
        /// Clear is not supported.
        /// </summary>
        public void Clear()
        {
            throw new NotSupportedException("Clear is not supported.");
        }

        /// <summary>
        /// Contains is not supported.
        /// </summary>
        public bool Contains(KeyValuePair<AttributeType, int> item)
        {
            throw new NotSupportedException("Contains is not supported.");
        }
        
        public bool ContainsKey(AttributeType key)
        {
            return _isRegistered.Invoke(key);
        }

        public void CopyTo(KeyValuePair<AttributeType, int>[] array, int arrayIndex)
        {
            throw new NotSupportedException("CopyTo is not supported for dynamic calculation.");
        }

        public IEnumerator<KeyValuePair<AttributeType, int>> GetEnumerator()
        {
            throw new NotSupportedException("GetEnumerator is not supported for dynamic calculation.");
        }

        public bool Remove(AttributeType key)
        {
            throw new NotSupportedException("Remove is not supported for dynamic calculation.");
        }

        public bool Remove(KeyValuePair<AttributeType, int> item)
        {
            throw new NotSupportedException("Remove is not supported for dynamic calculation.");
        }

        public bool TryGetValue(AttributeType key, out int value)
        {
            value = GetCalculatedValue(key);
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException("GetEnumerator is not supported for dynamic calculation.");
        }
        #endregion
    }
} 