
using System.Collections.Generic;
using UnityEngine;

namespace FantasyMapGenerator.Core.Maps.Voronoi
{
    public class VoronoiCell
    {
        public Vector2 Site; // Voronoi site
        public List<Vector2> Vertices; // Cell vertices

        public VoronoiCell(Vector2 site)
        {
            Site = site;
            Vertices = new List<Vector2>();
        }

        public override string ToString()
        {
            return $"VoronoiCell: Site={Site}, Vertices={Vertices.Count}";
        }
    }
}