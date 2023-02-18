using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.EntityComponetSystem
{
    public interface IComponentArray
    {
        public void EntityDestroyed(Entity entity);
    }

    public abstract class BasicComponentArray<T> : IComponentArray
    {
        protected int _mSize;

        protected T[] _mCompomentArray;

        protected Dictionary<Entity, List<int>> _mEntityToIndexMap;

        protected Dictionary<int, Entity> _mIndexToEntityMap;


        #region Constructor
        public BasicComponentArray()
        {
            _mSize = 0;
            _mCompomentArray = new T[ECS.MaxEntities];
            _mEntityToIndexMap = new Dictionary<Entity, List<int>>();
            _mIndexToEntityMap = new Dictionary<int, Entity>();
        }
        #endregion


        #region Funcionalities
        public abstract void GetData(Entity entity, out List<T> data);
        public abstract void SetData(Entity entity, List<T> data);
        public abstract void InsertData(Entity entity, T component);
        public abstract void RemoveData(Entity entity);
        public abstract void EntityDestroyed(Entity entity);
        #endregion


        #region Basic functionalities
        protected void BInsertData(Entity entity, T component)
        {
            //if (_mEntityToIndexMap.ContainsKey(entity)) throw new Exception("Component added to same entity more than once.s");
            if (entity < 0) throw new Exception("Entity's value is incorrect.");
            if (entity > ECS.MaxEntities) throw new Exception("Entity out of range.");

            int newIndex = _mSize;

            if (!_mEntityToIndexMap.ContainsKey(entity))
            {
                _mEntityToIndexMap[entity] = new List<int>();
            }


            _mEntityToIndexMap[entity].Add(newIndex);

            _mIndexToEntityMap[newIndex] = entity;
            _mCompomentArray[newIndex] = component;
            ++_mSize;
        }

        protected void BRemoveData(Entity entity)
        {
            if (!_mEntityToIndexMap.ContainsKey(entity)) throw new Exception("Removing non-existent component.");
            if (entity < 0) throw new Exception("Entity's value is incorrect.");

            List<int> indicesOfRemovedEntity = _mEntityToIndexMap[entity];
            foreach (int indexOfRemovedEntity in indicesOfRemovedEntity)
            {
                //int indexOfRemovedEntity = _mEntityToIndexMap[entity];
                int indexOfLastElement = _mSize - 1;
                _mCompomentArray[indexOfRemovedEntity] = _mCompomentArray[indexOfLastElement];

                Entity entityOfLastElement = _mIndexToEntityMap[indexOfLastElement];
                _mIndexToEntityMap[indexOfRemovedEntity] = entityOfLastElement;
                _mEntityToIndexMap[entityOfLastElement].Remove(indexOfLastElement);
                _mEntityToIndexMap[entityOfLastElement].Add(indexOfRemovedEntity);
                //_mEntityToIndexMap[entityOfLastElement] = indexOfRemovedEntity;


                _mIndexToEntityMap.Remove(indexOfLastElement);
                --_mSize;
            }

            _mEntityToIndexMap.Remove(entity);
        }

        protected void BGetData(Entity entity, out List<T> data)
        {
            if (!_mEntityToIndexMap.ContainsKey(entity)) throw new Exception("Retrieving non-existent component.");
            if (entity < 0) throw new Exception("Entity's value is incorrect.");

            data = new List<T>();
            List<int> compoentsIdices = _mEntityToIndexMap[entity];
            foreach (var idx in compoentsIdices)
            {
                data.Add(_mCompomentArray[(int)idx]);
            }
        }

        public void BSetData(Entity entity, List<T> data)
        {
            if (!_mEntityToIndexMap.ContainsKey(entity)) throw new Exception("Retrieving non-existent component.");
            if (entity < 0) throw new Exception("Entity's value is incorrect.");

            List<int> compoentsIdices = _mEntityToIndexMap[entity];
            for (int i = 0; i < compoentsIdices.Count; i++)
            {
                _mCompomentArray[compoentsIdices[i]] = data[i];
            }
        }
        #endregion
    }

    public class SimpleComponentArray<T> : BasicComponentArray<T>
    {
        #region Funcionalities
        public override void GetData(int entity, out List<T> data) => BGetData(entity, out data);
        public override void SetData(Entity entity, List<T> data) => BSetData(entity, data);
        public override void InsertData(int entity, T component) => BInsertData(entity, component);
        public override void RemoveData(int entity) => BRemoveData(entity);

        public override void EntityDestroyed(Entity entity)
        {
            if (entity < 0) throw new Exception("Entity's value is incorrect.");

            if (_mEntityToIndexMap.ContainsKey(entity))
            {
                RemoveData(entity);
            }
        }
        #endregion
    }

    public class DisposableComponentArray<T> : BasicComponentArray<T> where T : IDisposable
    {
        #region Funcionalities
        public override void GetData(int entity, out List<T> data) => BGetData(entity, out data);
        public override void SetData(Entity entity, List<T> data) => BSetData(entity, data);
        public override void InsertData(int entity, T component) => BInsertData(entity, component);
        public override void RemoveData(int entity)
        {
            if (!_mEntityToIndexMap.ContainsKey(entity)) throw new Exception("Removing non-existent component.");
            if (entity < 0) throw new Exception("Entity's value is incorrect.");

            List<int> indicesOfRemovedEntity = _mEntityToIndexMap[entity];
            foreach (int indexOfRemovedEntity in indicesOfRemovedEntity)
            {
                _mCompomentArray[indexOfRemovedEntity].Dispose();

                //int indexOfRemovedEntity = _mEntityToIndexMap[entity];
                int indexOfLastElement = _mSize - 1;
                _mCompomentArray[indexOfRemovedEntity] = _mCompomentArray[indexOfLastElement];

                Entity entityOfLastElement = _mIndexToEntityMap[indexOfLastElement];
                _mIndexToEntityMap[indexOfRemovedEntity] = entityOfLastElement;
                _mEntityToIndexMap[entityOfLastElement].Remove(indexOfLastElement);
                _mEntityToIndexMap[entityOfLastElement].Add(indexOfRemovedEntity);
                //_mEntityToIndexMap[entityOfLastElement] = indexOfRemovedEntity;


                _mIndexToEntityMap.Remove(indexOfLastElement);
                --_mSize;
            }

            _mEntityToIndexMap.Remove(entity);
        }

        public override void EntityDestroyed(Entity entity)
        {
            if (entity < 0) throw new Exception("Entity's value is incorrect.");

            if (_mEntityToIndexMap.ContainsKey(entity))
            {
                RemoveData(entity);
            }
        }
        #endregion
    }
}
