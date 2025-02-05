using UnityEngine;
using System.Collections.Generic;
using FantasyMapGenerator.Interfaces;


namespace FantasyMapGenerator.Core.World
{
    public class World : MonoBehaviour, IWorldProvider
    {
        // Voronoi diagram data (core geometry)
        [SerializeField] private VoronoiDiagram _voronoiDiagram;
        public VoronoiDiagram Voronoi => _voronoiDiagram;

        // Layers (e.g., Heightmap, Tectonics)
        [SerializeField] private List<ILayer> _layers = new List<ILayer>();

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
            _voronoiDiagram = VoronoiAdapter.Generate(seed, bounds, 1000); // 1000 cells
        }

        public void AddLayer(ILayer layer)
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
        public void RemoveLayer(ILayer layer)
        {
            if (_layers.Contains(layer))
            {
                _layers.Remove(layer);
            }
        }

        public ILayer GetLayer(string layerName)
        {
            return _layers.Find(l => l.Name == layerName);
        }

        public List<ILayer> GetLayers() => _layers;

    }
}