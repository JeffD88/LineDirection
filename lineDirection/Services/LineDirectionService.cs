using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;

namespace LineDirection.Services
{
    enum OutputType
    {
        Vector,
        Quaternion
    }

    class LineDirectionService : ILineDirectionService
    {

        #region Fields

        private List<string> lineData = null;

        #endregion

        #region Structures

        struct Line
        {
            public double StartPointX, StartPointY, StartPointZ, EndPointX, EndPointY, EndPointZ;

            public Line(double startPointX, double startPointY, double startPointZ, double endPointX, double endPointY, double endPointZ)
            {
                this.StartPointX = startPointX;
                this.StartPointY = startPointY;
                this.StartPointZ = startPointZ;

                this.EndPointX = endPointX;
                this.EndPointY = endPointY;
                this.EndPointZ = endPointZ;
            }

            public override string ToString()
            {
                return $"{Math.Round(StartPointX, 8)},{Math.Round(StartPointY, 8)},{Math.Round(StartPointZ, 8)}" +
                       $"{Math.Round(EndPointX, 8)},{Math.Round(EndPointY, 8)},{Math.Round(EndPointZ, 8)}";
            }
        }

        struct EulerAngles
        {
            public double Precession, Nutation, Rotation;

            public EulerAngles(double precession, double nutation, double rotation)
            {
                this.Precession = precession;
                this.Nutation = nutation;
                this.Rotation = rotation;
            }

            public override string ToString()
            {
                return $"{Math.Round(Precession, 8)},{Math.Round(Nutation, 8)},{Math.Round(Rotation, 8)}";
            }
        }

        struct Quaternion
        {
            public double W, X, Y, Z;

            public Quaternion(double w, double x, double y, double z)
            {
                this.W = w;
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            public override string ToString()
            {
                return $"{Math.Round(W, 8)},{Math.Round(X, 8)},{Math.Round(Y, 8)},{Math.Round(Z, 8)}";
            }
        }

        #endregion

        #region Constructor

        public LineDirectionService()
        {
            lineData = new List<string>();
        }

        #endregion

        #region Public Methods

        public bool ProcessLinesInFile(string selectedFile, OutputType outputType)
        {
            var lines = ParseLines(selectedFile);

            if (lines.Count() > 0)
            {
                foreach (var line in lines)
                {

                    var startPoint = new Vector3D(line.StartPointX, line.StartPointY, line.StartPointZ);
                    var endPoint = new Vector3D(line.EndPointX, line.EndPointY, line.EndPointZ);

                    var vector = Vector3D.Subtract(endPoint, startPoint);

                    vector.Normalize();

                    switch (outputType)
                    {
                        case OutputType.Vector:
                            lineData.Add($"{Math.Round(startPoint.X, 8)},{Math.Round(startPoint.Y, 8)},{Math.Round(startPoint.Z, 8)}," +
                                         $"{Math.Round(vector.X, 8)},{Math.Round(vector.Y, 8)},{Math.Round(vector.Z, 8)}");
                            break;

                        case OutputType.Quaternion:
                            var eulerAngles = CaculateEulerAngles(vector);
                            var quaternion = ConvertEulerAnglesToQuaternion(eulerAngles);

                            lineData.Add($"{Math.Round(startPoint.X, 8)},{Math.Round(startPoint.Y, 8)},{Math.Round(startPoint.Z, 8)}," +
                                         $"{quaternion.ToString()}");
                            break;

                        default:
                            break;
                    }               
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public string WriteCSV(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                foreach (var line in lineData)
                {
                    writer.WriteLine(line);
                }
            }
            return path;
        }

        #endregion

        #region Private Methods

        private List<Line> ParseLines(string selectedFile)
        {
            var Lines = new List<Line>();
            var currentLine = "";

            using (var file = new StreamReader(selectedFile))
            {
                while ((currentLine = file.ReadLine()) != null)
                {
                    var lineData = Array.ConvertAll(currentLine.Split(','), Double.Parse);
                    Lines.Add(new Line(lineData[0], lineData[1], lineData[2],
                                       lineData[3], lineData[4], lineData[5]));
                }
            }

            return Lines;
        }

        private EulerAngles CaculateEulerAngles(Vector3D zVector)
        {
            var xVector = Vector3D.CrossProduct(zVector, new Vector3D(zVector.X, zVector.Y, 0.0));
            var yVector = Vector3D.CrossProduct(zVector, xVector);

            xVector.Normalize();
            yVector.Normalize();

            double precession, nutation, rotation;

            var xZyZComponents = Math.Sqrt((xVector.Z * xVector.Z) + (yVector.Z * yVector.Z));

            if (xZyZComponents > Double.Epsilon)
            {
                precession = Math.Atan2(((xVector.Y * yVector.Z) - (yVector.Y * xVector.Z)), ((xVector.X * zVector.Y) - (yVector.X * xVector.Z)));
                nutation = Math.Atan2(xZyZComponents, zVector.Z);
                rotation = -Math.Atan2(-xVector.Z, yVector.Z);
            }
            else
            {
                precession = 0;
                nutation = (zVector.Z > 0) ? 0 : Math.PI;
                rotation = -Math.Atan2(yVector.X, xVector.X);
            }

            return new EulerAngles(precession, nutation, rotation);
        }

        private Quaternion ConvertEulerAnglesToQuaternion(EulerAngles euler)
        {
            var c1 = Math.Cos(euler.Precession / 2);
            var s1 = Math.Sin(euler.Precession / 2);
            var c2 = Math.Cos(euler.Nutation / 2);
            var s2 = Math.Sin(euler.Nutation / 2);
            var c3 = Math.Cos(euler.Rotation / 2);
            var s3 = Math.Sin(euler.Rotation / 2);

            var c1c2 = c1 * c2;
            var s1s2 = s1 * s2;

            var w = (c1c2 * c3) - (s1s2 * s3);
            var x = (s1 * c2 * c3) + (c1 * s2 * s3);
            var y = (c1 * s2 * c3) - (s1 * c2 * s3);
            var z = (c1c2 * s3) + (s1s2 * c3);

            return new Quaternion(w, x, y, z);
        }

            #endregion

    }
}
