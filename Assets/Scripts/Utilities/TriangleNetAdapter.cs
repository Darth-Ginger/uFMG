using System.Collections.Generic;
using UnityEngine;

// Triangle.NET namespaces (adjust these as needed)
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
                diagram.AddSite(site);
            }

            // Create a Triangle.NET polygon from the generated sites.
            Polygon polygon = new Polygon();
            foreach (Vector2 site in generatedSites)
            {
                polygon.Add(new TriangleNet.Geometry.Vertex(site.x, site.y));
            }

            // Triangulate the polygon using Triangle.NET.
            IMesh mesh = polygon.Triangulate();

            // Define Triangle.NET bounds (Rectangle: left, bottom, right, top).
            var triBounds = new TriangleNet.Geometry.Rectangle(bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax);

            // Create the full Voronoi diagram using Triangle.NET’s StandardVoronoi.
            StandardVoronoi voronoi = new StandardVoronoi((TriangleNet.Mesh)mesh, triBounds);

            // Create a lookup from sites to corresponding faces.
            Dictionary<int, VoronoiCell> siteToCellMap = new();

            List<VoronoiEdge> generatedEdges = new();

            int cellIndex = 0;

            foreach (var face in voronoi.Faces)
            {
                if (face.Generator == null) continue;

                // Create a new cell for the face.
                Vector2 centroid = new((float)face.Generator.X, (float)face.Generator.Y);
                VoronoiCell cell = new(centroid, cellIndex)
                {
                    // Add vertices to the cell.
                    Vertices = new()
                };

                HalfEdge edge = face.Edge;
                HalfEdge startEdge = edge;
                do
                {
                    if (edge == null || edge.Origin == null)
                    {
                        Debug.LogError("Edge or origin is null.");
                        break;
                    }

                    // Add the vertex to the cell.
                    var v = edge.Origin;
                    if (v != null) 
                    {
                        Vector2 vertex = new((float)v.X, (float)v.Y);
                        vertex.x = Mathf.Clamp(vertex.x, bounds.xMin, bounds.xMax);
                        vertex.y = Mathf.Clamp(vertex.y, bounds.yMin, bounds.yMax);
                        cell.AddVertex(vertex);
                    }

                    // Create Voronoi edge and add it to the cell.
                    if (edge.Twin != null && edge.Twin.Face != null && face != edge.Twin.Face)
                    {
                        VoronoiEdge voronoiEdge = new(
                            new Vector2((float)edge.Origin.X, (float)edge.Origin.Y),
                            new Vector2((float)edge.Twin.Origin.X, (float)edge.Twin.Origin.Y),
                            cellIndex,
                            edge.Twin.Face.ID
                        );
                        generatedEdges.Add(voronoiEdge);
                    }
                    
                    edge = edge.Next;
                } while (edge != null && edge != startEdge);

                // Add the cell to the diagram.
                cellIndex++;
                diagram.AddCell(cell);
                siteToCellMap[face.ID] = cell;
            }

            diagram.AddEdges(generatedEdges);

            // Compute Neighbors.
             ComputeNeighbors(diagram, voronoi, siteToCellMap);
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

        /// <summary>
        /// Helper method to extract a Unity Vector2 from a Triangle.NET vertex.
        /// </summary>
        private static Vector2 GetVertexPosition(TriangleNet.Geometry.Vertex vertex)
        {
            return new Vector2((float)vertex.X, (float)vertex.Y);
        }
    }

}