using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenericStl;
using STLenographer.Data;
using System.IO;

namespace STLenographer {
    public partial class STLenographer : Form {
        public STLenographer() {
            InitializeComponent();
            var list = Encoding.GetEncodings().Select(x => x.Name.ToUpper()).ToList();
            list.Sort();
            comboBox2.Items.AddRange(list.ToArray());
            
            comboBox2.SelectedIndex = list.IndexOf("UTF-8");
        }

        private StlReaderBase<Triangle, Vertex, Normal> reader;
        private StlWriterBase<Triangle, Vertex, Normal> writer;

        private String PathRead {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }
        private String PathWrite {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }

        private bool IsWrite {
            get { return radioButton1.Checked; }
        }

        private Encoding MyEncoding {
            get { return Encoding.GetEncoding(comboBox2.Text); }
        }

        private string showFileDialog(bool mustExist, string previousValue) {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = previousValue ?? "";
            openFileDialog1.CheckFileExists = mustExist;
            openFileDialog1.Filter = "STL files (*.stl)|*.stl|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                return openFileDialog1.FileName;
            }
            return null;
        }
        private void button1_Click(object sender, EventArgs e) {
            PathRead = showFileDialog(true, PathRead) ?? PathRead;
            if (IsWrite) {
                PathWrite = findNextFileName();
            }
        }
 
        private void button2_Click(object sender, EventArgs e) {
            PathWrite = showFileDialog(false, PathWrite) ?? PathWrite;
        }

