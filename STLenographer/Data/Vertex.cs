﻿using System;

namespace STLenographer.Data {

    public class Vertex : IEquatable<Vertex> {
        private float _x;
        private float _y;
        private float _z;


        public Vertex(Vertex copy) {
            _x = copy._x;
            _y = copy._y;
            _z = copy._z;
        }

        public Vertex(float x, float y, float z) {
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

        public void Set(Vertex other) {
            _x = other._x;
            _y = other._y;
            _z = other._z;
        }

        public override string ToString() {
            return $"({_x},{_y},{_z})";
        }

        public bool Equals(Vertex other) {
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
            return Equals(other as Vertex);
        }
    }
}
