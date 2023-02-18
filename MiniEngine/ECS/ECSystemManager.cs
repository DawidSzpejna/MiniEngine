using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.EntityComponetSystem
{
    public abstract class ECSystem
    {
        public HashSet<Entity> Entities { get; set; }

        public ECSystem()
        {
            Entities = new HashSet<Entity>();
        }

        public abstract void Init(Coordinator coordinator);
    }

    internal class ECSystemManager
    {
        private Dictionary<string, Signature> _mSignatures;

        private Dictionary<string, ECSystem> _mECSystems;

        #region Constructors
        public ECSystemManager()
        {
            _mSignatures = new Dictionary<string, Signature>();
            _mECSystems = new Dictionary<string, ECSystem>();
        }
        #endregion

        #region Funcionalities
        public T RegisterSystem<T>()
            where T : ECSystem, new()
        {
            string typeName = typeof(T).Name;

            if (_mECSystems.ContainsKey(typeName)) throw new Exception("Registering system more than once.");

            var ecSystem = new T();
            _mECSystems.Add(typeName, ecSystem);
            return ecSystem;
        }

        public void SetSignature<T>(Signature signature)
        {
            string typeName = typeof(T).Name;

            if (!_mECSystems.ContainsKey(typeName)) throw new Exception("System used before registered.");

            _mSignatures.Add(typeName, signature);
        }
        #endregion

        #region For entities
        public void EntityDestroyed(Entity entity)
        {
            foreach (var pair in _mECSystems)
            {
                ECSystem sys = pair.Value;
                sys.Entities.Remove(entity);
            }
        }

        public void EntitySignatureChanged(Entity entity, Signature entitySignature)
        {
            foreach (var pair in _mECSystems)
            {

                var type = pair.Key;
                var sys = pair.Value;
                var systemSignature = _mSignatures[type];

                if (Signature.AndEquals(systemSignature, entitySignature))
                {
                    sys.Entities.Add(entity);
                }
                else
                {
                    sys.Entities.Remove(entity);
                }
            }
        }
        #endregion
    }
}
