﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.ValueTable
{
    [ContextClass("СтрокаТаблицыЗначений", "ValueTableRow")]
    public class ValueTableRow : DynamicPropertiesAccessor, ICollectionContext
    {
        private Dictionary<IValue, IValue> _data = new Dictionary<IValue, IValue>();
        private WeakReference _owner;

        public ValueTableRow(ValueTable Owner)
        {
            _owner = new WeakReference(Owner);
        }

        public int Count()
        {
            var Owner = _owner.Target as ValueTable;
            return Owner.Columns.Count();
        }

        [ContextMethod("Владелец", "Owner")]
        public ValueTable Owner()
        {
            return _owner.Target as ValueTable;
        }

        private IValue TryValue(ValueTableColumn Column)
        {
            IValue Value;
            if (_data.TryGetValue(Column, out Value))
                return Value;
            return ValueFactory.Create(); // TODO: Определять пустое значение для типа колонки
        }

        [ContextMethod("Получить", "Get")]
        public IValue Get(int index)
        {
            var C = Owner().Columns.FindColumnByIndex(index);
            return TryValue(C);
        }

        public IValue Get(IValue index)
        {
            var C = Owner().Columns.GetColumnByIIndex(index);
            return TryValue(C);
        }

        public IValue Get(ValueTableColumn C)
        {
            return TryValue(C);
        }

        [ContextMethod("Установить", "Set")]
        public void Set(int index, IValue Value)
        {
            var C = Owner().Columns.FindColumnByIndex(index);
            _data[C] = Value;
        }

        public void Set(IValue index, IValue Value)
        {
            var C = Owner().Columns.GetColumnByIIndex(index);
            _data[C] = Value;
        }

        public void Set(ValueTableColumn Column, IValue Value)
        {
            _data[Column] = Value;
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (ValueTableColumn item in Owner().Columns)
            {
                yield return TryValue(item);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public override int FindProperty(string name)
        {
            ValueTableColumn C = Owner().Columns.FindColumnByName(name);
            
            if (C == null)
                throw RuntimeException.PropNotFoundException(name);

            return C.ID;
        }

        public override IValue GetPropValue(int propNum)
        {
            ValueTableColumn C = Owner().Columns.FindColumnById(propNum);
            return TryValue(C);
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            ValueTableColumn C = Owner().Columns.FindColumnById(propNum);
            _data[C] = newVal;
        }

        private ValueTableColumn GetColumnByIIndex(IValue index)
        {
            return Owner().Columns.GetColumnByIIndex(index);
        }

        public override IValue GetIndexedValue(IValue index)
        {
            ValueTableColumn C = GetColumnByIIndex(index);
            return TryValue(C);
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            _data[GetColumnByIIndex(index)] = val;
        }


        private static ContextMethodsMapper<ValueTableRow> _methods = new ContextMethodsMapper<ValueTableRow>();

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var binding = _methods.GetMethod(methodNumber);
            try
            {
                binding(this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var binding = _methods.GetMethod(methodNumber);
            try
            {
                retValue = binding(this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        protected override IEnumerable<KeyValuePair<string, int>> GetProperties()
        {
            return Owner().Columns
                .Select(x=>
                    {
                        var column = x as ValueTableColumn;
                        return new KeyValuePair<string, int>(column.Name, column.ID);
                    });
        }
    }
}
