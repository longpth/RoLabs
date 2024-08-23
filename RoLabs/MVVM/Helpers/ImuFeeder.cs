using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics; // For Vector3
using YamlDotNet.RepresentationModel;

namespace Rolabs.MVVM.Helpers
{
    public class ImuFeeder
    {
        public float SampleRateHz { get; private set; } // Sample rate in Hz
        public List<Vector3> AngularVelocities { get; private set; }
        public List<Vector3> Accelerations { get; private set; }

        public ImuFeeder()
        {
            AngularVelocities = new List<Vector3>();
            Accelerations = new List<Vector3>();
        }

        public void LoadSensorInfo(string yamlFilePath)
        {
            using (var reader = new StreamReader(yamlFilePath))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);

                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                var rateHzNode = (YamlScalarNode)mapping.Children[new YamlScalarNode("rate_hz")];

                SampleRateHz = float.Parse(rateHzNode.Value, CultureInfo.InvariantCulture);
            }
        }

        public void LoadData(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    {
                        // Skip header or empty lines
                        continue;
                    }

                    var parts = line.Split(',');

                    if (parts.Length != 7)
                    {
                        throw new FormatException("Invalid data format. Each line must have 7 comma-separated values.");
                    }

                    double w_RS_S_x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    double w_RS_S_y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    double w_RS_S_z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                    double a_RS_S_x = double.Parse(parts[4], CultureInfo.InvariantCulture);
                    double a_RS_S_y = double.Parse(parts[5], CultureInfo.InvariantCulture);
                    double a_RS_S_z = double.Parse(parts[6], CultureInfo.InvariantCulture);

                    var angularVelocity = new Vector3((float)w_RS_S_x, (float)w_RS_S_y, (float)w_RS_S_z);
                    var acceleration = new Vector3((float)a_RS_S_x, (float)a_RS_S_y, (float)a_RS_S_z);

                    AngularVelocities.Add(angularVelocity);
                    Accelerations.Add(acceleration);
                }
            }
        }

        public Vector3 GetAngularVelocity(int index)
        {
            if (index >= 0 && index < AngularVelocities.Count)
            {
                return AngularVelocities[index];
            }
            throw new IndexOutOfRangeException("Index is out of range.");
        }

        public Vector3 GetAcceleration(int index)
        {
            if (index >= 0 && index < Accelerations.Count)
            {
                return Accelerations[index];
            }
            throw new IndexOutOfRangeException("Index is out of range.");
        }
    }
}