        private bool doRead() {
            if (!File.Exists(PathRead)) {
                MessageBox.Show("File does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (StlFile.IsBinaryFile(PathRead)) {
                reader = new BinaryStlReader<Triangle, Vertex, Normal>(new DataCreator());
                writer = null;
            } else {
                reader = new AsciiStlReader<Triangle, Vertex, Normal>(new DataCreator());
                writer = null;
            }
            
            ByteReadHelper readHelper = readStenography();
            string msg = "";
            if(readHelper.ReadEverything()) {
                try {
                    textBox3.Text = MyEncoding.GetString(readHelper.Data.ToArray());
                    return true;
                } catch(Exception e) {
                    msg = "\n" + e.Message;
                }
            }
            MessageBox.Show("Could not decode data!" + msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private bool doWrite() {
            if (!File.Exists(PathRead)) {
                MessageBox.Show("Read file does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (File.Exists(PathWrite)) {
                DialogResult result = MessageBox.Show("Write file already exists, overwrite?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No) {
                    return false;
                }
            }
            if (StlFile.IsBinaryFile(PathRead)) {
                reader = new BinaryStlReader<Triangle, Vertex, Normal>(new DataCreator());
                writer = new BinaryStlWriter<Triangle, Vertex, Normal>(new DataExtractor());
            } else {
                reader = new AsciiStlReader<Triangle, Vertex, Normal>(new DataCreator());
                writer = new AsciiStlWriter<Triangle, Vertex, Normal>(new DataExtractor());
            }

            return performStenography(MyEncoding.GetBytes(textBox3.Text));
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            RadioButton rb = sender as RadioButton;
            if (IsWrite) {
                //Write
                button2.Enabled = true;
            } else {
                //Read
                button2.Enabled = false;
                PathWrite = "";
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            Cursor cursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            button3.Enabled = false;
            bool ret = false;
            if (IsWrite) {
                ret = doWrite();
            } else {
                ret = doRead();
            }
            Cursor.Current = cursor;
            button3.Enabled = true;
            if (ret) {
                DialogResult result = MessageBox.Show("Done!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string findNextFileName() {
            int dotPos = PathRead.LastIndexOf('.');
            if(dotPos < 0) {
                return PathWrite;
            }
            string extStr = PathRead.Substring(dotPos);
            string baseStr = PathRead.Substring(0, dotPos);
            
            string tmp = "";
            while(stringEndsWithNumber(baseStr)) {
                tmp = baseStr[baseStr.Length - 1] + tmp;
                baseStr = baseStr.Remove(baseStr.Length - 1);
            }

            int num = int.Parse(string.IsNullOrEmpty(tmp) ? "0" : tmp);

            do {
                num++;
                tmp = baseStr + num + extStr;
            } while (File.Exists(tmp));
            return tmp;
        }

        static bool stringEndsWithNumber(string str) {
            int c = str[str.Length - 1] - '0';
            return c >= 0 && c <= 9;
        }
        
        private bool performStenography(byte[] data) {
            ByteWriteHelper writeHelper = new ByteWriteHelper(checkBox1.Checked, textBox4.Text);
            writeHelper.AppendData(new byte[] { 0x77, 0 }); //Magic byte, version
            writeHelper.AppendData(BitConverter.GetBytes(data.Length));
            writeHelper.AppendData(data);
            writeHelper.FinalizeData();
            
            List<Triangle> triangles = new List<Triangle>();
            Dictionary<Vertex,Vertex> knownVertices = new Dictionary<Vertex,Vertex>();
            foreach ( Triangle tri in reader.ReadFromFile(PathRead)) {
                if (writeHelper.HasData()) {
                    if (!(knownVertices.ContainsKey(tri.V1))) {
                        Vertex tmp = new Vertex(tri.V1);
                        performSteganographyPerVertex(tri.V1, writeHelper);
                        knownVertices.Add(tmp, new Vertex(tri.V1));
                    }
                    if (!(knownVertices.ContainsKey(tri.V2))) {
                        Vertex tmp = new Vertex(tri.V2);
                        performSteganographyPerVertex(tri.V2, writeHelper);
                        knownVertices.Add(tmp, new Vertex(tri.V2));
                    }
                    if (!(knownVertices.ContainsKey(tri.V3))) {
                        Vertex tmp = new Vertex(tri.V3);
                        performSteganographyPerVertex(tri.V3, writeHelper);
                        knownVertices.Add(tmp, new Vertex(tri.V3));
                    }
                }
                
                if (knownVertices.ContainsKey(tri.V1)) {
                    tri.V1.Set(knownVertices[tri.V1]);
                }
                if (knownVertices.ContainsKey(tri.V2)) {
                    tri.V2.Set(knownVertices[tri.V2]);
                }
                if (knownVertices.ContainsKey(tri.V3)) {
                    tri.V3.Set(knownVertices[tri.V3]);
                }
                triangles.Add(tri);
            }
            if (writeHelper.HasData()) {
                MessageBox.Show("The file is to small for the data amount!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } else {
                writer.WriteToFile(PathWrite, triangles);
                return true;
            }
            return false;
        }

        private void performSteganographyPerVertex(Vertex v, ByteWriteHelper writeHelper) {
            if (!writeHelper.HasData()) return;
            v.X = performSteganographyPerFloat(writeHelper.GetAndMoveCurrentBit(), v.X);
            if (!writeHelper.HasData()) return;
            v.Y = performSteganographyPerFloat(writeHelper.GetAndMoveCurrentBit(), v.Y);
            if (!writeHelper.HasData()) return;
            v.Z = performSteganographyPerFloat(writeHelper.GetAndMoveCurrentBit(), v.Z);
        }

        private float performSteganographyPerFloat(bool bit, float val) {
            byte[] bts = BitConverter.GetBytes(val);
            if (bit) {
                bts[0] |= 1;
            } else {
                bts[0] &= 0xFE;
            }            
            return BitConverter.ToSingle(bts, 0);
        }

        public ByteReadHelper readStenography() {
            ByteReadHelper readHelper = new ByteReadHelper(checkBox1.Checked, textBox4.Text);
            HashSet<Vertex> knownVertices = new HashSet<Vertex>();
            foreach (Triangle tri in reader.ReadFromFile(PathRead)) {
                if (!readHelper.ReadEverything()) {
                    if (!(knownVertices.Contains(tri.V1))) {
                        Vertex tmp = new Vertex(tri.V1);
                        readStenographyPerVertex(tri.V1, readHelper);
                        knownVertices.Add(tmp);
                    }
                    if (!(knownVertices.Contains(tri.V2))) {
                        Vertex tmp = new Vertex(tri.V2);
                        readStenographyPerVertex(tri.V2, readHelper);
                        knownVertices.Add(tmp);
                    }
                    if (!(knownVertices.Contains(tri.V3))) {
                        Vertex tmp = new Vertex(tri.V3);
                        readStenographyPerVertex(tri.V3, readHelper);
                        knownVertices.Add(tmp);
                    }
                }
            }

            return readHelper;
        }

        private void readStenographyPerVertex(Vertex v, ByteReadHelper readHelper) {
            if (readHelper.ReadEverything()) return;
            readHelper.SetCurrentBitAndMove(readStenographyByFloat(v.X));
            if (readHelper.ReadEverything()) return;
            readHelper.SetCurrentBitAndMove(readStenographyByFloat(v.Y));
            if (readHelper.ReadEverything()) return;
            readHelper.SetCurrentBitAndMove(readStenographyByFloat(v.Z));
        }

        private bool readStenographyByFloat(float val) {
            byte[] bts = BitConverter.GetBytes(val);
            return (bts[0] & 1) == 1;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            
        }
    }
}
