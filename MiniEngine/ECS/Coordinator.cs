using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.EntityComponetSystem
{
    public class Coordinator
    {
        private ComponentManager _mComponentManager;
        private EntityManager _mEntityManager;
        private ECSystemManager _mECSystemManager;


        #region Constructors
        public Coordinator()
        {
            _mComponentManager = new ComponentManager();
            _mEntityManager = new EntityManager();
            _mECSystemManager = new ECSystemManager();
        }
        #endregion


        #region Functionalities
        public Entity CreateEntity() => _mEntityManager.CreateEntity();

        public void DestroyEntity(Entity entity)
        {
            _mEntityManager.DestroyEntity(entity);
            _mComponentManager.EntityDestroyed(entity);
            _mECSystemManager.EntityDestroyed(entity);
        }

        public void RegisterSimpleComponent<T>()
            => _mComponentManager.RegisterSimpleComponent<T>();

        public void RegisterDisposableComponent<T>() where T : IDisposable
            => _mComponentManager.RegisterDisposableComponent<T>();

        public void AddComponent<T>(Entity entity, T component)
        {
            _mComponentManager.AddComponent<T>(entity, component);

            var signature = _mEntityManager.GetSignature(entity);
            int idx = _mComponentManager.GetComponentType<T>();
            signature.Set(idx, true);
            _mEntityManager.SetSignature(entity, signature);

            _mECSystemManager.EntitySignatureChanged(entity, signature);
        }

        public void RemoveCompoment<T>(Entity entity)
        {
            _mComponentManager.RemoveComponent<T>(entity);

            var signature = _mEntityManager.GetSignature(entity);
            signature.Set((int)_mComponentManager.GetComponentType<T>(), false);
            _mEntityManager.SetSignature(entity,signature);

            _mECSystemManager.EntitySignatureChanged(entity, signature);
        }

        public void GetComponents<T>(Entity entity, out List<T> componentList)
            => _mComponentManager.GetComponents(entity, out componentList);

        public void GetComponent<T>(Entity entity, int index, out T component)
        {
            _mComponentManager.GetComponents(entity, out List<T> componentList);
            component = componentList[index];
        }

        public void SetComponents<T>(Entity entity, List<T> componentList)
            => _mComponentManager.SetComponents<T>(entity, componentList);

        public void SetComponent<T>(Entity entity, int index, T component)
        {
            _mComponentManager.GetComponents(entity, out List<T> componentList);
            componentList[index] = component;
            _mComponentManager.SetComponents<T>(entity, componentList);
        }

        public ComponentType GetComponentType<T>() 
            => _mComponentManager.GetComponentType<T>();

        public T RegisterSystem<T>() where T : ECSystem, new()
        {
            T system = _mECSystemManager.RegisterSystem<T>();
            system.Init(this);

            return system;
        } 

        public void SetSystemSignature<T>(Signature siganture) 
            => _mECSystemManager.SetSignature<T>(siganture);
        #endregion
    }
}
