using System;

namespace STLenographer.Data {
    public class Triangle : IEquatable<Triangle> {
        private readonly Normal _n;
        private readonly Vertex _v1;
        private readonly Vertex _v2;
        private readonly Vertex _v3;

        public Triangle(Vertex v1, Vertex v2, Vertex v3, Normal n) {
            if (v1 == null) throw new ArgumentNullException("v1");
            if (v2 == null) throw new ArgumentNullException("v2");
            if (v3 == null) throw new ArgumentNullException("v3");
            if (n == null) throw new ArgumentNullException("n");

            _v1 = v1;
            _v2 = v2;
            _v3 = v3;
            _n = n;
        }

        public Normal N {
            get { return _n; }
        }

        public Vertex V1 {
            get { return _v1; }
        }

        public Vertex V2 {
            get { return _v2; }
        }

        public Vertex V3 {
            get { return _v3; }
        }

        public override string ToString() {
            return $"V1: {_v1.ToString()}, V1: {_v2.ToString()}, V1: {_v3.ToString()}, N: {_n.ToString()}";
        }

        public bool Equals(Triangle other) {
            if (other == null) {
                return false;
            }

            return other.V1.Equals(V1) && other.V2.Equals(V2) && other.V3.Equals(V3) && other.N.Equals(N);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (V1 != null ? V1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (V2 != null ? V2.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (V3 != null ? V3.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (N != null ? N.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj) {
            return Equals(obj as Triangle);
        }
    }
}
