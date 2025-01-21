using System.Collections.Generic;
using UnityEngine;

namespace FantasyMapGenerator.Interfaces
{
    public interface IWorldDataProvider
    {
        Vector2Int GetWorldSize();
        int GetSeed();
        string GetWorldName();
        Dictionary<string, IMap> GetMaps();
    }
}
