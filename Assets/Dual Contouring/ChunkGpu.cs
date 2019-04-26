using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Dual_Contouring
{
    [Serializable]
    public class ChunkGpu
    {
        public Vector3 Position;
        public Vector3 Size;

        public Voxel[] Voxels;
        public Vector3[] FeaturePoints;

        public ComputeShader ComputeShader;
        public ComputeBuffer VoxelBuffer;
        public ComputeBuffer FeaturePointBuffer;

        public ComputeBuffer VerticesBuffer;
        public ComputeBuffer VerticesArgumentBuffer;
        public ComputeBuffer TrisBuffer;
        public ComputeBuffer TrisArgumentBuffer;

        public ChunkGpu(Vector3 position, Vector3 size)
        {
            Position = position;
            Size = size;

            Voxels = new Voxel[(int)size.x * (int)size.y * (int)size.z];
            FeaturePoints = new Vector3[(int)size.x * (int)size.y * (int)size.z];

            VoxelBuffer = new ComputeBuffer(Voxels.Length, Marshal.SizeOf<Voxel>());
            FeaturePointBuffer = new ComputeBuffer(Voxels.Length, Marshal.SizeOf<Vector3>());

            var indirectArray = new[] { 0, 1, 0, 0 };

            VerticesBuffer = new ComputeBuffer(Voxels.Length, Marshal.SizeOf<Vector3>(), ComputeBufferType.Append);
            VerticesArgumentBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            VerticesArgumentBuffer.SetData(indirectArray);

            TrisBuffer = new ComputeBuffer(Voxels.Length, sizeof(int), ComputeBufferType.Append);
            TrisArgumentBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            TrisArgumentBuffer.SetData(indirectArray);
        }

        public int GetIndex(Vector3 position)
        {
            return (int)(Size.z * position.z + position.y * (Size.x * Size.z) + position.x);
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
                        Voxels[GetIndex(new Vector3(x, y, z))].Density = Sphere(new Vector3(x, y, z));
                    }
                }
            }
        }

        public float Sphere(Vector3 position)
        {
            var spherePosition = new Vector3(Size.x / 2, Size.y / 2, Size.z / 2);
            var sphereRadius = Size.x / 4;

            return Mathf.Sqrt(Mathf.Pow(position.x - spherePosition.x, 2) +
                              Mathf.Pow(position.y - spherePosition.y, 2) +
                              Mathf.Pow(position.z - spherePosition.z, 2)) - sphereRadius;
        }

        public Mesh ConstructMesh()
        {
            var mesh = new Mesh();

            VoxelBuffer.SetData(Voxels);

            ComputeShader.SetVector("Size", Size);
            ComputeShader.SetBuffer(0, "Voxels", VoxelBuffer);
            ComputeShader.SetBuffer(0, "FeaturePoints", FeaturePointBuffer);
            ComputeShader.Dispatch(0, (int)Size.x, (int)Size.y, (int)Size.z);

            FeaturePointBuffer.GetData(FeaturePoints);

            ComputeShader.SetBuffer(1, "FeaturePoints", FeaturePointBuffer);
            ComputeShader.SetBuffer(1, "Vertices", VerticesBuffer);
            ComputeShader.Dispatch(1, (int)Size.x, (int)Size.y, (int)Size.z);

            ComputeBuffer.CopyCount(VerticesBuffer, VerticesArgumentBuffer, 0);

            var verticeArguments = new int[4];
            VerticesArgumentBuffer.GetData(verticeArguments);
            Debug.Log(verticeArguments[0]);

            var vertices = new Vector3[verticeArguments[0]];
            VerticesBuffer.GetData(vertices);

            ComputeShader.SetBuffer(2, "Vertices", VerticesBuffer);
            ComputeShader.SetBuffer(2, "Tris", TrisBuffer);
            ComputeShader.Dispatch(2, verticeArguments[0]/4, 1, 1);

            ComputeBuffer.CopyCount(TrisBuffer, TrisArgumentBuffer, 0);

            var trisArguments = new int[4];
            TrisArgumentBuffer.GetData(trisArguments);
            Debug.Log(trisArguments[0]);

            var tris = new int[trisArguments[0]];
            TrisBuffer.GetData(tris);
            
            mesh.vertices = vertices;
            mesh.triangles = tris;

            return mesh;
        }

        private void OnDestroy()
        {
            VoxelBuffer.Release();
            FeaturePointBuffer.Release();

            TrisBuffer.Release();
            VerticesBuffer.Release();
        }
    }
}