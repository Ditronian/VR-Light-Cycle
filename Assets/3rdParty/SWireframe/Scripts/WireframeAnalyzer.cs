
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using StopWatch = System.Diagnostics.Stopwatch;

namespace S.Wireframe
{
    [System.Serializable]
    public class WireframeAnalyzer
    { 
        private List<Triangle> triangles = new List<Triangle>();
        private List<Triangle> ignoreTriangles = new List<Triangle>();
        private ConcurrentDictionary<Edge, ConcurrentBag<SharedEdgeResult>> resultViaEdgeDic = new ConcurrentDictionary<Edge, ConcurrentBag<SharedEdgeResult>>();

        private WireframeData wireframeData;
        private Action<List<Edge>> completed;
        private float verticalTolerance = 0.05f;
        private float minAngleInRadian = 0.95f;
        private float maxAngleInRadian = -0.95f;

        [Header("Basic Setting")]
        [Range(0.0f, 89.9f)] public float minimumAngleInDegree = 5.0f;
        [Range(90.1f, 180.0f)] public float maximumAngleInDegree = 175.0f;
        [Range(0.0f,45.0f)] public float verticalAngleToleranceInDegree = 2.5f; // angle in degree

        public float toleranceBetweenVertexAndTriangle = 0.0f;
        public float toleranceVertexInTriangle = 0.0f;

        [HideInInspector] public bool enableIgnoreConditions = false;
        [Header("Ignore Setting")]
        public bool ignoreIsolatedEdge = true;
        public bool ignoreMinimalTriangle = false;
        public float minimalTriangleEdgeLength = 0.0f;
        public float ignoreEdgeLengthLessThan = 0.0f;
         
        // every task would process 128 triangles. more thread does not meaning faster processing speed.
        // you can try to modify this parameter according to pc performance
        [Header("Multi Thread Setting")]
        [Range(32,256)] public int MAXIMUM_THREAD_PROCESSING_CAPACITY = 128;

        [Header("Debug Setting")]
        public bool enableDebugOutput = false;
         
        public void AnalyzeMesh( WireframeData wireframeData , Action<List<Edge>> completed )
        {
            this.wireframeData = wireframeData;
            this.completed = completed;
            this.PreprocessParameters();
            this.PreprocessMesh();
            this.TriangleFilter();
            this.AnalyzeSharedEdge();
        }

        private void ClearAnalyzer()
        {
            this.triangles.Clear();
            this.ignoreTriangles.Clear();
            this.resultViaEdgeDic.Clear();
        }

        private void PreprocessMesh()
        { 
            List<Vector3> vertices = new List<Vector3>();
            this.wireframeData.mesh.GetVertices(vertices);
            var indices = this.wireframeData.mesh.GetTriangles(0);
            List<Vector3> normals = new List<Vector3>();
            this.wireframeData.mesh.GetNormals(normals);

            for (int i = 0; i < indices.Length; i += 3)
            {
                Triangle triangle = new Triangle(
                    MathHelper.ToVec3(vertices[indices[i + 0]]),
                    MathHelper.ToVec3(vertices[indices[i + 1]]),
                    MathHelper.ToVec3(vertices[indices[i + 2]]));
                this.triangles.Add(triangle);
            }
        }

        private void PreprocessParameters()
        {
            this.verticalTolerance = Mathf.Abs(Mathf.Cos(Mathf.PI * 0.5f - this.verticalAngleToleranceInDegree * Mathf.Deg2Rad));
            this.minAngleInRadian = Mathf.Cos(this.minimumAngleInDegree * Mathf.Deg2Rad);
            this.maxAngleInRadian = Mathf.Cos(this.maximumAngleInDegree * Mathf.Deg2Rad);
        }

