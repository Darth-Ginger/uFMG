using System.Collections.Generic;
using UnityEngine;
using FantasyMapGenerator.Utilities;


namespace FantasyMapGenerator.Interfaces
{
    public interface ILayer {
        void Initialize(IWorldProvider world); // Inject World reference
        void Generate();              // Generate layer data
        void Clear();                 // Reset layer data
}

    // Example abstract class (optional)
    public abstract class LayerBase : MonoBehaviour, ILayer {
        public string Name { get; private set; }
        protected IWorldProvider World { get; private set; }

        public LayerBase(string name) 
        {
            Name = name;
        }

        public void Initialize(IWorldProvider world) {
            World = world;
        }

        public abstract void Generate();
        public abstract void Clear();
    }
}
