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
        /// Generates a VoronoiDiagram from the given list of sites using Triangle.NET.
        /// </summary>
        /// <param name="sites">The input sites as a list of Vector2.</param>
        /// <param name="bounds">A bounding rectangle to clip infinite edges.</param>
        /// <returns>A VoronoiDiagram populated with cells and edges.</returns>
        public static VoronoiDiagram GenerateVoronoiDiagram(List<Vector2> sites, Rect bounds)
        {
            // 1. Create a Triangle.NET polygon and add the input sites.
            var polygon = new TriangleNet.Geometry.Polygon();
            foreach (var site in sites)
            {
                // Note: Triangle.NET uses double precision.
                polygon.Add(new Vertex(site.x, site.y));
            }

            // 2. Triangulate the polygon.
            var mesh = polygon.Triangulate();

            // 3. Define a Triangle.NET rectangle for bounding.
            // (TriangleNet.Geometry.Rectangle expects left, bottom, right, top.)
            var triBounds = new TriangleNet.Geometry.Rectangle(bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax);

            // 4. Generate the Voronoi diagram.
            // StandardVoronoi (or your version’s equivalent) computes the dual.
            var voronoi = new StandardVoronoi(mesh, triBounds);

            // 5. Create our custom VoronoiDiagram instance.
            VoronoiDiagram diagram = new VoronoiDiagram();
            diagram.Sites.AddRange(sites);

            // We'll use a dictionary to map from Triangle.NET generator vertex ID to our cell index.
            Dictionary<int, int> siteIdToCellIndex = new Dictionary<int, int>();

            // 6. Map Triangle.NET regions (cells) to our VoronoiCell.
            // Here we assume that voronoi.Cells is a collection of regions.
            foreach (var region in voronoi.Cells)
            {
                // Each region has a Generator (a Triangle.NET Vertex) that corresponds to an input site.
                int cellIndex = diagram.Cells.Count;
                VoronoiCell vCell = new VoronoiCell();

                // For now, we set the cell’s centroid to the generator’s coordinates.
                // (Optionally, you can compute the centroid from the polygon vertices.)
                vCell.Centroid = new Vector2((float)region.Generator.X, (float)region.Generator.Y);

                // Extract the polygon vertices from the cell.
                List<Vector2> polyVerts = new List<Vector2>();
                foreach (var vert in region.Vertices)
                {
                    polyVerts.Add(new Vector2((float)vert.X, (float)vert.Y));
                }
                vCell.Vertices = polyVerts;

                diagram.Cells.Add(vCell);
                siteIdToCellIndex[region.Generator.ID] = cellIndex;
            }

            // 7. Map the Voronoi edges.
            // We assume voronoi.Edges is a collection of edges with Start/End points,
            // and with Left and Right references pointing to the generator vertices of adjacent cells.
            foreach (var edge in voronoi.Edges)
            {
                VoronoiEdge vEdge = new VoronoiEdge();

                // Convert Triangle.NET points to UnityEngine.Vector2.
                vEdge.Start = new Vector2((float)edge.Start.X, (float)edge.Start.Y);
                vEdge.End = new Vector2((float)edge.End.X, (float)edge.End.Y);

                // Determine left and right cell indices.
                // If a side is null (or not present in our mapping) we set it to -1 (indicating a boundary).
                if (edge.Left != null && siteIdToCellIndex.ContainsKey(edge.Left.ID))
                {
                    vEdge.LeftCellIndex = siteIdToCellIndex[edge.Left.ID];
                }
                else
                {
                    vEdge.LeftCellIndex = -1;
                }

                if (edge.Right != null && siteIdToCellIndex.ContainsKey(edge.Right.ID))
                {
                    vEdge.RightCellIndex = siteIdToCellIndex[edge.Right.ID];
                }
                else
                {
                    vEdge.RightCellIndex = -1;
                }

                // Add the edge and register its index in the associated cells.
                int edgeIndex = diagram.Edges.Count;
                diagram.Edges.Add(vEdge);

                if (vEdge.LeftCellIndex != -1)
                    diagram.Cells[vEdge.LeftCellIndex].EdgeIndices.Add(edgeIndex);
                if (vEdge.RightCellIndex != -1)
                    diagram.Cells[vEdge.RightCellIndex].EdgeIndices.Add(edgeIndex);
            }

            // 8. Update neighbor indices for each cell.
            foreach (var edge in diagram.Edges)
            {
                if (edge.LeftCellIndex != -1 && edge.RightCellIndex != -1)
                {
                    if (!diagram.Cells[edge.LeftCellIndex].Neighbors.Contains(edge.RightCellIndex))
                    {
                        diagram.Cells[edge.LeftCellIndex].Neighbors.Add(edge.RightCellIndex);
                    }
                    if (!diagram.Cells[edge.RightCellIndex].Neighbors.Contains(edge.LeftCellIndex))
                    {
                        diagram.Cells[edge.RightCellIndex].Neighbors.Add(edge.LeftCellIndex);
                    }
                }
            }

            return diagram;
        }
    }
}