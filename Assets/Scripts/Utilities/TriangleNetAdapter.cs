using System.Collections.Generic;
using UnityEngine;

// Triangle.NET namespaces (adjust these as needed)
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Voronoi;
using TriangleNet.Topology.DCEL;



namespace FantasyMapGenerator.Utilities
{


    public static class TriangleNetAdapter
    {
        /// <summary>
        /// Generates a Voronoi diagram and populates the provided diagram instance.
        /// This method handles site generation, triangulation, and conversion.
        /// </summary>
        public static void GenerateDiagram(VoronoiDiagram diagram, int siteCount)
        {
            // Clear any existing data in the diagram.
            diagram.ClearDiagram();

            // Set bounds for the site generation.
            Rect bounds = diagram.Bounds;

            // Generate sites within the bounds.
            List<Vector2> generatedSites = GenerateSites(siteCount, bounds);

            // Add the generated sites to the diagram.
            foreach (var site in generatedSites)
            {
                diagram.AddSite(site);
            }

            // Triangulate the polygon using Triangle.NET.
            IMesh mesh = TriangulateSites(generatedSites);

            // Define Triangle.NET bounds (Rectangle: left, bottom, right, top).
            var triBounds = new TriangleNet.Geometry.Rectangle(bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax);

            // Create the full Voronoi diagram using Triangle.NET’s StandardVoronoi.
            StandardVoronoi voronoi = CreateVoronoi(mesh, bounds);

            // Create a lookup from sites to corresponding faces.
            Dictionary<int, VoronoiCell> siteToCellMap = ExtractCells(diagram, voronoi, bounds);

            // Compute neighbors
            ComputeNeighbors(diagram, voronoi, siteToCellMap);

            // Extract edges from the Voronoi diagram.
            List<VoronoiEdge> generatedEdges = ExtractEdges(voronoi, siteToCellMap, diagram);
            diagram.AddEdges(generatedEdges);
            
        }

        /// <summary>
        /// Generates random sites within the given bounds.
        /// </summary>
        private static List<Vector2> GenerateSites(int count, Rect bounds)
        {
            List<Vector2> sites = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                float x = Random.Range(bounds.xMin, bounds.xMax);
                float y = Random.Range(bounds.yMin, bounds.yMax);
                sites.Add(new Vector2(x, y));
            }
            return sites;
        }

        /// <summary>
        /// Creates a Delaunay triangulation from a list of sites.
        /// </summary>
        private static IMesh TriangulateSites(List<Vector2> sites)
        {
            Polygon polygon = new Polygon();
            foreach (Vector2 site in sites)
                polygon.Add(new TriangleNet.Geometry.Vertex(site.x, site.y));
            return polygon.Triangulate();
        }

        /// <summary>
        /// Creates a Voronoi diagram from the triangulation, using the given bounds for clipping.
        /// </summary>
        private static StandardVoronoi CreateVoronoi(IMesh mesh, Rect bounds)
        {
            var triBounds = new TriangleNet.Geometry.Rectangle(bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax);
            return new StandardVoronoi((TriangleNet.Mesh)mesh, triBounds);
        }

        /// <summary>
        /// Extracts Voronoi cells from the StandardVoronoi object and adds them to the diagram.
        /// Returns a lookup dictionary from face ID to cell.
        /// </summary>
        private static Dictionary<int, VoronoiCell> ExtractCells(VoronoiDiagram diagram, StandardVoronoi voronoi, Rect clipBounds)
        {
            Dictionary<int, VoronoiCell> siteToCellMap = new Dictionary<int, VoronoiCell>();
            int cellIndex = 0;
            foreach (var face in voronoi.Faces)
            {
                if (face.Generator == null)
                    continue;

                Vector2 Centroid = new Vector2((float)face.Generator.X, (float)face.Generator.Y);
                VoronoiCell cell = new VoronoiCell(Centroid, cellIndex);

                // Reconstruct polygon from half-edges
                List<Vector2> vertices = new List<Vector2>();
                HalfEdge edge = face.Edge;
                HalfEdge startEdge = edge;
                do
                {
                    if (edge == null || edge.Origin == null)
                        break;
                    Vector2 v = new Vector2((float)edge.Origin.X, (float)edge.Origin.Y);
                    // Clip vertex to ensure it remains within clipBounds
                    v.x = Mathf.Clamp(v.x, clipBounds.xMin, clipBounds.xMax);
                    v.y = Mathf.Clamp(v.y, clipBounds.yMin, clipBounds.yMax);
                    vertices.Add(v);
                    edge = edge.Next;
                } while (edge != null && edge != startEdge);

                // Optionally, clip the entire polygon to the clipBounds here.
                cell.Vertices = VoronoiUtilities.ClipPolygonToRect(vertices, clipBounds);

                // Set cell index and add it to the diagram
                cell.Index = cellIndex++;
                diagram.AddCell(cell);
                siteToCellMap[face.ID] = cell;
            }
            return siteToCellMap;
        }

