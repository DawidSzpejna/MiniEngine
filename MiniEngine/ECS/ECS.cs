global using ComponentType = System.Int32;
global using Entity = System.Int32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.EntityComponetSystem
{
    internal class ECS
    {
        // Globally important variables.
        public static readonly Entity MaxEntities = 5000;

        public static readonly ComponentType MaxComponents = 32;
    }

    public class Signature
    {
        private bool[] list;


        #region Constructors
        public Signature() : this(ECS.MaxComponents) { }

        private Signature(int length)
        {
            list = new bool[length];
        }
        #endregion


        #region Functionalities
        public void Set(int index, bool value)
        {
            list[index] = value;
        }

        public void SetAll(bool value)
        {
            for (int i = 0; i < list.Length; i++)
            {
                list[i] = value;
            }
        }

        public static bool AndEquals(Signature baseS, Signature toCompareS)
        {
            for (int i = 0; i < ECS.MaxComponents; i++)
            {
                // this tests the case : ( baseS[i] AND toCompareS[i] ) == baseS[i]
                if (baseS[i] && !toCompareS[i]) return false; 
            }

            return true;
        }

        public bool this[int i]
        {
            get { return list[i]; }
            set { list[i] = value; }
        }
        #endregion


        public override bool Equals(object obj)
        {
            return obj is Signature signature &&
                Enumerable.SequenceEqual(list, signature.list);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(list);
        }
    }
}
