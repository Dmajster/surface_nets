using System;
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


        public Vector3[] Vertices;
        public int[] Indices;


        public ComputeShader ComputeShader;
        public ComputeBuffer VoxelBuffer;
        public ComputeBuffer FeaturePointBuffer;

        public ComputeBuffer QuadsBuffer;
        public ComputeBuffer VerticesArgumentBuffer;
        public ComputeBuffer IndicesBuffer;


        public ChunkGpu(Vector3 position, Vector3 size)
        {
            Position = position;
            Size = size;

            Voxels = new Voxel[(int)size.x * (int)size.y * (int)size.z];
            FeaturePoints = new Vector3[(int)size.x * (int)size.y * (int)size.z];

            VoxelBuffer = new ComputeBuffer(Voxels.Length, Marshal.SizeOf<Voxel>());
            FeaturePointBuffer = new ComputeBuffer(Voxels.Length, Marshal.SizeOf<Vector3>());

            var indirectArray = new[] { 0, 1, 0, 0 };

            QuadsBuffer = new ComputeBuffer(Voxels.Length * 4, Marshal.SizeOf<Vector3>(), ComputeBufferType.Append);
            VerticesArgumentBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            VerticesArgumentBuffer.SetData(indirectArray);
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

            //PART 1 - FIGURE OUT MESH VERTICES
            VoxelBuffer.SetData(Voxels); //Put the chunk data in the gpu buffer
            ComputeShader.SetVector("Size", Size); //Set the chunk dimensions on the gpu
            ComputeShader.SetBuffer(0, "Voxels", VoxelBuffer); //Bind the gpu buffer t
            ComputeShader.SetBuffer(0, "FeaturePoints", FeaturePointBuffer); //Bind the output buffer to the 0th kernel
            ComputeShader.Dispatch(0, (int)Size.x, (int)Size.y, (int)Size.z); //Dispatch the task to cores, one for each voxel of the mesh? 

            FeaturePointBuffer.GetData(FeaturePoints); //TODO: COMMENT OUT, USED ONLY FOR OUTPUT VISUALIZATION 

            //PART 2 - CONSTRUCT THE MESH QUADS 
            ComputeShader.SetBuffer(1, "FeaturePoints", FeaturePointBuffer); //Bind the output buffer of kernel 0 to kernel 1
            ComputeShader.SetBuffer(1, "Quads", QuadsBuffer); //Output buffer containing actual quads of the mesh
            ComputeShader.Dispatch(1, (int)Size.x, (int)Size.y, (int)Size.z); //Dispatch the task to cores, one for each voxel of the mesh? 

            ComputeBuffer.CopyCount(QuadsBuffer, VerticesArgumentBuffer, 0); //copy the indirect arguments to indirect buffer

            var verticeArguments = new int[4]; //make a array to store indirect arguments
            VerticesArgumentBuffer.GetData(verticeArguments); //copy data from indirect buffer to array

            //Debug.Log(verticeArguments[0]);

            var quadCount = verticeArguments[0]; //verticeArguments[0] contains count of quads
            var verticeCount = quadCount * 4; //each quad contains 4 vertices
            Vertices = new Vector3[verticeCount]; //create new vertice array for the new mesh
            QuadsBuffer.GetData(Vertices); //store vertices

            //PART 3 - CONSTRUCT THE MESH INDICES
            var indicesCount = quadCount * 6;
            IndicesBuffer = new ComputeBuffer(indicesCount, sizeof(int));
            ComputeShader.SetBuffer(2, "Quads", QuadsBuffer);
            ComputeShader.SetBuffer(2, "Indices", IndicesBuffer);
            ComputeShader.Dispatch(2, quadCount, 1, 1);

            Indices = new int[indicesCount];
            IndicesBuffer.GetData(Indices);

            mesh.vertices = Vertices;
            mesh.triangles = Indices;
            mesh.RecalculateNormals();

            Debug.Log("gpu vertices: " + mesh.vertices.Length);
            Debug.Log("gpu indices: " + mesh.triangles.Length);

            return mesh;
        }



        private void OnDestroy()
        {
            VoxelBuffer.Release();
            FeaturePointBuffer.Release();

            IndicesBuffer.Release();
            QuadsBuffer.Release();
        }
    }
}