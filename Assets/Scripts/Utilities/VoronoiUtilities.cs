using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        public static Mesh CreateCellMesh(VoronoiCell cell)
        {
            Mesh mesh = new Mesh();
            if (cell == null || cell.Vertices == null || cell.Vertices.Count < 3)
                return mesh;

            Vector2 centroid = cell.Centroid;

            List<Vector2> sortedVertices = cell.Vertices
                .OrderBy(v => Mathf.Atan2(v.y - centroid.y, v.x - centroid.x))
                .ToList();

            // Convert the sorted 2D vertices into 3D vertices (placed on the XY plane, Z = 0).
            Vector3[] vertices3D = new Vector3[sortedVertices.Count];
            for (int i = 0; i < sortedVertices.Count; i++)
            {
                vertices3D[i] = new Vector3(sortedVertices[i].x, sortedVertices[i].y, 0f);
            }

            // Create a triangle fan: use the first vertex as an anchor and form triangles (0, i, i+1).
            List<int> triangles = new List<int>();
            for (int i = 1; i < sortedVertices.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            // Assign vertices and triangles to the mesh.
            mesh.vertices = vertices3D;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }


        /// <summary>
        /// Clips a polygon to the given rectangular bounds using the Sutherland–Hodgman algorithm.
        /// </summary>
        public static List<Vector2> ClipPolygonToRect(List<Vector2> polygon, Rect clipRect)
        {
            List<Vector2> outputList = new List<Vector2>(polygon);

            // Define the clipping edges: left, top, right, bottom.
            // Each edge is represented by a function that returns true if a point is inside.
            System.Func<Vector2, bool>[] inside =
            {
                    (p) => p.x >= clipRect.xMin, // left
                    (p) => p.y <= clipRect.yMax, // top (assuming y increases upward)
                    (p) => p.x <= clipRect.xMax, // right
                    (p) => p.y >= clipRect.yMin  // bottom
                };

            // For each edge, we need to clip the polygon.
            // Also, define a function that computes the intersection of a polygon edge with a clip edge.
            System.Func<Vector2, Vector2, int, Vector2> intersect = (p1, p2, edgeIndex) =>
            {
                float x = 0, y = 0;
                switch (edgeIndex)
                {
                    case 0: // left: x = clipRect.xMin
                        x = clipRect.xMin;
                        y = p1.y + (p2.y - p1.y) * (clipRect.xMin - p1.x) / (p2.x - p1.x);
                        break;
                    case 1: // top: y = clipRect.yMax
                        y = clipRect.yMax;
                        x = p1.x + (p2.x - p1.x) * (clipRect.yMax - p1.y) / (p2.y - p1.y);
                        break;
                    case 2: // right: x = clipRect.xMax
                        x = clipRect.xMax;
                        y = p1.y + (p2.y - p1.y) * (clipRect.xMax - p1.x) / (p2.x - p1.x);
                        break;
                    case 3: // bottom: y = clipRect.yMin
                        y = clipRect.yMin;
                        x = p1.x + (p2.x - p1.x) * (clipRect.yMin - p1.y) / (p2.y - p1.y);
                        break;
                }
                return new Vector2(x, y);
            };

            // Clip against each of the 4 boundaries.
            for (int edge = 0; edge < 4; edge++)
            {
                List<Vector2> inputList = new List<Vector2>(outputList);
                outputList.Clear();
                if (inputList.Count == 0)
                    break;

                Vector2 S = inputList[inputList.Count - 1];
                foreach (Vector2 E in inputList)
                {
                    if (inside[edge](E))
                    {
                        if (!inside[edge](S))
                        {
                            outputList.Add(intersect(S, E, edge));
                        }
                        outputList.Add(E);
                    }
                    else if (inside[edge](S))
                    {
                        outputList.Add(intersect(S, E, edge));
                    }
                    S = E;
                }
            }

            return outputList;
        }
    }

}
