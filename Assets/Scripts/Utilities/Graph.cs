using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyMapGenerator.Utilities
{
    public class Graph<TNodeKey, TEdgeKey>
    {
        // Node and Edge classes with arbitrary properties
        public class Node
        {
            public TNodeKey Key { get; set; }
            public Dictionary<string, object> Properties { get; set; } = new();

            public Node(TNodeKey key)
            {
                Key = key;
            }
        }

        public class Edge
        {
            public TEdgeKey Key { get; set; }
            public Node From { get; set; }
            public Node To { get; set; }
            public bool IsDirected { get; set; }
            public Dictionary<string, object> Properties { get; set; } = new();
            public string WeightKey { get; set; } // Key for the weight property

            public Edge(TEdgeKey key, Node from, Node to, bool isDirected = false, string weightKey = "weight")
            {
                Key = key;
                From = from;
                To = to;
                IsDirected = isDirected;
                WeightKey = weightKey;
            }

            public float GetWeight() => (float)Properties.GetValueOrDefault(WeightKey, 1f);

            /// <summary>
            ///  Acts as a placeholder for possible non-float based weights to floats
            ///  For example, 
            ///     if calculating based from a biome type A to biome type B, defines a method to calculate the weight
            /// </summary>
            /// <param name="heuristic"></param>
            /// <returns> 
            /// The weight of the edge as a float
            /// </returns>
            public float CalculateWeight(Func<Node, Node, float> heuristic) => heuristic(From, To); 
        }

        // Graph data structures
        private readonly Dictionary<TNodeKey, Node> _nodes = new();
        private readonly Dictionary<TEdgeKey, Edge> _edges = new();

        // Add a node to the graph
        public void AddNode(TNodeKey key, Dictionary<string, object> properties = null)
        {
            if (!_nodes.ContainsKey(key))
            {
                var node = new Node(key);
                if (properties != null)
                    node.Properties = new Dictionary<string, object>(properties);
                _nodes[key] = node;
            }
        }

        // Add an edge to the graph
        public void AddEdge(TEdgeKey key, TNodeKey fromKey, TNodeKey toKey, bool isDirected, Dictionary<string, object> properties = null)
        {
            if (!_nodes.ContainsKey(fromKey) || !_nodes.ContainsKey(toKey))
                throw new ArgumentException("Both nodes must exist in the graph.");

            if (!_edges.ContainsKey(key))
            {
                var edge = new Edge(key, _nodes[fromKey], _nodes[toKey], isDirected);
                if (properties != null)
                    edge.Properties = new Dictionary<string, object>(properties);
                _edges[key] = edge;
            }
        }

        // Remove a node and its associated edges
        public void RemoveNode(TNodeKey key)
        {
            if (_nodes.Remove(key))
            {
                var edgesToRemove = _edges.Values.Where(e => e.From.Key.Equals(key) || e.To.Key.Equals(key)).ToList();
                foreach (var edge in edgesToRemove)
                {
                    _edges.Remove(edge.Key);
                }
            }
        }

        // Remove an edge
        public void RemoveEdge(TEdgeKey key)
        {
            _edges.Remove(key);
        }

        // Get a node
        public Node GetNode(TNodeKey key)
        {
            return _nodes.TryGetValue(key, out var node) ? node : null;
        }

        // Get an edge
        public Edge GetEdge(TEdgeKey key)
        {
            return _edges.TryGetValue(key, out var edge) ? edge : null;
        }

        // Get all nodes
        public IEnumerable<Node> GetNodes()
        {
            return _nodes.Values;
        }

        // Get all edges
        public IEnumerable<Edge> GetEdges()
        {
            return _edges.Values;
        }

        // Breadth-First Search
        public List<TNodeKey> BreadthFirstSearch(TNodeKey startKey)
        {
            if (!_nodes.ContainsKey(startKey)) return new List<TNodeKey>();

            var visited = new HashSet<TNodeKey>();
            var queue = new Queue<Node>();
            var result = new List<TNodeKey>();

            queue.Enqueue(_nodes[startKey]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current.Key)) continue;

                result.Add(current.Key);
                var neighbors = _edges.Values
                    .Where(e => e.From == current && !visited.Contains(e.To.Key))
                    .Select(e => e.To);
                foreach (var neighbor in neighbors)
                {
                    queue.Enqueue(neighbor);
                }
            }

            return result;
        }

        // Dijkstra's Algorithm for Shortest Path
        public List<TNodeKey> Dijkstra(TNodeKey startKey, TNodeKey endKey)
        {
            if (!_nodes.ContainsKey(startKey) || !_nodes.ContainsKey(endKey)) return new List<TNodeKey>();

            var distances = new Dictionary<TNodeKey, float>();
            var previous = new Dictionary<TNodeKey, TNodeKey>();
            var unvisited = new HashSet<TNodeKey>(_nodes.Keys);

            foreach (var node in _nodes.Keys)
                distances[node] = float.MaxValue;

            distances[startKey] = 0;

            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(n => distances[n]).First();
                unvisited.Remove(current);

                if (current.Equals(endKey)) break;

                var neighbors = _edges.Values
                    .Where(e => e.From.Key.Equals(current) && unvisited.Contains(e.To.Key))
                    .Select(e => e.To.Key);

                foreach (var neighbor in neighbors)
                {
                    var edge = _edges.Values.First(e => e.From.Key.Equals(current) && e.To.Key.Equals(neighbor));
                    var alt = distances[current] + (float)edge.Properties.GetValueOrDefault("weight", 1f);
                    if (alt < distances[neighbor])
                    {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                    }
                }
            }

            var path = new List<TNodeKey>();
            for (var at = endKey; at != null && previous.ContainsKey(at); at = previous[at])
                path.Insert(0, at);

            return path.Contains(startKey) ? path : new List<TNodeKey>();
        }
        public List<TNodeKey> AStar(TNodeKey startKey, TNodeKey endKey, Func<TNodeKey, TNodeKey, float> heuristic)
        {
            if (!_nodes.ContainsKey(startKey) || !_nodes.ContainsKey(endKey))
                return new List<TNodeKey>();

            var openSet = new HashSet<TNodeKey> { startKey };
            var cameFrom = new Dictionary<TNodeKey, TNodeKey>();

            var gScore = new Dictionary<TNodeKey, float>();
            var fScore = new Dictionary<TNodeKey, float>();

            foreach (var nodeKey in _nodes.Keys)
            {
                gScore[nodeKey] = float.MaxValue;
                fScore[nodeKey] = float.MaxValue;
            }

            gScore[startKey] = 0;
            fScore[startKey] = heuristic(startKey, endKey);

            while (openSet.Count > 0)
            {
                // Get the node with the lowest fScore
                var current = openSet.OrderBy(n => fScore[n]).First();

                if (current.Equals(endKey))
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);

                // Explore neighbors
                var neighbors = _edges.Values
                    .Where(e => e.From.Key.Equals(current) || (!e.IsDirected && e.To.Key.Equals(current)))
                    .Select(e => e.From.Key.Equals(current) ? e.To.Key : e.From.Key);

                foreach (var neighbor in neighbors)
                {
                    var edge = _edges.Values.First(e =>
                        (e.From.Key.Equals(current) && e.To.Key.Equals(neighbor)) ||
                        (!e.IsDirected && e.From.Key.Equals(neighbor) && e.To.Key.Equals(current)));

                    var tentativeGScore = gScore[current] + edge.GetWeight();

                    if (tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + heuristic(neighbor, endKey);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return new List<TNodeKey>(); // No path found
        }

        private List<TNodeKey> ReconstructPath(Dictionary<TNodeKey, TNodeKey> cameFrom, TNodeKey current)
        {
            var path = new List<TNodeKey> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }
    }
}
