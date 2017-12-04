using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericStl;

namespace STLenographer.Data {
    class Helper {
        public static Vertex CreateVertex(float x, float y, float z) {
            return new Vertex(x, y, z);
        }

        public static Normal CreateNormal(float x, float y, float z) {
            return new Normal(x, y, z);
        }

        public static Triangle CreateTriangle(Vertex a, Vertex b, Vertex c, Normal n) {
            return new Triangle(a, b, c, n);
        }

        public static Tuple<Vertex, Vertex, Vertex, Normal> ExtractTriangle(Triangle t) {
            return new Tuple<Vertex, Vertex, Vertex, Normal>(t.V1, t.V2, t.V3, t.N);
        }

        public static Tuple<float, float, float> ExtractVertex(Vertex v) {
            return new Tuple<float, float, float>(v.X, v.Y, v.Z);
        }

        public static Tuple<float, float, float> ExtractNormal(Normal n) {
            return new Tuple<float, float, float>(n.X, n.Y, n.Z);
        }
    }

    class DataExtractor : IDataStructureExtractor<Triangle, Vertex, Normal> {
        public Tuple<float, float, float> ExtractNormal(Normal normal) {
            return Helper.ExtractNormal(normal);
        }

        public Tuple<Vertex, Vertex, Vertex, Normal> ExtractTriangle(Triangle triangle1) {
            return Helper.ExtractTriangle(triangle1);
        }

        public Tuple<float, float, float> ExtractVertex(Vertex vertex) {
            return Helper.ExtractVertex(vertex);
        }
    }

    class DataCreator : IDataStructureCreator<Triangle, Vertex, Normal> {
        public Normal CreateNormal(float x, float y, float z) {
            return Helper.CreateNormal(x, y, z);
        }

        public Triangle CreateTriangle(Vertex v1, Vertex v2, Vertex v3, Normal n) {
            return Helper.CreateTriangle(v1, v2, v3, n);
        }

        public Vertex CreateVertex(float x, float y, float z) {
            return Helper.CreateVertex(x, y, z);
        }
    }
}