        private void TriangleFilter()
        {
            List<int> omitList = new List<int>();

            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle tri = this.triangles[i];
                float aLength = tri.edgeA.length;
                float bLength = tri.edgeB.length;
                float cLength = tri.edgeC.length;
                 
                if ((aLength == 0.0f) && (bLength == 0.0f) && (cLength == 0.0f))
                {
                    ignoreTriangles.Add(tri);
                    omitList.Add(i);
                    continue;
                } 

                if (this.ignoreMinimalTriangle)
                {
                    if (aLength < minimalTriangleEdgeLength && bLength < minimalTriangleEdgeLength && cLength < minimalTriangleEdgeLength)
                    {
                        ignoreTriangles.Add(tri);
                        omitList.Add(i);
                        continue;
                    }
                }

                //if (this.ignoreOxygonTriangle)
                //{
                //    if (tri.IsOxygon(minOxygonCosAngle))
                //    {
                //        ignoreTriangles.Add(tri);
                //        omitList.Add(i);
                //        continue;
                //    }
                //}
            }

            if (enableIgnoreConditions)
            {
                for (int i = omitList.Count - 1; i >= 0; i--)
                    triangles.RemoveAt(omitList[i]);
            }

            if( this.enableDebugOutput )
                Debug.Log($"<color=green>filter invalid triangle count : {ignoreTriangles.Count}</color>");
        }

        private bool IsIgnoreTriangles(in Edge edge)
        {
            for (int i = 0; i < ignoreTriangles.Count; i++)
            {
                var tri = ignoreTriangles[i];
                if (edge.BelongTriangle(tri))
                    return true;
            }
            return false;
        }

        private void InsertIntoQueryPreparationDic(in Edge edge, List<SharedEdgeResult> results)
        {
            if (resultViaEdgeDic.ContainsKey(edge))
            {
                foreach (var result in results)
                    resultViaEdgeDic[edge].Add(result);
            }
            else
            {
                var list = new ConcurrentBag<SharedEdgeResult>();
                foreach (var result in results)
                    list.Add(result);
                resultViaEdgeDic[edge] = list;
            }
        }

        private bool Has(in Edge edge)
        {
            return resultViaEdgeDic.ContainsKey(edge);
        }

        private void AnalyzeSharedEdge()
        {
            StopWatch watch = new StopWatch();
            watch.Start();

            int trisCount = this.triangles.Count;
            int threadNumber = Mathf.CeilToInt(trisCount * 1.0f / MAXIMUM_THREAD_PROCESSING_CAPACITY);
            Task[] tasks = new Task[threadNumber];
            for (int beginIndex = 0, taskId = 0; beginIndex < trisCount; beginIndex += MAXIMUM_THREAD_PROCESSING_CAPACITY, taskId++)
            {
                int count = MAXIMUM_THREAD_PROCESSING_CAPACITY;
                if (beginIndex + MAXIMUM_THREAD_PROCESSING_CAPACITY > trisCount)
                    count = trisCount - beginIndex;
                int startIndex = beginIndex;
                var task = this.Dispatch(this.triangles, startIndex, count);
                tasks[taskId] = task;
            }
            Task.WaitAll(tasks);
            AnalysisSharedEdges();

            watch.Stop();
            if (this.enableDebugOutput)
                Debug.LogError($"<color=green> elapsed time : {watch.ElapsedMilliseconds} ms</color>");
        }

        private Task Dispatch(List<Triangle> allTris, int beginIndex, int count)
        {
            Task task = new Task(() =>
            { 
                for (int i = beginIndex; i < (beginIndex + count); i ++)
                {
                    Triangle A = allTris[i];
                    if (!Has(A.edgeA))
                    {
                        var repeatEdgeA = this.RetrieveRepeatEdge(i, allTris, in A, in A.edgeA);
                        this.InsertIntoQueryPreparationDic(in A.edgeA, repeatEdgeA);
                    }
                    if (!Has(A.edgeB))
                    {
                        var repeatEdgeB = this.RetrieveRepeatEdge(i, allTris, in A, in A.edgeB);
                        this.InsertIntoQueryPreparationDic(in A.edgeB, repeatEdgeB);
                    }
                    if (!Has(A.edgeC))
                    {
                        var repeatEdgeC = this.RetrieveRepeatEdge(i, allTris, in A, in A.edgeC);
                        this.InsertIntoQueryPreparationDic(in A.edgeC, repeatEdgeC);
                    }
                }
            });
            task.Start();
            return task;
        }

