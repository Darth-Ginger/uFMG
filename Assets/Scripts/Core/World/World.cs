using UnityEngine;
using System.Collections.Generic;
using FantasyMapGenerator.Interfaces;
using FantasyMapGenerator.Utilities;


namespace FantasyMapGenerator.Core.World
{
    public class World : MonoBehaviour, IWorldProvider
    {
        // Voronoi diagram data (core geometry)
        [SerializeField] private VoronoiDiagram _voronoiDiagram;
        public VoronoiDiagram Voronoi => _voronoiDiagram;

        // Layers (e.g., Heightmap, Tectonics)
        [SerializeField] private List<LayerBase> _layers = new List<LayerBase>();

        // Diagram properties for easy access
        public IReadOnlyList<Vector2> Sites => _voronoiDiagram.Sites;
        public IReadOnlyList<VoronoiEdge> Edges => _voronoiDiagram.Edges;
        public IReadOnlyList<VoronoiCell> Cells => _voronoiDiagram.Cells;

        // Configuration
        [SerializeField] private int _seed = 12345;
        [SerializeField] private Rect _bounds = new Rect(0, 0, 1000, 1000);

        private void Awake()
        {
            // Generate Voronoi diagram on startup
            GenerateVoronoi(_seed, _bounds);
        }

        public void GenerateVoronoi(int seed, Rect bounds)
        {
            // Use VoronoiLib or your custom generator
            _voronoiDiagram = new VoronoiDiagram(bounds.width, bounds.height, seed);
        }

        public void AddLayer(LayerBase layer)
        {
            _layers.Add(layer);
            layer.Initialize(this); // Pass World reference to layer
        }

        public void RemoveLayer(string layerName)
        {
            var layer = _layers.Find(l => l.Name == layerName);
            if (layer != null)
            {
                _layers.Remove(layer);
            }
        }
        public void RemoveLayer(LayerBase layer)
        {
            if (_layers.Contains(layer))
            {
                _layers.Remove(layer);
            }
        }

        public LayerBase GetLayer(string layerName)
        {
            return _layers.Find(l => l.Name == layerName);
        }

        public List<LayerBase> GetLayers() => _layers;

    }
}