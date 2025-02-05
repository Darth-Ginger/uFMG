using UnityEngine;
using System.Collections.Generic;
using FantasyMapGenerator.Utilities;

namespace FantasyMapGenerator.Utilities.Debugging
{
    public class VoronoiDebugger : MonoBehaviour
    {
        public VoronoiDiagram diagram;

        // Toggle these in the Inspector to show different visualizations.
        public bool drawEdgesAndCentroids = true;
        public bool drawColoredCells = true;
        public Color edgeColor = Color.black;
        public Color centroidColor = Color.red;


        private void OnDrawGizmos()
        {
            if (diagram == null) return;

            if (drawEdgesAndCentroids)
            {
                VoronoiDebugVisualizer.DrawDiagramGizmos(diagram, edgeColor, centroidColor);
            }
            if (drawColoredCells)
            {
                VoronoiDebugVisualizer.DrawColoredDiagramOnPlane(diagram);
            }
        }
    }

    public static class VoronoiDebugVisualizer
    {

        /// <summary>
        /// Draws the diagram edges and centroids using Gizmos.
        /// </summary>
        public static void DrawDiagramGizmos(VoronoiDiagram diagram, Color edgeColor, Color centroidColor)
        {
            if (diagram == null) return;
            // Draw edges.
            Gizmos.color = edgeColor;
            foreach (var edge in diagram.Edges)
            {
                Gizmos.DrawLine(edge.Start, edge.End);
            }
            // Draw cell centroids.
            Gizmos.color = centroidColor;
            foreach (var cell in diagram.Cells)
            {
                Gizmos.DrawSphere(cell.Centroid, 0.1f);
            }
        }

        /// <summary>
        /// Draws filled cell polygons on a plane, assigning each cell a color that is (ideally)
        /// unique relative to its neighbors.
        /// </summary>
        public static void DrawColoredDiagramOnPlane(VoronoiDiagram diagram)
        {
            if (diagram == null) return;

            // Obtain a mapping from cell indices to colors.
            Dictionary<int, Color> cellColors = VoronoiUtilities.ColorCells(diagram);

            // Draw each cell as a filled polygon.
            for (int i = 0; i < diagram.Cells.Count; i++)
            {
                VoronoiCell cell = diagram.Cells[i];
                // Ensure the cell has a valid polygon.
                if (cell.Vertices == null || cell.Vertices.Count < 3)
                    continue;

                Mesh cellMesh = VoronoiUtilities.CreateCellMesh(cell.Vertices);
                Gizmos.color = cellColors[i];
                // Draw the mesh at identity. If your diagram should be transformed,
                // you can adjust the matrix via Gizmos.matrix.
                Gizmos.DrawMesh(cellMesh, Vector3.zero);
                // Optionally, draw the cell centroid.
                Gizmos.DrawSphere(cell.Centroid, 0.1f);
            }
        }
    }
}