
using UnityEngine;
using System.Collections.Generic;

namespace S.Wireframe
{
    public class WireframeUtility
    {
        public static void GenerateWireframe( IList<WireframeData> wireframeDatas )
        {
            if (null == wireframeDatas || wireframeDatas.Count <= 0)
                return;

            foreach (var wireframeData in wireframeDatas)
            {
                WireframeAnalyzer analyzer = new WireframeAnalyzer();
                analyzer.AnalyzeMesh(wireframeData, edges =>
                 {
                     GenerateWireframeMesh(edges, wireframeData.srcName, wireframeData.srcTransform);
                });
            }
        }

        public static void GenerateWireframeMesh(List<Edge> edges, string assetName,Transform parent = null)
        {
            Mesh mesh = new Mesh();
            mesh.name = assetName;
            List<Vector3> veritices = new List<Vector3>();
            List<int> indices = new List<int>();

            for (int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                veritices.Add(MathHelper.ToVector3(edge.A.pos));
                veritices.Add(MathHelper.ToVector3(edge.B.pos));
                indices.Add(i * 2 + 0);
                indices.Add(i * 2 + 1);
            }

            mesh.SetVertices(veritices);
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);

            GameObject wireframeCopy = new GameObject($"{assetName}_wireframe");
            wireframeCopy.transform.SetParent(parent);
            wireframeCopy.transform.localScale = Vector3.one;
            wireframeCopy.transform.localPosition = Vector3.zero;
            wireframeCopy.transform.localEulerAngles = Vector3.zero;
            var meshFilter = wireframeCopy.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            var meshRenderer = wireframeCopy.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Resources.Load<Material>("Wireframe"));
        }
    }
}
