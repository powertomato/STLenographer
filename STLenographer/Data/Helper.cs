using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericStl;

namespace STLenographer.Data {
    class Helper {
        public static Vector3D CreateVertex(float x, float y, float z) {
            return new Vector3D(x, y, z);
        }

        public static Vector3D CreateNormal(float x, float y, float z) {
            return new Vector3D(x, y, z);
        }

        public static Triangle CreateTriangle(Vector3D a, Vector3D b, Vector3D c, Vector3D n) {
            return new Triangle(a, b, c, n);
        }

        public static Tuple<Vector3D, Vector3D, Vector3D, Vector3D> ExtractTriangle(Triangle t) {
            return new Tuple<Vector3D, Vector3D, Vector3D, Vector3D>(t.V1, t.V2, t.V3, t.N);
        }

        public static Tuple<float, float, float> ExtractVertex(Vector3D v) {
            return new Tuple<float, float, float>(v.X, v.Y, v.Z);
        }

        public static Tuple<float, float, float> ExtractNormal(Vector3D n) {
            return new Tuple<float, float, float>(n.X, n.Y, n.Z);
        }
    }

    class DataExtractor : IDataStructureExtractor<Triangle, Vector3D, Vector3D> {
        public Tuple<float, float, float> ExtractNormal(Vector3D normal) {
            return Helper.ExtractNormal(normal);
        }

        public Tuple<Vector3D, Vector3D, Vector3D, Vector3D> ExtractTriangle(Triangle triangle1) {
            return Helper.ExtractTriangle(triangle1);
        }

        public Tuple<float, float, float> ExtractVertex(Vector3D vertex) {
            return Helper.ExtractVertex(vertex);
        }
    }

    class DataCreator : IDataStructureCreator<Triangle, Vector3D, Vector3D> {
        public Vector3D CreateNormal(float x, float y, float z) {
            return Helper.CreateNormal(x, y, z);
        }

        public Triangle CreateTriangle(Vector3D v1, Vector3D v2, Vector3D v3, Vector3D n) {
            return Helper.CreateTriangle(v1, v2, v3, n);
        }

        public Vector3D CreateVertex(float x, float y, float z) {
            return Helper.CreateVertex(x, y, z);
        }
    }
}
