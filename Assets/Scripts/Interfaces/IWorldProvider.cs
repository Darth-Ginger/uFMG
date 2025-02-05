using System.Collections.Generic;
using UnityEngine;

namespace FantasyMapGenerator.Interfaces
{
    public interface IWorldProvider
    {
        public void         AddLayer    (ILayer layer);
        public void         RemoveLayer (string name);
        public ILayer       GetLayer    (string name);
        public List<ILayer> GetLayers   ();
    }
}