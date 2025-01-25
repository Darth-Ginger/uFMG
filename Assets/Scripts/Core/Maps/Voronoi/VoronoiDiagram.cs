using System.Collections.Generic;
using UnityEngine;

namespace FantasyMapGenerator.Core.Maps.Voronoi
{
    public static class VoronoiDiagram
    {
        public static List<VoronoiCell> Generate(List<Triangle> delaunayTriangles, List<Vector2> points)
        {
            // Create a dictionary to store the cells
            var cells = new Dictionary<Vector2, VoronoiCell>();

            // Initialize cells for each point
            foreach (var point in points)
            {
                cells[point] = new VoronoiCell(point);
            }

            // Store edges to track shared ones
            Dictionary<Edge, List<VoronoiCell>> edgeToCells = new();

            // Iterate through each Delaunay triangle to compute circumcenters
            foreach (var triangle in delaunayTriangles)
            {
                // Compute the circumcenter of the triangle
                Vector2 circumcenter = ComputeCircumcenter(triangle);

                // Assign the circumcenter to all three vertices (sites) of the triangle
                foreach (var vertex in triangle.Vertices)
                {
                    // Add the circumcenter to the Voronoi cell for the corresponding site
                    if (cells.ContainsKey(vertex))
                    {
                        cells[vertex].Vertices.Add(circumcenter);
                    }
                }
                // Add triangle edges to edgeToCells
                var edges = triangle.GetEdges();
                foreach (var edge in edges)
                {
                    if (!edgeToCells.ContainsKey(edge))
                    {
                        edgeToCells[edge] = new List<VoronoiCell>();
                    }

                    foreach (var vertex in triangle.Vertices)
                    {
                        if (cells.TryGetValue(vertex, out VoronoiCell cell))
                        {
                            if (!edgeToCells[edge].Contains(cell))
                            {
                                edgeToCells[edge].Add(cell);
                            }
                        }
                    }
                }
            }

            // Connect circumcenters to form edges (optional visualization)
            foreach (var cell in cells.Values)
            {
                OrderVertices(cell); // Order the vertices in a clockwise manner
            }

            // Establish adjacency relationships
            foreach (var pair in edgeToCells)
            {
                var adjacentCells = pair.Value;
                if (adjacentCells.Count == 2)
                {
                    adjacentCells[0].AddNeighbor(adjacentCells[1]);
                    adjacentCells[1].AddNeighbor(adjacentCells[0]);
                }
            }

            // Return the Voronoi cells
            return new List<VoronoiCell>(cells.Values);
        }

        private static Vector2 ComputeCircumcenter(Triangle triangle)
        {
            // Extract the vertices of the triangle
            Vector2 a = triangle.Vertices[0];
            Vector2 b = triangle.Vertices[1];
            Vector2 c = triangle.Vertices[2];

            // Compute the midpoints of two edges
            Vector2 midAB = (a + b) / 2f;
            Vector2 midBC = (b + c) / 2f;

            // Compute perpendicular slopes
            Vector2 dirAB = b - a;
            Vector2 dirBC = c - b;

            Vector2 perpAB = new Vector2(-dirAB.y, dirAB.x); // Rotate 90 degrees
            Vector2 perpBC = new Vector2(-dirBC.y, dirBC.x);

            // Solve for intersection of two lines: midAB + t * perpAB = midBC + s * perpBC
            float t = (midBC.x - midAB.x) * perpBC.y - (midBC.y - midAB.y) * perpBC.x;
            t /= (perpAB.x * perpBC.y - perpAB.y * perpBC.x);

            // Compute the circumcenter
            return midAB + t * perpAB;
        }

        private static void OrderVertices(VoronoiCell cell)
        {
            // Order the vertices in a clockwise manner for rendering
            Vector2 center = Vector2.zero;
            foreach (var vertex in cell.Vertices)
            {
                center += vertex;
            }
            center /= cell.Vertices.Count;

            cell.Vertices.Sort((v1, v2) =>
            {
                float angle1 = Mathf.Atan2(v1.y - center.y, v1.x - center.x);
                float angle2 = Mathf.Atan2(v2.y - center.y, v2.x - center.x);
                return angle1.CompareTo(angle2);
            });
        }
    }
}