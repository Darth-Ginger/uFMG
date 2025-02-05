using UnityEngine;
using System.Collections.Generic;

namespace FantasyMapGenerator.Utilities
{
    [System.Serializable]
    public class VoronoiDiagram
    {
        [SerializeField]
        private List<Vector2> _sites = new List<Vector2>();
        public List<Vector2> Sites { get => _sites; }

        [SerializeField]
        private List<VoronoiEdge> _edges = new List<VoronoiEdge>();
        public List<VoronoiEdge> Edges { get => _edges; }

        [SerializeField]
        private List<VoronoiCell> _cells = new List<VoronoiCell>();
        public List<VoronoiCell> Cells { get => _cells; }

        [SerializeField]
        private Rect _bounds = new Rect(0, 0, 1000, 1000);
        public Rect Bounds { get => _bounds; }

        // Lazy lookup cache for sites to cells
        private Dictionary<Vector2, VoronoiCell> _siteToCell = new Dictionary<Vector2, VoronoiCell>();
        public Dictionary<Vector2, VoronoiCell> SiteToCell
        {
            get
            {
                if (_siteToCell == null)
                {
                    _siteToCell = new Dictionary<Vector2, VoronoiCell>();
                    for (int i = 0; i < Cells.Count; i++)
                    {
                        // Assumes sites and cells are in the same order
                        if (i < _cells.Count)
                        {
                            _siteToCell[_sites[i]] = _cells[i];
                        }
                    }
                }
                return _siteToCell;
            }
        }

        // Default Constructor
        public VoronoiDiagram(Rect bounds, List<Vector2> sites, List<VoronoiEdge> edges, List<VoronoiCell> cells)
        {
            _bounds = bounds;
            _sites = sites;
            _edges = edges;
            _cells = cells;
        }
        

        // Lazy Lookup Cache reset
        // Use if the sites or cells have changed
        public void ResetCaches()
        {
            _siteToCell = null;
        }

    }

    [System.Serializable]
    public class VoronoiCell
    {
        public Vector2 Centroid;

        [SerializeField]
        private List<int> _edgeIndices = new List<int>(); // Indices into VoronoiDiagram.Edges
        public List<int> EdgeIndices { get => _edgeIndices; }

        [SerializeField]
        private List<int> _neighbors = new List<int>(); // Indices into VoronoiDiagram.Cells
        public List<int> Neighbors { get => _neighbors; set { _neighbors = value; } }

        [SerializeField]
        private List<Vector2> vertices = new List<Vector2>(); // Polygon vertices
        public List<Vector2> Vertices { get { return vertices; } set { vertices = value; } }
    }

    [System.Serializable]
    public class VoronoiEdge
    {
        public Vector2 Start;
        public Vector2 End;
        public int LeftCellIndex;  // Index into World.Cells
        public int RightCellIndex; // -1 if boundary edge
    }
}