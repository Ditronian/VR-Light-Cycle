
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace S.Wireframe
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WireframeGenerator),isFallback = false)]
    public class WireframeGeneratorEditor :Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Renew Wireframe"))
            {
                var generator = serializedObject.targetObject as WireframeGenerator;
                var legacyWireframe = generator.transform.Find($"{generator.name}_wireframe");
                if (null != legacyWireframe)
                    GameObject.DestroyImmediate(legacyWireframe.gameObject);
                generator.Generate();
                generator.GenerateWireframeAtRuntime = false;
            }

            if (GUILayout.Button("Delete Exist Wireframe"))
            {
                var generator = serializedObject.targetObject as WireframeGenerator;
                var legacyWireframe = generator.transform.Find($"{generator.name}_wireframe");
                if (null != legacyWireframe)
                    GameObject.DestroyImmediate(legacyWireframe.gameObject);
            }
        } 

        [MenuItem("GameObject/SWireframe/Generate Wireframe",false,0)]
        public static void GenerateWireframe()
        {
            var gameobjects = Selection.gameObjects;
            if (null == gameobjects || gameobjects.Length <= 0)
                return;

            List<WireframeData> wireframeDatas = new List<WireframeData>();            
            foreach (var gameobject in gameobjects)
            {
                MeshFilter meshFilter = gameobject.GetComponent<MeshFilter>();
                if (null == meshFilter || null == meshFilter.sharedMesh)
                    continue;
                WireframeData wireframeData = new WireframeData();
                wireframeData.srcName = gameobject.name;
                wireframeData.srcTransform = gameobject.transform;
                wireframeData.mesh = meshFilter.sharedMesh;
                wireframeDatas.Add(wireframeData);
            }
            WireframeUtility.GenerateWireframe(wireframeDatas);
        }
    }
}
