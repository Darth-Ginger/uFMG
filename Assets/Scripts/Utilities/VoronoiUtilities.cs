using UnityEngine;
using System.Collections.Generic;

namespace FantasyMapGenerator.Utilities
{
    public static class VoronoiUtilities
    {

        /// <summary>
        /// Computes the centroid of a polygon defined by the given vertices.
        /// Uses the standard centroid formula for polygons.
        /// </summary>
        public static Vector2 ComputePolygonCentroid(List<Vector2> vertices)
        {
            if (vertices == null || vertices.Count == 0)
                return Vector2.zero;

            float signedArea = 0;
            float cx = 0, cy = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 current = vertices[i];
                Vector2 next = vertices[(i + 1) % vertices.Count];
                float cross = current.x * next.y - next.x * current.y;
                signedArea += cross;
                cx += (current.x + next.x) * cross;
                cy += (current.y + next.y) * cross;
            }
            signedArea *= 0.5f;
            if (Mathf.Approximately(signedArea, 0)) return Vector2.zero;
            cx /= (6 * signedArea);
            cy /= (6 * signedArea);
            return new Vector2(cx, cy);
        }

        /// <summary>
        /// Returns the indices of neighboring cells for a given cell.
        /// This assumes that two cells sharing an edge are neighbors.
        /// </summary>
        public static List<int> GetNeighborCellIndices(VoronoiDiagram diagram, VoronoiCell cell)
        {
            List<int> neighbors = new List<int>();
            foreach (int edgeIndex in cell.EdgeIndices)
            {
                VoronoiEdge edge = diagram.Edges[edgeIndex];
                // Check both sides; exclude -1.
                if (edge.LeftCellIndex != -1 && edge.LeftCellIndex != diagram.Cells.IndexOf(cell)
                    && !neighbors.Contains(edge.LeftCellIndex))
                {
                    neighbors.Add(edge.LeftCellIndex);
                }
                if (edge.RightCellIndex != -1 && edge.RightCellIndex != diagram.Cells.IndexOf(cell)
                    && !neighbors.Contains(edge.RightCellIndex))
                {
                    neighbors.Add(edge.RightCellIndex);
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Returns a mapping from cell index to color. The algorithm uses a fixed palette
        /// and attempts to choose a color that none of the cell’s neighbors already use.
        /// </summary>
        public static Dictionary<int, Color> ColorCells(VoronoiDiagram diagram)
        {
            Dictionary<int, Color> cellColors = new Dictionary<int, Color>();

            // A sample palette – expand or modify as needed.
            Color[] palette = new Color[] {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.magenta, Color.cyan, Color.gray
        };

            // Process each cell.
            for (int i = 0; i < diagram.Cells.Count; i++)
            {
                HashSet<Color> neighborColors = new HashSet<Color>();
                foreach (int neighborIndex in diagram.Cells[i].Neighbors)
                {
                    if (cellColors.ContainsKey(neighborIndex))
                        neighborColors.Add(cellColors[neighborIndex]);
                }
                // Choose the first available color not used by neighbors.
                Color chosenColor = palette[0];
                foreach (Color candidate in palette)
                {
                    if (!neighborColors.Contains(candidate))
                    {
                        chosenColor = candidate;
                        break;
                    }
                }
                cellColors[i] = chosenColor;
            }
            return cellColors;
        }

        /// <summary>
        /// Creates a simple mesh for a convex polygon defined by the given vertices.
        /// </summary>
        public static Mesh CreateCellMesh(List<Vector2> polygon)
        {
            Mesh mesh = new Mesh();
            if (polygon == null || polygon.Count < 3)
                return mesh;

            Vector3[] vertices = new Vector3[polygon.Count];
            for (int i = 0; i < polygon.Count; i++)
            {
                // Place vertices on the XY plane (Z = 0)
                vertices[i] = new Vector3(polygon[i].x, polygon[i].y, 0);
            }

            // Triangulate via a fan (works for convex polygons)
            List<int> triangles = new List<int>();
            for (int i = 1; i < polygon.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}