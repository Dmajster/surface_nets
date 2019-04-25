using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Dual_Contouring
{
    [Serializable]
    public struct Chunk
    {
        public Vector3 Position;
        public Vector3 Size;

        public Dictionary<Vector3, Vector3> FeaturePoints;

        public Voxel[] Voxels;

        public Chunk(Vector3 position, Vector3 size)
        {
            Position = position;
            Size = size;

            FeaturePoints = new Dictionary<Vector3, Vector3>();
            Voxels = new Voxel[(int)size.x * (int)size.y * (int)size.z];
        }

        public int GetIndex(Vector3 position)
        {
            return (int)(Size.z * position.z + position.y * (Size.x * Size.z) + position.x);
        }

        public Vector3 GetPosition(int index)
        {
            return new Vector3(
                (int)(index % Size.z),
                (int)(index / (Size.x * Size.z)),
                (int)(index % (Size.x * Size.z) / Size.z)
            );
        }

        public int Sign(float value) => value > 0 ? 1 : value < 0 ? -1 : 0;

        public void PopulateSphere()
        {
            for (var x = 0; x < Size.x; x++)
            {
                for (var y = 0; y < Size.y; y++)
                {
                    for (var z = 0; z < Size.z; z++)
                    {
                        Voxels[GetIndex(new Vector3(x, y, z))].Density = Torus(new Vector3(x, y, z));
                    }
                }
            }
        }

        public float Torus(Vector3 position)
        {
            var torusPosition = new Vector3(Size.x/2, Size.y/2, Size.z/2);
            var torusRadius = Size.x / 8;
            var torusTubeRadius = torusRadius / 2 * 3;
            return Mathf.Pow(
               Mathf.Sqrt(
                   Mathf.Pow(position.x - torusPosition.x, 2)
                   + Mathf.Pow(position.y - torusPosition.y, 2)
               )
               - torusTubeRadius
               , 2)
           + Mathf.Pow(position.z - torusPosition.z, 2)
           - Mathf.Pow(torusRadius, 2);
        }

        public float Sphere(Vector3 position)
        {
            var spherePosition = new Vector3(Size.x / 2, Size.y / 2, Size.z / 2);
            var sphereRadius = Size.x / 4;

            return Mathf.Sqrt(Mathf.Pow(position.x - spherePosition.x, 2) +
                                    Mathf.Pow(position.y - spherePosition.y, 2) +
                                    Mathf.Pow(position.z - spherePosition.z, 2)) - sphereRadius;
        }

        public bool FeaturePoint(Vector3 position, out Vector3 featurePoint)
        {
            featurePoint = Vector3.zero;

            if (position.x + 1 >= Size.x || position.y + 1 >= Size.y || position.z + 1 >= Size.z)
            {
                return false;
            }

            var indices = new[]
            {
                GetIndex(position),
                GetIndex(position + new Vector3(0,0,1)), //1
                GetIndex(position + new Vector3(0,1,0)),
                GetIndex(position + new Vector3(0,1,1)), //3
                                  
                GetIndex(position + new Vector3(1,0,0)), //4
                GetIndex(position + new Vector3(1,0,1)),
                GetIndex(position + new Vector3(1,1,0)), //6
                GetIndex(position + new Vector3(1,1,1))
            };

            var edges = new[]
            {
                new Edge(indices[0],indices[1]),
                new Edge(indices[0],indices[2]),
                new Edge(indices[0],indices[4]),
                new Edge(indices[1],indices[3]),

                new Edge(indices[1],indices[5]),
                new Edge(indices[2],indices[3]),
                new Edge(indices[2],indices[6]),
                new Edge(indices[4],indices[5]),

                new Edge(indices[4],indices[6]),
                new Edge(indices[3],indices[7]),
                new Edge(indices[6],indices[7]),
                new Edge(indices[5],indices[7]),
            };

            var edgeCrossings = 0;

            for (var i = 0; i < edges.Length; i++)
            {
                var index1 = edges[i].Index1;
                var index2 = edges[i].Index2;
                var position1 = GetPosition(index1);
                var position2 = GetPosition(index2);
                var density1 = Voxels[index1].Density;
                var density2 = Voxels[index2].Density;

                if (Sign(density1) == Sign(density2))
                {
                    continue;
                }

                featurePoint += Vector3.Lerp(position1, position2, Mathf.InverseLerp(density1, density2, 0f));
                edgeCrossings++;
            }

            if (edgeCrossings > 0)
            {
                featurePoint /= edgeCrossings;
            }

            return edgeCrossings > 0;
        }

        public Mesh ConstructMesh()
        {
            var mesh = new Mesh();
            var featurePoints = new Dictionary<Vector3, Vector3>();

            for (var i = 0; i < Voxels.Length; i++)
            {
                var position = GetPosition(i);
                if (FeaturePoint(position, out var featurePoint))
                {
                    featurePoints.Add(position, featurePoint);
                }
            }

            FeaturePoints = featurePoints;

            var vertices = new List<Vector3>();
            var indices = new List<int>();

            foreach (var keyValuePair in featurePoints)
            {
                var k0 = keyValuePair.Key;
                var k1 = k0 + new Vector3Int(1, 0, 0);
                var k2 = k0 + new Vector3Int(1, 0, 1);
                var k3 = k0 + new Vector3Int(0, 0, 1);

                if (featurePoints.ContainsKey(k1) &&
                    featurePoints.ContainsKey(k2) &&
                    featurePoints.ContainsKey(k3))
                {
                    vertices.AddRange(new[]
                    {
                        featurePoints[k0],
                        featurePoints[k1],
                        featurePoints[k2],
                        featurePoints[k3]
                    });

                    if (Voxels[GetIndex(k0 + new Vector3(1, 0, 1))].Density > 0)
                    {
                        indices.AddRange(new[]
                        {
                            vertices.Count - 4,
                            vertices.Count - 3,
                            vertices.Count - 2,

                            vertices.Count - 2,
                            vertices.Count - 1,
                            vertices.Count - 4
                        });
                    }
                    else
                    {
                        indices.AddRange(new[]
                        {
                            vertices.Count - 2,
                            vertices.Count - 3,
                            vertices.Count - 4,

                            vertices.Count - 4,
                            vertices.Count - 1,
                            vertices.Count - 2
                        });
                    }
                }

                k1 = k0 + new Vector3Int(0, 0, 1);
                k2 = k0 + new Vector3Int(0, 1, 1);
                k3 = k0 + new Vector3Int(0, 1, 0);

                if (featurePoints.ContainsKey(k1) &&
                    featurePoints.ContainsKey(k2) &&
                    featurePoints.ContainsKey(k3))
                {
                    vertices.AddRange(new[]
                    {
                        featurePoints[k0],
                        featurePoints[k1],
                        featurePoints[k2],
                        featurePoints[k3]
                    });

                    if (Voxels[GetIndex(k0 + new Vector3(0, 1, 1))].Density > 0)
                    {
                        indices.AddRange(new[]
                        {
                            vertices.Count - 4,
                            vertices.Count - 3,
                            vertices.Count - 2,

                            vertices.Count - 2,
                            vertices.Count - 1,
                            vertices.Count - 4
                        });
                    }
                    else
                    {
                        indices.AddRange(new[]
                        {
                            vertices.Count - 2,
                            vertices.Count - 3,
                            vertices.Count - 4,

                            vertices.Count - 4,
                            vertices.Count - 1,
                            vertices.Count - 2
                        });
                    }
                }

                k1 = k0 + new Vector3Int(0, 1, 0);
                k2 = k0 + new Vector3Int(1, 1, 0);
                k3 = k0 + new Vector3Int(1, 0, 0);

                if (featurePoints.ContainsKey(k1) &&
                    featurePoints.ContainsKey(k2) &&
                    featurePoints.ContainsKey(k3))
                {
                    vertices.AddRange(new[]
                    {
                        featurePoints[k0],
                        featurePoints[k1],
                        featurePoints[k2],
                        featurePoints[k3]
                    });

                    if (Voxels[GetIndex(k0 + new Vector3(1, 1, 0))].Density > 0)
                    {
                        indices.AddRange(new[]
                        {
                            vertices.Count - 4,
                            vertices.Count - 3,
                            vertices.Count - 2,

                            vertices.Count - 2,
                            vertices.Count - 1,
                            vertices.Count - 4
                        });
                    }
                    else
                    {
                        indices.AddRange(new[]
                        {
                            vertices.Count - 2,
                            vertices.Count - 3,
                            vertices.Count - 4,

                            vertices.Count - 4,
                            vertices.Count - 1,
                            vertices.Count - 2
                        });
                    }
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}