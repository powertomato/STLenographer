using System;
using System.Collections.Generic;
using System.Text;

namespace STLenographer.Data
{
    public class StenographyReader
    {
        ByteReadHelper readHelper;
        HashSet<Vertex> knownVertices;

        public StenographyReader(ByteReadHelper readHelper)
        {
            this.readHelper = readHelper;
            this.knownVertices = new HashSet<Vertex>();
        }

        public void ReadFromTriangles(IEnumerable<Triangle> triangles)
        {
            foreach (Triangle tri in triangles)
            {
                if (!readHelper.HasReadEverything())
                {
                    if (!(knownVertices.Contains(tri.V1)))
                    {
                        Vertex tmp = new Vertex(tri.V1);
                        readStenographyPerVertex(tri.V1, readHelper);
                        knownVertices.Add(tmp);
                    }
                    if (!(knownVertices.Contains(tri.V2)))
                    {
                        Vertex tmp = new Vertex(tri.V2);
                        readStenographyPerVertex(tri.V2, readHelper);
                        knownVertices.Add(tmp);
                    }
                    if (!(knownVertices.Contains(tri.V3)))
                    {
                        Vertex tmp = new Vertex(tri.V3);
                        readStenographyPerVertex(tri.V3, readHelper);
                        knownVertices.Add(tmp);
                    }
                }
            }
        }

        private void readStenographyPerVertex(Vertex v, ByteReadHelper readHelper)
        {
            if (readHelper.HasReadEverything()) return;
            readHelper.SetCurrentBitAndMove(readStenographyByFloat(v.X));
            if (readHelper.HasReadEverything()) return;
            readHelper.SetCurrentBitAndMove(readStenographyByFloat(v.Y));
            if (readHelper.HasReadEverything()) return;
            readHelper.SetCurrentBitAndMove(readStenographyByFloat(v.Z));
        }

        private bool readStenographyByFloat(float val)
        {
            byte[] bts = BitConverter.GetBytes(val);
            return (bts[0] & 1) == 1;
        }

        public String GetString(Encoding encoding)
        {   if (readHelper.HasReadEverything())
            {
                return encoding.GetString(readHelper.Data.ToArray());
            } else
            {
                throw new InvalidOperationException("Data seems to be incomplete!");
            }
        }
    }
}
