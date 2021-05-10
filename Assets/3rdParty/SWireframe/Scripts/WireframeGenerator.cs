
using UnityEngine;

namespace S.Wireframe
{
    public class WireframeGenerator : MonoBehaviour
    {
        public bool GenerateWireframeAtRuntime = false;
        [SerializeField] private WireframeAnalyzer analyzer = new WireframeAnalyzer();

        private void Awake()
        {
            if (this.GenerateWireframeAtRuntime)
                Generate();
        }

        public void Generate()
        { 
            var meshFilter = this.GetComponent<MeshFilter>();
            Mesh mesh;
#if UNITY_EDITOR
            mesh = meshFilter.sharedMesh;
#else
            mesh = meshFilter.mesh;
#endif
            if (null == meshFilter || null == mesh)
                return;
            WireframeData wireframeData = new WireframeData();
            wireframeData.srcName = this.name;
            wireframeData.srcTransform = this.transform;
            wireframeData.mesh = mesh;

            analyzer.AnalyzeMesh(wireframeData, edges =>
            {
                WireframeUtility.GenerateWireframeMesh(edges, wireframeData.srcName, wireframeData.srcTransform);
            });
        }
    }
}