        /// <summary>
    /// Extracts Voronoi edges from the StandardVoronoi object.
    /// Uses a dictionary keyed by a sorted pair of cell indices to avoid duplicate edges.
    /// </summary>
    private static List<VoronoiEdge> ExtractEdges(StandardVoronoi voronoi, Dictionary<int, VoronoiCell> siteToCellMap, VoronoiDiagram diagram)
    {
        Dictionary<string, VoronoiEdge> edgeDict = new Dictionary<string, VoronoiEdge>();

        foreach (var face in voronoi.Faces)
        {
            if (!siteToCellMap.ContainsKey(face.ID))
                continue;
            // Get the current cell's index.
            int leftCellIndex = siteToCellMap[face.ID].Index;
            HalfEdge edge = face.Edge;
            HalfEdge startEdge = edge;
            do
            {
                if (edge != null && edge.Twin != null && siteToCellMap.ContainsKey(edge.Twin.Face.ID))
                {
                    int rightCellIndex = siteToCellMap[edge.Twin.Face.ID].Index;
                    // Create a key with the smaller index first to avoid duplicates.
                    int minIndex = Mathf.Min(leftCellIndex, rightCellIndex);
                    int maxIndex = Mathf.Max(leftCellIndex, rightCellIndex);
                    string key = minIndex + "_" + maxIndex;
                    if (!edgeDict.ContainsKey(key))
                    {
                        // Use the origin of this edge and the origin of its twin to define the edge.
                        Vector2 startPt = new((float)edge.Origin.X, (float)edge.Origin.Y);
                        Vector2 endPt   = new((float)edge.Twin.Origin.X, (float)edge.Twin.Origin.Y);
                        
                        VoronoiEdge vEdge = new VoronoiEdge(startPt, endPt, leftCellIndex, rightCellIndex);
                        edgeDict.Add(key, vEdge);
                    }
                }
                edge = edge.Next;
            } while (edge != null && edge != startEdge);
        }

        List<VoronoiEdge> edges = new List<VoronoiEdge>(edgeDict.Values);

        // Assign edge indices to cells
        for(int i = 0; i < edges.Count; i++)
        {
            VoronoiEdge e = edges[i];
            // Add the edge index to the both cells
            if (e.LeftCellIndex > 0 && e.LeftCellIndex < diagram.Cells.Count)
            {
                diagram.Cells[e.LeftCellIndex].AddEdge(i);
            }
            if (e.RightCellIndex > 0 && e.RightCellIndex < diagram.Cells.Count)
            {
                diagram.Cells[e.RightCellIndex].AddEdge(i);
            }
        }

        return edges;
    }

        /// <summary>
        /// Computes neighbor relationships for cells by inspecting shared edges.
        /// </summary>
        private static void ComputeNeighbors(VoronoiDiagram diagram, StandardVoronoi voronoi, Dictionary<int, VoronoiCell> siteToCellMap)
        {
            foreach (var face in voronoi.Faces)
            {
                if (!siteToCellMap.ContainsKey(face.ID)) continue;
                VoronoiCell cell = siteToCellMap[face.ID];

                HalfEdge edge = face.Edge;
                HalfEdge startEdge = edge;
                do
                {
                    if (edge == null || edge.Origin == null)
                    {
                        Debug.LogError("Edge or origin is null.");
                        break;
                    }

                    if (edge.Twin != null && siteToCellMap.ContainsKey(edge.Twin.Face.ID))
                    {
                        VoronoiCell neighbor = siteToCellMap[edge.Twin.Face.ID];
                        if (!cell.Neighbors.Contains(neighbor.Index))
                        {
                            cell.Neighbors.Add(neighbor.Index);
                        }
                    }
                    edge = edge.Next;
                } while (edge != null && edge != startEdge);
            }
        }

        private static Rect SiteBounds(List<Vector2> vertices)
        {
            // Set initial min/max values
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            // Find min/max values
            foreach (Vector2 vertex in vertices)
            {
                minX = Mathf.Min(minX, vertex.x);
                maxX = Mathf.Max(maxX, vertex.x);
                minY = Mathf.Min(minY, vertex.y);
                maxY = Mathf.Max(maxY, vertex.y);
            }

            // Return bounds as a Rect
            return new Rect(minX, minY, maxX - minX, maxY - minY);

        }

        /// <summary>
        /// Helper method to extract a Unity Vector2 from a Triangle.NET vertex.
        /// </summary>
        private static Vector2 GetVertexPosition(TriangleNet.Geometry.Vertex vertex)
        {
            return new Vector2((float)vertex.X, (float)vertex.Y);
        }
    }

}