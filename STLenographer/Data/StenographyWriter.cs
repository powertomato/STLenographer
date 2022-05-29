using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLenographer.Data
{
    class StenographyWriter
    {
        ByteWriteHelper writeHelper;
        readonly List<Triangle> triangles;
        readonly Dictionary<Vertex, Vertex> knownVertices;

        public StenographyWriter(ByteWriteHelper writeHelper)
        {
            this.writeHelper = writeHelper;
            this.knownVertices = new Dictionary<Vertex, Vertex>();
            this.triangles = new List<Triangle>();
        }

        public List<Triangle> Triangles => triangles;

        public bool HasUnencodedData => writeHelper.HasUnencodedData();

        public void AddTriangles(IEnumerable<Triangle> triangles)
        {
            foreach (Triangle tri in triangles) {
                AddTriangle(tri);
            }
        }

        public void AddTriangle(Triangle tri)
        {
            if (writeHelper.HasUnencodedData())
            {
                if (!(knownVertices.ContainsKey(tri.V1)))
                {
                    Vertex tmp = new Vertex(tri.V1);
                    performSteganographyPerVertex(tri.V1, writeHelper);
                    knownVertices.Add(tmp, new Vertex(tri.V1));
                }
                if (!(knownVertices.ContainsKey(tri.V2)))
                {
                    Vertex tmp = new Vertex(tri.V2);
                    performSteganographyPerVertex(tri.V2, writeHelper);
                    knownVertices.Add(tmp, new Vertex(tri.V2));
                }
                if (!(knownVertices.ContainsKey(tri.V3)))
                {
                    Vertex tmp = new Vertex(tri.V3);
                    performSteganographyPerVertex(tri.V3, writeHelper);
                    knownVertices.Add(tmp, new Vertex(tri.V3));
                }
            }

            if (knownVertices.ContainsKey(tri.V1))
            {
                tri.V1.Set(knownVertices[tri.V1]);
            }
            if (knownVertices.ContainsKey(tri.V2))
            {
                tri.V2.Set(knownVertices[tri.V2]);
            }
            if (knownVertices.ContainsKey(tri.V3))
            {
                tri.V3.Set(knownVertices[tri.V3]);
            }
            triangles.Add(tri);
        }

        private void performSteganographyPerVertex(Vertex v, ByteWriteHelper writeHelper)
        {
            if (!writeHelper.HasUnencodedData()) return;
            v.X = performSteganographyPerFloat(writeHelper.GetAndMoveCurrentBit(), v.X);
            if (!writeHelper.HasUnencodedData()) return;
            v.Y = performSteganographyPerFloat(writeHelper.GetAndMoveCurrentBit(), v.Y);
            if (!writeHelper.HasUnencodedData()) return;
            v.Z = performSteganographyPerFloat(writeHelper.GetAndMoveCurrentBit(), v.Z);
        }

        private float performSteganographyPerFloat(bool bit, float val)
        {
            byte[] bts = BitConverter.GetBytes(val);
            if (bit)
            {
                bts[0] |= 1;
            }
            else
            {
                bts[0] &= 0xFE;
            }
            return BitConverter.ToSingle(bts, 0);
        }

    }
}
