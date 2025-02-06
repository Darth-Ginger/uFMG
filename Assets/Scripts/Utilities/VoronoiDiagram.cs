using UnityEngine;
using System.Collections.Generic;
using System;

namespace FantasyMapGenerator.Utilities
{
    [System.Serializable]
    public class VoronoiDiagram
    {
        // Private backing fields
        [SerializeField] private List<Vector2> _sites = new List<Vector2>();
        [SerializeField] private List<VoronoiEdge> _edges = new List<VoronoiEdge>();
        [SerializeField] private List<VoronoiCell> _cells = new List<VoronoiCell>();
        [SerializeField] private Rect _bounds;
        private bool initialized = false;

        // Publicproperties   
        public List<Vector2> Sites { get => _sites; }
        public List<VoronoiEdge> Edges { get => _edges; }
        public List<VoronoiCell> Cells { get => _cells; }
        public Rect Bounds { get => _bounds; }

        public bool Initialized { get => initialized; }

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

        // Lazy Lookup Cache reset
        // Use if the sites or cells have changed
        public void ResetCaches() => _siteToCell = null;

        /// <summary>
        /// Creates a Voronoi diagram for a map of size (width, height) using the specified number of sites.
        /// The adapter is called here to generate the diagram.
        /// </summary>
        public VoronoiDiagram(float width = 1000, float height = 1000, int siteCount = 1000)
        {
            // Set up the bounds (here we assume the origin is at (0,0)).
            _bounds = new Rect(0, 0, width, height);
            // Generate the sites within the bounds.
            TriangleNetAdapter.GenerateDiagram(this, siteCount);

            initialized = true;
        }

        public void AddSite(Vector2 site)
        {
            _sites.Add(site);
            ResetCaches();
        }

        public void AddEdge(VoronoiEdge edge)
        {
            _edges.Add(edge);
            ResetCaches();
        }
        public void AddEdges(List<VoronoiEdge> edges) { foreach (VoronoiEdge edge in edges) AddEdge(edge); }

        public void AddCell(VoronoiCell cell)
        {
            _cells.Add(cell);
            ResetCaches();
        }


        /// <summary>
        /// Resets the cells and edges data.
        /// </summary>
        public void ClearDiagram()
        {
            _sites.Clear();
            _edges.Clear();
            _cells.Clear();

            initialized = false;
        }
    }
    [System.Serializable]
    public class VoronoiCell
    {
        public int Index;
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

        public VoronoiCell(Vector2 centroid, int index = -1)
        {
            Centroid = centroid;
            if (index < 0)
            {
                Index = Mathf.Abs(HashCode.Combine(Centroid));
            }
            else
            {
                Index = index;
            }
        }

        public void AddEdge(int edgeIndex) { if (!_edgeIndices.Contains(edgeIndex)) _edgeIndices.Add(edgeIndex); }
        public void AddEdges(List<int> edgeIndices) { foreach (int edgeIndex in edgeIndices) AddEdge(edgeIndex); }

        public void AddNeighbor(int cellIndex) { if (!_neighbors.Contains(cellIndex)) _neighbors.Add(cellIndex); }
        public void AddNeighbors(List<int> cellIndices) { foreach (int cellIndex in cellIndices) AddNeighbor(cellIndex);}

        public void AddVertex(Vector2 vertex) { if (!vertices.Contains(vertex)) vertices.Add(vertex); }
        public void AddVertices(List<Vector2> vertices) { foreach (Vector2 vertex in vertices) AddVertex(vertex); }

    }

    [System.Serializable]
    public class VoronoiEdge
    {
        public int Index;
        public Vector2 Start; // Start point of the edge
        public Vector2 End;   // End point of the edge
        public int LeftCellIndex;  // Indices into VoronoiDiagram.Cells
        public int RightCellIndex; // Indices into VoronoiDiagram.Cells

        public VoronoiEdge(Vector2 start, Vector2 end, int leftcell, int rightcell, int index = -1)
        {
            Start = start;
            End = end;
            LeftCellIndex = leftcell;
            RightCellIndex = rightcell;

            if (index < 0)
            {
                Index = Mathf.Abs(HashCode.Combine(LeftCellIndex, RightCellIndex));
            }
            else
            {
                Index = index;
            }
        }
    }
}