        private List<SharedEdgeResult> RetrieveRepeatEdge(int index, List<Triangle> tris, in Triangle edgeTri, in Edge edge)
        {
            List<SharedEdgeResult> results = new List<SharedEdgeResult>();
            for (int i = 0; i < tris.Count; i++)
            {
                if (index == i)
                    continue;
                Triangle tri = tris[i];

                bool sharedEdge = tri.SharedEdge(in edge);
                if (sharedEdge)
                {
                    SharedEdgeResult result = new SharedEdgeResult();
                    result.sharedEdge = edge;
                    result.cosAngle = tri.CosAngle(edgeTri);
                    if (!results.Contains(result))
                    {
                        result.absCosAngle = System.Math.Abs(result.cosAngle);
                        results.Add(result);
                    }
                    continue;
                }

                if (i < index)
                    continue;

                bool parallel = tri.ParallelWith(in edge, verticalTolerance);
                if (!parallel)
                    continue;


                bool vertical = tri.VerticalWith(edgeTri, verticalTolerance);
                if (!vertical)
                    continue;

                if (tri.ContainEdge(edge, this.toleranceBetweenVertexAndTriangle, this.toleranceVertexInTriangle))
                {
                    SharedEdgeResult result = new SharedEdgeResult();
                    result.sharedEdge = edge;
                    result.cosAngle = tri.CosAngle(edgeTri);
                    if (!results.Contains(result))
                    {
                        result.absCosAngle = System.Math.Abs(result.cosAngle);
                        results.Add(result);
                    }
                }
            }
            return results;
        }

        private void AnalysisSharedEdges()
        {
            List<SharedEdgeResult> sharedEdgeResults = new List<SharedEdgeResult>();
            foreach (var pair in resultViaEdgeDic)
            {
                var res = pair.Value;
                SharedEdgeResult result = new SharedEdgeResult();
                if (this.QueryResult(res, ref result))
                    sharedEdgeResults.Add(result);
            }

            if (sharedEdgeResults.Count <= 0)
                return;

            List<Edge> sharedEdges = new List<Edge>();
            for (int x = 0; x < sharedEdgeResults.Count; x++)
            {
                var result = sharedEdgeResults[x];
                if (result.sharedEdge.length == 0) //剔除长度为0的edge
                    continue;

                if (result.sharedEdge.length < this.ignoreEdgeLengthLessThan)
                    continue;

                bool ignore = this.IsIgnoreTriangles(result.sharedEdge);// 检测是否在需要剔除的三角形中
                if (ignore)
                    continue;

                if (this.ignoreIsolatedEdge)
                {
                    bool isolated = true;
                    for (int y = 0; y < sharedEdgeResults.Count; y++)
                    {
                        if (x == y)
                            continue;
                        if (result.sharedEdge.SharedVertex(sharedEdgeResults[y].sharedEdge))
                        {
                            isolated = false;
                            break;
                        }
                    }

                    if (isolated)
                        continue;
                }

                if (result.cosAngle < minAngleInRadian && result.cosAngle > maxAngleInRadian)
                    sharedEdges.Add(result.sharedEdge);
            }
            this.completed?.Invoke(sharedEdges);
            this.ClearAnalyzer();
        }

        private bool QueryResult(ConcurrentBag<SharedEdgeResult> results, ref SharedEdgeResult searchResult)
        {
            if (results == null || results.Count <= 0)
                return false;

            float maxCosAngle = 0.0f;
            foreach (var item in results)
            { 
                if (item.absCosAngle > maxCosAngle)
                    maxCosAngle = item.absCosAngle;
            }
            if (maxCosAngle >= 1.0f)
                return false;

            results.TryPeek(out searchResult);
            foreach (var nextResult in results)
            {
                if (nextResult.absCosAngle < searchResult.absCosAngle)
                    searchResult = nextResult;
            }
            return true;
        }
    }
}