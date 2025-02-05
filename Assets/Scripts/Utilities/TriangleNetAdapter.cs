using System.Collections.Generic;
using UnityEngine;

// Triangle.NET namespaces (adjust these as needed)
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Voronoi;


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

            // Generate sites within the bounds.
            Rect bounds = diagram.Bounds;
            List<Vector2> generatedSites = new List<Vector2>();
            for (int i = 0; i < siteCount; i++)
            {
                float x = Random.Range(bounds.xMin, bounds.xMax);
                float y = Random.Range(bounds.yMin, bounds.yMax);
                generatedSites.Add(new Vector2(x, y));
            }

            // Add the generated sites to the diagram.
            foreach (var site in generatedSites)
            {
                // Note: The diagram only stores the positions.
                // If you want to associate more information, you can extend this.
                diagram.Sites.Add(site);
            }

            // Create a Triangle.NET polygon from the generated sites.
            Polygon polygon = new Polygon();
            foreach (Vector2 site in generatedSites)
            {
                polygon.Add(new Vertex(site.x, site.y));
            }

            // Optionally, you could add constraints or boundary segments here based on bounds.

            // Triangulate the polygon using Triangle.NET.
            // This produces an IMesh.
            IMesh mesh = polygon.Triangulate();

            // Convert the Triangle.NET mesh into your Voronoi diagram structure.
            // (This example uses a simplified conversion; you might need to extract full Voronoi cells.)
            UpdateDiagramFromMesh(diagram, mesh);
        }

        /// <summary>
        /// Converts a Triangle.NET mesh (IMesh) into the cells, edges, and neighbors for the diagram.
        /// </summary>
        private static void UpdateDiagramFromMesh(VoronoiDiagram diagram, IMesh mesh)
        {
            // Dictionary to map a Triangle.NET vertex ID to a cell index.
            Dictionary<int, int> siteIdToCellIndex = new Dictionary<int, int>();

            // Create cells for each triangle in the mesh.
            // (In a complete implementation, the conversion from Delaunay triangulation to Voronoi cells is more involved.)
            foreach (var tri in mesh.Triangles)
            {
                VoronoiCell cell = new VoronoiCell();
                // For illustration, compute the triangle's centroid as the cell centroid.
                Vector2 v0 = GetVertexPosition(tri.GetVertex(0));
                Vector2 v1 = GetVertexPosition(tri.GetVertex(1));
                Vector2 v2 = GetVertexPosition(tri.GetVertex(2));
                cell.Centroid = (v0 + v1 + v2) / 3f;
                
                cell.Vertices.Add(v0);
                cell.Vertices.Add(v1);
                cell.Vertices.Add(v2);

                int cellIndex = diagram.Cells.Count;
                diagram.Cells.Add(cell);

                // Use the first vertex's ID as a placeholder for mapping.
                int siteId = tri.GetVertex(0).ID;
                if (!siteIdToCellIndex.ContainsKey(siteId))
                    siteIdToCellIndex.Add(siteId, cellIndex);
            }

            // Convert triangle edges to Voronoi edges.
            foreach (var tri in mesh.Triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    VoronoiEdge edge = new VoronoiEdge();
                    edge.Start = GetVertexPosition(tri.GetVertex(i));
                    edge.End = GetVertexPosition(tri.GetVertex((i + 1) % 3));

                    // Use our mapping as a placeholder for cell indices.
                    int id1 = tri.GetVertex(i).ID;
                    int id2 = tri.GetVertex((i + 1) % 3).ID;
                    edge.LeftCellIndex = siteIdToCellIndex.ContainsKey(id1) ? siteIdToCellIndex[id1] : -1;
                    edge.RightCellIndex = siteIdToCellIndex.ContainsKey(id2) ? siteIdToCellIndex[id2] : -1;

                    int edgeIndex = diagram.Edges.Count;
                    diagram.Edges.Add(edge);

                    // Register this edge with the corresponding cells.
                    if (edge.LeftCellIndex != -1)
                        diagram.Cells[edge.LeftCellIndex].EdgeIndices.Add(edgeIndex);
                    if (edge.RightCellIndex != -1)
                        diagram.Cells[edge.RightCellIndex].EdgeIndices.Add(edgeIndex);
                }
            }

            // Post-process to compute neighbor relationships.
            ComputeNeighbors(diagram);
        }

        /// <summary>
        /// Computes neighbor relationships for cells by inspecting shared edges.
        /// </summary>
        private static void ComputeNeighbors(VoronoiDiagram diagram)
        {
            foreach (var edge in diagram.Edges)
            {
                if (edge.LeftCellIndex != -1 && edge.RightCellIndex != -1)
                {
                    VoronoiCell left = diagram.Cells[edge.LeftCellIndex];
                    VoronoiCell right = diagram.Cells[edge.RightCellIndex];
                    if (!left.Neighbors.Contains(edge.RightCellIndex))
                        left.Neighbors.Add(edge.RightCellIndex);
                    if (!right.Neighbors.Contains(edge.LeftCellIndex))
                        right.Neighbors.Add(edge.LeftCellIndex);
                }
            }
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