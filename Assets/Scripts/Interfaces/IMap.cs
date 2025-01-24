using System.Collections.Generic;
using UnityEngine;
using FantasyMapGenerator.Utilities;

namespace FantasyMapGenerator.Interfaces
{
    public interface IMap
    {
        Vector2Int Size { get; } // e.g., width and height in grid units
        int Seed { get; }        // e.g., 123
        string Type { get; }     // e.g., "HeightMap", "Voronoi"
        void Generate();
        void Clear();
    }

    public interface IMap2D : IMap {}
    public interface IMap3D : IMap 
    {
        int Depth { get; }
    }
    public interface IGraphMap : IMap 
    {
        Graph<int, int> Graph { get; } // e.g., a graph of nodes and edges
    }
}
