using System.Collections.Generic;
using UnityEngine;

namespace FantasyMapGenerator.Interfaces
{
    public interface IWorldProvider
    {
        public void            AddLayer    (LayerBase layer);
        public void            RemoveLayer (string name);
        public LayerBase       GetLayer    (string name);
        public List<LayerBase> GetLayers   ();
    }
}