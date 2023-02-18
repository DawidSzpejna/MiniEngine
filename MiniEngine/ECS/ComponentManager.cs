using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.EntityComponetSystem
{
    internal class ComponentManager
    {
        private Dictionary<string, ComponentType> _mComponentTypes;

        private Dictionary<string, IComponentArray> _mCompomentArrays;

        private ComponentType _mNextComponentType;


        #region Constructor
        public ComponentManager()
        {
            _mComponentTypes = new Dictionary<string, ComponentType>();
            _mCompomentArrays = new Dictionary<string, IComponentArray>();
            _mNextComponentType = 0;
        }
        #endregion


        #region Funcionalities
        public void RegisterSimpleComponent<T>()
        {
            string typeName = typeof(T).Name;

            if (_mComponentTypes.ContainsKey(typeName)) throw new Exception("Registering component type more than once.");

            _mComponentTypes.Add(typeName, _mNextComponentType);
            _mCompomentArrays.Add(typeName, new SimpleComponentArray<T>());
            ++_mNextComponentType;
        }

        public void RegisterDisposableComponent<T>() where T : IDisposable
        {
            string typeName = typeof(T).Name;

            if (_mComponentTypes.ContainsKey(typeName)) throw new Exception("Registering component type more than once.");

            _mComponentTypes.Add(typeName, _mNextComponentType);
            _mCompomentArrays.Add(typeName, new DisposableComponentArray<T>());
            ++_mNextComponentType;
        }

        public ComponentType GetComponentType<T>()
        {
            string typeName = typeof(T).Name;

            if (!_mComponentTypes.ContainsKey(typeName)) throw new Exception("Component not registered before use.");

            return _mComponentTypes[typeName];
        }

        public void AddComponent<T>(Entity entity, T component)
            => GetComponentArray<T>().InsertData(entity, component);
        
        public void RemoveComponent<T>(Entity entity)
            => GetComponentArray<T>().RemoveData(entity);

        public void GetComponents<T>(Entity entity, out List<T> componentList)
            => GetComponentArray<T>().GetData(entity, out componentList);

        public void SetComponents<T>(Entity entity, List<T> componentList)
            => GetComponentArray<T>().SetData(entity, componentList);
        
        public void EntityDestroyed(Entity entity)
        {
            foreach (var pair in _mCompomentArrays)
            {
                pair.Value.EntityDestroyed(entity);
            }
        }
        
        private BasicComponentArray<T> GetComponentArray<T>()
        {
            string typeName = typeof(T).Name;

            if (!_mComponentTypes.ContainsKey(typeName)) throw new Exception("Component not registered before use.");

            return _mCompomentArrays[typeName] as BasicComponentArray<T>;
        }
        #endregion
    }
}
