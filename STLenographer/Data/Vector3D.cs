using System;

namespace STLenographer.Data {

    public class Vector3D : IEquatable<Vector3D> {
        private float _x;
        private float _y;
        private float _z;


        public Vector3D(Vector3D copy) {
            _x = copy._x;
            _y = copy._y;
            _z = copy._z;
        }

        public Vector3D(float x, float y, float z) {
            _x = x;
            _y = y;
            _z = z;
        }

        public float X {
            get { return _x; }
            set { _x = value; }
        }

        public float Y {
            get { return _y; }
            set { _y = value; }
        }

        public float Z {
            get { return _z; }
            set { _z = value; }
        }

        public void Set(Vector3D other) {
            _x = other._x;
            _y = other._y;
            _z = other._z;
        }

        public override string ToString() {
            return $"({_x},{_y},{_z})";
        }

        public bool Equals(Vector3D other) {
            if (other == null) {
                return false;
            }

            return Math.Abs(other.X - X) < float.Epsilon && Math.Abs(other.Y - Y) < float.Epsilon && Math.Abs(other.Z - Z) < float.Epsilon;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object other) {
            return Equals(other as Vector3D);
        }

        public static Vector3D operator -(Vector3D a, Vector3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3D Cross(Vector3D a, Vector3D b) => new Vector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

        public void Normalize()
        {
            double len = Math.Sqrt(X * X + Y * Y + Z * Z);
            X = (float) (X / len);
            Y = (float) (Y / len);
            Z = (float) (Z / len);
        }
    }
}
