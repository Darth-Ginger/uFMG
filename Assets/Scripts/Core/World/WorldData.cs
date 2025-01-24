using System.Collections.Generic;
using UnityEngine;
using FantasyMapGenerator.Interfaces;

namespace FantasyMapGenerator.Core.World
{
    [System.Serializable]
    public class WorldData : IWorldDataProvider
    {
        // Metadata
        public string worldName;
        public int worldSeed;
        public Vector2Int worldSize; // e.g., width and height in grid units

        // Maps
        public Dictionary<string, IMap> maps; // Keyed by map type (e.g., "HeightMap", "Voronoi")

        // Constructor
        public WorldData(string name, int seed, Vector2Int size)
        {
            worldName = name;
            worldSeed = seed;
            worldSize = size;
            maps = new Dictionary<string, IMap>();
        }

        // IWorldDataProvider Properties
        public Vector2Int GetWorldSize()          => worldSize;
        public int GetSeed()                      => worldSeed;
        public string GetWorldName()              => worldName;
        public Dictionary<string, IMap> GetMaps() => maps;

        // Add a map to the world
        public void AddMap(string mapName, IMap map)
        {
            if (!maps.ContainsKey(mapName))
            {
                Debug.Log($"Adding new map with name {mapName}");
                maps[mapName] = map;
            }
            else
            {
                Debug.LogWarning($"Map with name {mapName} exists updating map.");
                maps[mapName] = map;
            }
        }

        // Retrieve a map
        public IMap GetMap(string mapName) 
        {
            if (!maps.TryGetValue(mapName, out var map))
            {
                Debug.LogWarning($"Map with name {mapName} not found.");
                return null;
            }
            return map;
        }

        // Serialization Methods
        public string SerializeToJson() => JsonUtility.ToJson(this);

        public static WorldData DeserializeFromJson(string json) => JsonUtility.FromJson<WorldData>(json);

    }

}
