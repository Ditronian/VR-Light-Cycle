using System;

namespace S.Wireframe
{
    public struct Vec3
    {
        public float x, y, z;

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        public static float Magnitude(in Vec3 a)
        {
            return a.Magnitude();
        }

        public static float Distance(in Vec3 a, in Vec3 b)
        {
            Vec3 vec3 = a - b;
            return vec3.Magnitude();
        }

        public static Vec3 Normalize(in Vec3 vec3)
        {
            float magnitude = vec3.Magnitude();
            return vec3 / magnitude;
        }

        public static Vec3 Cross(in Vec3 a, in Vec3 b)
        {
            return new Vec3
                (
                    a.y * b.z - a.z * b.y,
                    a.z * b.x - a.x * b.z,
                    a.x * b.y - a.y * b.x
                );
        }

        public static float Dot(in Vec3 a, in Vec3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static bool operator ==(in Vec3 a, in Vec3 b)
        {
            return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
        }

        public static bool operator !=(in Vec3 a, in Vec3 b)
        {
            return !(a == b);
        }

        public static Vec3 operator +(in Vec3 a, in Vec3 b)
        {
            return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vec3 operator -(in Vec3 a)
        {
            return new Vec3(-a.x,-a.y,-a.z);
        }

        public static Vec3 operator -(in Vec3 a, in Vec3 b)
        {
            return new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vec3 operator /(in Vec3 a,float dividend )
        {
            return new Vec3(a.x / dividend, a.y / dividend, a.z / dividend);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{x},{y},{z}";
        }
    }

    public struct Vertex
    {
        public Vec3 pos;
        public Vec3 normal;
         
        public override bool Equals(object obj)
        {
            return this == (Vertex)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(Vertex a, Vertex b)
        {
            float dis = Vec3.Distance(a.pos,b.pos);
            return a.pos == b.pos || dis < 0.2f;
        }

        public static bool operator !=(Vertex a, Vertex b)
        {
            return !(a == b);
        }
    }

    public struct Edge
    {
        public Vertex A;
        public Vertex B;

        public Vec3 VectorAB;
        public float length;

        public Edge(Vertex a,Vertex b)
        {
            this.A = a;
            this.B = b;
            this.VectorAB = this.B.pos - this.A.pos;
            this.length = Vec3.Distance(A.pos, B.pos);
        }

        public bool SharedVertex( in Edge edge )
        {
            return A.pos == edge.A.pos || A.pos == edge.B.pos || B.pos == edge.A.pos || B.pos == edge.B.pos;
        }

        public bool BelongTriangle( Triangle tri )
        {
            return tri.edgeA == this || tri.edgeB == this || tri.edgeC == this;
        }

        public override bool Equals(object obj)
        {
            Edge edgeB = (Edge)obj;
            return this == edgeB;
        }

        public static bool operator ==(Edge a, Edge b)
        {
            bool b1 = a.A.pos == b.A.pos && a.B.pos == b.B.pos;
            if (b1) return true;
            bool b2 = a.A.pos == b.B.pos && a.B.pos == b.A.pos;
            return b2;
        }

        public static bool operator !=(Edge a, Edge b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"point a :[{this.A.pos.x.ToString("F7")},{this.A.pos.y.ToString("F7")},{this.A.pos.z.ToString("F7")}] ," +
                $"point b:[{this.B.pos.x.ToString("F7")},{this.B.pos.y.ToString("F7")},{this.B.pos.z.ToString("F7")}]";
        }
    }

    public struct Triangle
    {
        public Vertex A;
        public Vertex B;
        public Vertex C;

        public Edge edgeA;
        public Edge edgeB;
        public Edge edgeC;

        public Vec3 FaceNormal;

        public Triangle(Vec3 a, Vec3 b, Vec3 c)
        {
            this.A = new Vertex() { pos = a };
            this.B = new Vertex() { pos = b };
            this.C = new Vertex() { pos = c };

            this.edgeA = new Edge(this.A, this.B);
            this.edgeB = new Edge(this.B, this.C);
            this.edgeC = new Edge(this.C, this.A);

            Vec3 AB = this.B.pos - this.A.pos;
            Vec3 AC = this.C.pos - this.A.pos;

            this.FaceNormal = Vec3.Normalize(Vec3.Cross(AB, AC));
        }

        public float Distance(in Vertex point)
        {
            Vec3 v = point.pos - this.A.pos;
            float distance = Math.Abs(Vec3.Dot(v, this.FaceNormal));
            return distance;
        }

        public bool VerticalWith(Triangle tri, float tolerance = 0.0f)
        { 
            var cosAngle = Math.Abs(Vec3.Dot(this.FaceNormal, tri.FaceNormal));
            return cosAngle <= tolerance;
        }

        public float CosAngle(Triangle tri)
        {
            return Vec3.Dot(this.FaceNormal, tri.FaceNormal);
        }

        public bool ParallelWith(in Edge edge, float verticalTolerance = 0.0f)
        { 
            Vec3 vEdge = Vec3.Normalize(edge.VectorAB);
            float cosAngle = Math.Abs(Vec3.Dot(vEdge, this.FaceNormal));
            bool parallel = cosAngle <= verticalTolerance;
            return parallel;
        }

        public bool SharedEdge(Triangle tri)
        {
            return this.edgeA == tri.edgeA ||
                   this.edgeA == tri.edgeB ||
                   this.edgeA == tri.edgeC ||
                   this.edgeB == tri.edgeB ||
                   this.edgeB == tri.edgeC ||
                   this.edgeC == tri.edgeC;
        }

        public bool SharedEdge(in Edge edge)
        {
            if (this.edgeA == edge)
                return true;
            if (this.edgeB == edge)
                return true;
            if (this.edgeC == edge)
                return true;
            return false;
        }

        public bool TryGetSharedEdge(Triangle tri, System.Collections.Generic.List<Edge> edges)
        { 
            bool share = this.edgeA == tri.edgeA || this.edgeA == tri.edgeB || this.edgeA == tri.edgeC;
            if (share) edges.Add(edgeA);
            share = this.edgeB == tri.edgeB || this.edgeB == tri.edgeC || this.edgeB == tri.edgeA;
            if (share) edges.Add(edgeB);
            share = this.edgeC == tri.edgeC || this.edgeC == tri.edgeA || this.edgeC == tri.edgeB;
            if (share) edges.Add(edgeC);
            return edges.Count > 0;
        }

        public bool TryGetSharedEdge(Triangle tri, ref Edge edge)
        {
            if (this.SharedEdge(tri.edgeA))
            {
                edge = tri.edgeA;
                return true;
            }
            if (this.SharedEdge(tri.edgeB))
            {
                edge = tri.edgeB;
                return true;
            }
            if (this.SharedEdge(tri.edgeC))
            {
                edge = tri.edgeC;
                return true;
            }
            return false;
        }

        public bool ContainEdge(in Edge edge, float distanceTolerance = 0.0f, float sameSideTolerance = 0.0f)
        {
            float dis = this.Distance(in edge.A);
            if (dis > distanceTolerance)
                return false;
            dis = this.Distance(in edge.A);
            if (dis > distanceTolerance)
                return false;
            bool contain = this.ContainVertex(in edge.A, sameSideTolerance);
            if (!contain)
                return false;
            contain = this.ContainVertex(in edge.B, sameSideTolerance);
            return contain;
        }

        public bool ContainVertex(in Vertex point, float tolerance = 0.0f)
        {
            bool ss1 = SameSide(A.pos, B.pos, C.pos, point.pos, tolerance);
            bool ss2 = SameSide(B.pos, C.pos, A.pos, point.pos, tolerance);
            bool ss3 = SameSide(C.pos, A.pos, B.pos, point.pos, tolerance);
            return ss1 && ss2 && ss3;
        }

        private bool SameSide(Vec3 A, Vec3 B, Vec3 C, Vec3 P, float tolerance = 0.0f)
        {
            Vec3 AB = B - A;
            Vec3 AC = C - A;
            Vec3 AP = P - A;
            Vec3 v1 = Vec3.Cross(AB, AC);
            Vec3 v2 = Vec3.Cross(AB, AP);

            bool sameside = Vec3.Dot(v1, v2) >= 0;
            if (!sameside)
            {
                float cosangle = Vec3.Dot(Vec3.Normalize(AB), Vec3.Normalize(AP));
                float pd = (float)Math.Sin(Math.Acos(cosangle)) * Vec3.Magnitude(AP);
                sameside = pd <= tolerance;
            }
            return sameside;
        }

        public bool Coplane( Triangle tri ,float parallelTolerance = 0.0f , float distanceToTriangleTolerance = 0.0f)
        {
            float cosangle = Math.Abs(Vec3.Dot(tri.FaceNormal, this.FaceNormal));

            if (cosangle >= parallelTolerance)
            {
                bool contain =
                    this.ContainVertex(tri.A, distanceToTriangleTolerance) &&
                    this.ContainVertex(tri.B, distanceToTriangleTolerance) &&
                    this.ContainVertex(tri.C, distanceToTriangleTolerance);
                return contain;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return this == (Triangle)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return
                $"point a:[{this.A.pos.x.ToString("F7")},{this.A.pos.y.ToString("F7")},{this.A.pos.z.ToString("F7")}] ," +
                $"point b:[{this.B.pos.x.ToString("F7")},{this.B.pos.y.ToString("F7")},{this.B.pos.z.ToString("F7")}]" +
                $"point b:[{this.C.pos.x.ToString("F7")},{this.C.pos.y.ToString("F7")},{this.C.pos.z.ToString("F7")}]" +
                $"edge A length {this.edgeA.length}" +
                $"edge B length {this.edgeB.length}" +
                $"edge C length {this.edgeC.length}";
        }

        public static bool operator ==(Triangle a, Triangle b)
        { 
            return
                a.A == b.A && a.B == b.B && a.C == b.C ||
                a.A == b.A && a.B == b.C && a.C == b.B ||
                a.A == b.B && a.B == b.C && a.C == b.A ||
                a.A == b.B && a.B == b.A && a.C == b.C ||
                a.A == b.C && a.B == b.A && a.C == b.B ||
                a.A == b.C && a.B == b.B && a.C == b.C;
        }

        public static bool operator !=(Triangle a, Triangle b)
        {
            return !(a == b);
        }

        public bool IsOxygon(float maxCosAngle)
        {
            bool isOxygon = false;

            Vec3 AB = Vec3.Normalize(this.B.pos - this.A.pos);
            Vec3 AC = Vec3.Normalize(this.C.pos - this.A.pos);
            float cosBAC = Math.Abs(Vec3.Dot(AB, AC));
            isOxygon = cosBAC > maxCosAngle;
            if (isOxygon)
                return true;

            Vec3 BA = -AB;
            Vec3 BC = Vec3.Normalize(this.C.pos - this.B.pos);
            float cosABC = Math.Abs(Vec3.Dot(BA, BC));
            isOxygon = cosABC > maxCosAngle;
            if (isOxygon)
                return true;

            Vec3 CA = -AC;
            Vec3 CB = -BC;
            float cosACB = Math.Abs(Vec3.Dot(CA, CB));
            isOxygon = cosACB > maxCosAngle;
            if (isOxygon)
                return true;

            return isOxygon;
        }
    }

    public struct SharedEdgeResult
    {
        public Edge sharedEdge;
        public float cosAngle;
        public float absCosAngle;

        public override bool Equals(object obj)
        {
            SharedEdgeResult rp = (SharedEdgeResult)obj;
            return cosAngle == rp.cosAngle && this.sharedEdge.A == rp.sharedEdge.A && this.sharedEdge.B == rp.sharedEdge.B;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class WireframeData
    {
        public string srcName;
        public UnityEngine.Transform srcTransform;
        public UnityEngine.Mesh mesh;
    }

    public static class MathHelper
    {
        public static UnityEngine.Vector3 ToVector3( in Vec3 vec3 )
        {
            return new UnityEngine.Vector3(vec3.x,vec3.y,vec3.z);
        }

        public static Vec3 ToVec3( in UnityEngine.Vector3 vector3 )
        {
            return new Vec3(vector3.x,vector3.y,vector3.z);
        }
    }
}