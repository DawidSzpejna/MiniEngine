using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MiniEngine.EntityComponetSystem
{
    internal class EntityManager
    {
        // Fields of the class.
        private Queue<Entity> _mAvailableEntities;

        private List<Signature> _mSignatures;

        private System.Int32 _mLivingEntityCount;


        #region Constructors
        public EntityManager()
        {
            _mLivingEntityCount = 0;
            _mAvailableEntities = new Queue<Entity>();
            _mSignatures = new List<Signature>(ECS.MaxEntities);

            FullQueueWithEntities();
            FillArrayWithEmptySignatures();
        }

        private void FullQueueWithEntities()
        {
            for (Entity entity = 0; entity < ECS.MaxEntities; entity++)
            {
                _mAvailableEntities.Enqueue(entity);
            }
        }

        private void FillArrayWithEmptySignatures()
        {
            for (int signature = 0; signature < ECS.MaxEntities; signature++)
            {
                _mSignatures.Add(new Signature());
            }
        }
        #endregion


        #region Functionalities
        public Entity CreateEntity()
        {
            if (_mLivingEntityCount > ECS.MaxEntities) throw new Exception("Too many entities in existence.");

            Entity id = _mAvailableEntities.Dequeue();
            ++_mLivingEntityCount;

            return id;
        }

        public void DestroyEntity(Entity entity)
        {
            if (entity >= ECS.MaxEntities || entity < 0) throw new Exception("Entity out of range.");
            _mSignatures[entity].SetAll(false);

            _mAvailableEntities.Enqueue(entity);
        }

        public void SetSignature(Entity entity, Signature signature)
        {
            if (entity >= ECS.MaxEntities || entity < 0) throw new Exception("Entity out of range.");

            _mSignatures[entity] = signature;
        }

        public Signature GetSignature(Entity entity)
        {
            if (entity >= ECS.MaxEntities || entity < 0) throw new Exception("Entity out of range.");

            return _mSignatures[entity];
        }
        #endregion
    }
}
