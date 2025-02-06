using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;



namespace FantasyMapGenerator.Utilities.Debugging
{

    public class VoronoiDebugger : MonoBehaviour
    {
        [Header("Input Settings")]
        [Tooltip("If true, the diagram will be generated at startup.")]
        public bool generateDiagramOnStart = true;
        [Tooltip("Bounds for the diagram generation (for clipping infinite edges).")]
        public float width = 1000;
        public float height = 1000;
        [Tooltip("Number of sites to generate.")]
        public int siteCount = 10;

        // Toggle these in the Inspector to show different visualizations.
        public bool drawEdgesAndCentroids = true;
        public bool drawColoredCells = true;
        public Color edgeColor = Color.black;
        public Color centroidColor = Color.red;
        public GameObject diagramHost = null;

        public VoronoiDiagram diagram = null;

        // Button for generating diagram in the inspector.
        [Button("Generate Diagram")]
        public void GenerateDiagram() => diagram = new VoronoiDiagram(width, height, siteCount);
        

        private void Start() 
        {
            if (diagramHost == null) diagramHost = gameObject;

            // Optionally generate the diagram if we have a list of sites.
            if (generateDiagramOnStart && siteCount > 0) GenerateDiagram();

            // Optionally create the diagram on a plane.
            if (drawColoredCells)
            {
                foreach (Transform child in diagramHost.transform)
                {
                    Destroy(child.gameObject);
                }

                CreateDiagramOnPlane();
            }
        }

        private bool CreateOnPlane() => diagram != null && diagram.Initialized;
        [ShowIf("CreateOnPlane")]
        [Button("Create Diagram On Plane")]
        private void CreateDiagramOnPlane()
        {
            if (diagram == null | !diagram.Initialized) return;
            if (diagramHost == null) diagramHost = gameObject;

            VoronoiDebugVisualizer.ApplyColoredDiagramToPlane(diagram, diagramHost);

        }

        private bool CanClearDiagram() => diagram != null && diagram.Initialized;
        [EnableIf("CanClearDiagram")]
        [Button("Clear Diagram")]
        private void ClearDiagram() 
        {
            if (diagram == null | !diagram.Initialized) return;
            diagram.ClearDiagram();
            ClearColoredCells();
        }

        private bool GOContainerExists() => diagramHost != null && diagramHost.transform.Find("ColoredCellsContainer") != null;
        [EnableIf("GOContainerExists")]
        [Button("Clear Colored Cells")]
        private void ClearColoredCells() 
        {
            if (diagramHost == null | !GOContainerExists()) return;
            DestroyImmediate(diagramHost.transform.Find("ColoredCellsContainer").gameObject);
        }

        private void OnDrawGizmos()
        {
            if (diagram == null | !diagram.Initialized) return;

            if (drawEdgesAndCentroids)
            {
                VoronoiDebugVisualizer.DrawDiagramGizmos(edgeColor, centroidColor, diagram);
            }

        }
    }

    public static class VoronoiDebugVisualizer
    {

        /// <summary>
        /// Draws the diagram edges and centroids using Gizmos.
        /// </summary>
        public static void DrawDiagramGizmos(Color edgeColor, Color centroidColor, VoronoiDiagram diagram = null)
        {
            if (diagram == null | !diagram.Initialized) return;
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
    /// For each cell in the diagram that has a valid polygon, create (or update) a child GameObject
    /// under the specified host to display the cell with a unique color.
    /// </summary>
    public static void ApplyColoredDiagramToPlane(VoronoiDiagram diagram, GameObject host) {
        if (diagram == null || host == null) return;
        
        // Get a mapping from cell index to color using our utility.
        Dictionary<int, Color> cellColors = VoronoiUtilities.ColorCells(diagram);

        // Look for (or create) a container child to hold the cell objects.
        Transform container = host.transform.Find("ColoredCellsContainer");
        if (container == null) {
            GameObject containerGO = new GameObject("ColoredCellsContainer");
            containerGO.transform.parent = host.transform;
            containerGO.transform.localPosition = Vector3.zero;
            container = containerGO.transform;
        }
        
        Debug.Log($"Applying colored diagram to plane with {diagram.Cells.Count} cells.");
        // For each cell, create or update a child object.
        for (int i = 0; i < diagram.Cells.Count; i++) {
            VoronoiCell cell = diagram.Cells[i];
            
            // Only proceed if the cell has a valid polygon.
            if (cell.Vertices == null || cell.Vertices.Count < 3)
            {
                Debug.LogWarning($"Cell {i} has no valid polygon.");
                continue;
            }

            string cellName = "Cell_" + i;
            Transform cellTransform = container.Find(cellName);
            GameObject cellGO;
            if (cellTransform == null) {
                cellGO = new GameObject(cellName);
                cellGO.transform.parent = container;
                cellGO.transform.localPosition = Vector3.zero;
            } else {
                cellGO = cellTransform.gameObject;
            }
            
            // Ensure the cell object has a MeshFilter and MeshRenderer.
            MeshFilter mf = cellGO.GetComponent<MeshFilter>();
            if (mf == null)
                mf = cellGO.AddComponent<MeshFilter>();
            MeshRenderer mr = cellGO.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = cellGO.AddComponent<MeshRenderer>();
            CellData cd = cellGO.AddComponent<CellData>();
            cd.SetCellData(cell);


            // Create (or update) the mesh for the cell polygon.
            Mesh cellMesh = VoronoiUtilities.CreateCellMesh(cell);
            mf.mesh = cellMesh;
            
            // Create a material with the assigned color.
            // Here we use an unlit color shader.
            Material cellMat = new Material(Shader.Find("Unlit/Color"));
            cellMat.color = cellColors.ContainsKey(i) ? cellColors[i] : Color.white;
            mr.material = cellMat;
        }
    }

    }
}