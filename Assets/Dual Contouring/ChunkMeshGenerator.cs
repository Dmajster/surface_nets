using System.Drawing;
using System.Runtime.InteropServices;
using Assets.Dual_Contouring.Structs;
using UnityEngine;

namespace Assets.Dual_Contouring
{
    public class ChunkMeshGenerator : MonoBehaviour
    {
        public ComputeShader ComputeShader;

        public ComputeBuffer VoxelBuffer;
        public ComputeBuffer FeaturePointBuffer;
        public ComputeBuffer QuadsBuffer;
        public ComputeBuffer IndicesBuffer;
        public ComputeBuffer VerticesArgumentBuffer;

        public Mesh CreateChunkMesh(Chunk chunk)
        {
            var mesh = new Mesh();

            VoxelBuffer = new ComputeBuffer(chunk.Voxels.Length, Marshal.SizeOf<Voxel>());
            FeaturePointBuffer = new ComputeBuffer(chunk.Voxels.Length, Marshal.SizeOf<Vector3>());
            QuadsBuffer = new ComputeBuffer(chunk.Voxels.Length * 4, Marshal.SizeOf<Vector3>(), ComputeBufferType.Append);
            VerticesArgumentBuffer = new ComputeBuffer(4,sizeof(int), ComputeBufferType.IndirectArguments);

            //PART 1 - FIGURE OUT MESH VERTICES
            VoxelBuffer.SetData(chunk.Voxels); //Put the chunk data in the gpu buffer
            ComputeShader.SetVector("Size", chunk.Size); //Set the chunk dimensions on the gpu
            ComputeShader.SetBuffer(0, "Voxels", VoxelBuffer); //Bind the gpu buffer t
            ComputeShader.SetBuffer(0, "FeaturePoints", FeaturePointBuffer); //Bind the output buffer to the 0th kernel
            ComputeShader.Dispatch(0, (int)chunk.Size.x, (int)chunk.Size.y, (int)chunk.Size.z); //Dispatch the task to cores, one for each voxel of the mesh? 

            //FeaturePointBuffer.GetData(FeaturePoints); //TODO: COMMENT OUT, USED ONLY FOR OUTPUT VISUALIZATION 

            //PART 2 - CONSTRUCT THE MESH QUADS 
            ComputeShader.SetBuffer(1, "Voxels", VoxelBuffer);
            ComputeShader.SetBuffer(1, "FeaturePoints", FeaturePointBuffer); //Bind the output buffer of kernel 0 to kernel 1
            ComputeShader.SetBuffer(1, "Quads", QuadsBuffer); //Output buffer containing actual quads of the mesh
            ComputeShader.Dispatch(1, (int)chunk.Size.x, (int)chunk.Size.y, (int)chunk.Size.z); //Dispatch the task to cores, one for each voxel of the mesh? 

            ComputeBuffer.CopyCount(QuadsBuffer, VerticesArgumentBuffer, 0); //copy the indirect arguments to indirect buffer

            var verticeArguments = new int[4]; //make a array to store indirect arguments
            VerticesArgumentBuffer.GetData(verticeArguments); //copy data from indirect buffer to array

            //Debug.Log(verticeArguments[0]);

            var quadCount = verticeArguments[0]; //verticeArguments[0] contains count of quads

            if (quadCount == 0)
            {
                ReleaseBuffers();
                return mesh;
            }

            var verticeCount = quadCount * 4; //each quad contains 4 vertices
            var vertices = new Vector3[verticeCount]; //create new vertice array for the new mesh
            QuadsBuffer.GetData(vertices); //store vertices

            //PART 3 - CONSTRUCT THE MESH INDICES
            var indicesCount = quadCount * 6;
            IndicesBuffer = new ComputeBuffer(indicesCount, sizeof(int));
            ComputeShader.SetBuffer(2, "Quads", QuadsBuffer);
            ComputeShader.SetBuffer(2, "Indices", IndicesBuffer);
            ComputeShader.Dispatch(2, quadCount, 1, 1);

            var indices = new int[indicesCount];
            IndicesBuffer.GetData(indices);

            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateNormals();

            //Debug.Log("gpu vertices: " + mesh.vertices.Length);
            //Debug.Log("gpu indices: " + mesh.triangles.Length);

            ReleaseBuffers();

            return mesh;
        }

        public void ReleaseBuffers()
        {
            VoxelBuffer?.Release();
            FeaturePointBuffer?.Release();
            QuadsBuffer?.Release();
            IndicesBuffer?.Release();
            VerticesArgumentBuffer?.Release();
        }
    }
}
