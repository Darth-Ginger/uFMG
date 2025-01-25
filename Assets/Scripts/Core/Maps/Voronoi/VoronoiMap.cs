using System.Collections.Generic;
using FantasyMapGenerator.Interfaces;
using UnityEngine;
using static UnityEngine.Random;


namespace FantasyMapGenerator.Core.Maps.Voronoi
{
    public class VoronoiMap : IMap
    {

        private List<Vector2> _points;      // Points of the voronoi map
        private List<Triangle> _triangles;  // Delaunay Triangules
        private List<VoronoiCell> _cells;   // Voronoi Cells


        // Public properties from IMap
        public Vector2Int Size { get; private set; }
        public int Seed { get; private set; }
        public string Type => "Voronoi";

        public List<VoronoiCell> GetCells() => _cells;


        public VoronoiMap(Vector2Int size, int seed, int pointCount = 100)
        {
            Size = size;
            Seed = seed;
            _points = new List<Vector2>();
            _triangles = new List<Triangle>();
            _cells = new List<VoronoiCell>();
            
            GeneratePoints(pointCount);
        }

        // Generate random points for the map
        public void GeneratePoints(int count)
        {
            Random.InitState(Seed);
            _points.Clear();

            for (int i = 0; i < count; i++)
            {
                _points.Add(new Vector2(
                    Random.Range(0, Size.x),
                    Random.Range(0, Size.y)
                ));
            }
        }
        // Generate the Voronoi diagram
        public void Generate()
        {
            // Ensure points exist
            if (_points.Count == 0)
            {
                Debug.LogError("No points to generate Voronoi diagram. Call GeneratePoints() first.");
                return;
            }

            // Perform Delaunay triangulation (placeholder for your Triangle logic)
            _triangles = DelaunayTriangulation.Generate(_points);

            // Derive Voronoi cells from triangles (implement cell generation logic)
            _cells = VoronoiDiagram.Generate(_triangles, _points);
        }

        // Clear the map
        public void Clear()
        {
            _points.Clear();
            _triangles.Clear();
            _cells.Clear();
        }


    }
}
