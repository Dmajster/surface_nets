using System.Drawing;
using System.Runtime.InteropServices;
using Assets.Dual_Contouring.Structs;
using UnityEngine;

namespace Assets.Dual_Contouring
{
    public class ChunkMeshGenerator : MonoBehaviour
    {
        public Material ChunkMaterial;

        public ComputeShader ComputeShader;
        private ComputeBuffer _voxelBuffer;
        private ComputeBuffer _featurePointBuffer;
        private ComputeBuffer _indicesBuffer;
        private ComputeBuffer _quadsBuffer;
        private ComputeBuffer _verticesArgumentBuffer;

        public Mesh CreateChunkMesh(Chunk chunk)
        {
            var mesh = new Mesh();

            _voxelBuffer = new ComputeBuffer(chunk.Voxels.Length, Marshal.SizeOf<Voxel>());
            _featurePointBuffer = new ComputeBuffer(chunk.Voxels.Length, Marshal.SizeOf<Vector3>());
            _quadsBuffer = new ComputeBuffer(chunk.Voxels.Length * 4, Marshal.SizeOf<Vector3>(), ComputeBufferType.Append);
            _verticesArgumentBuffer = new ComputeBuffer(4,sizeof(int), ComputeBufferType.IndirectArguments);

            //PART 1 - FIGURE OUT MESH VERTICES
            _voxelBuffer.SetData(chunk.Voxels); //Put the chunk data in the gpu buffer
            ComputeShader.SetVector("Size", chunk.Size); //Set the chunk dimensions on the gpu
            ComputeShader.SetBuffer(0, "Voxels", _voxelBuffer); //Bind the gpu buffer t
            ComputeShader.SetBuffer(0, "FeaturePoints", _featurePointBuffer); //Bind the output buffer to the 0th kernel
            ComputeShader.Dispatch(0, (int)chunk.Size.x / 8, (int)chunk.Size.y / 8, (int)chunk.Size.z / 8); //Dispatch the task to cores, one for each voxel of the mesh? 

            //FeaturePointBuffer.GetData(FeaturePoints); //TODO: COMMENT OUT, USED ONLY FOR OUTPUT VISUALIZATION 

            //PART 2 - CONSTRUCT THE MESH QUADS 
            ComputeShader.SetBuffer(1, "Voxels", _voxelBuffer);
            ComputeShader.SetBuffer(1, "FeaturePoints", _featurePointBuffer); //Bind the output buffer of kernel 0 to kernel 1
            ComputeShader.SetBuffer(1, "Quads", _quadsBuffer); //Output buffer containing actual quads of the mesh
            ComputeShader.Dispatch(1, (int)chunk.Size.x / 8, (int)chunk.Size.y / 8, (int)chunk.Size.z / 8); //Dispatch the task to cores, one for each voxel of the mesh? 

            ComputeBuffer.CopyCount(_quadsBuffer, _verticesArgumentBuffer, 0); //copy the indirect arguments to indirect buffer

            var verticeArguments = new int[4]; //make a array to store indirect arguments
            _verticesArgumentBuffer.GetData(verticeArguments); //copy data from indirect buffer to array

            //Debug.Log(verticeArguments[0]);

            var quadCount = verticeArguments[0]; //verticeArguments[0] contains count of quads

            if (quadCount == 0)
            {
                ReleaseBuffers();
                return mesh;
            }

            var verticeCount = quadCount * 4; //each quad contains 4 vertices
            var vertices = new Vector3[verticeCount]; //create new vertice array for the new mesh
            _quadsBuffer.GetData(vertices); //store vertices

            //PART 3 - CONSTRUCT THE MESH INDICES
            var indicesCount = quadCount * 6;
            _indicesBuffer = new ComputeBuffer(indicesCount, sizeof(int));
            ComputeShader.SetBuffer(2, "Quads", _quadsBuffer);
            ComputeShader.SetBuffer(2, "Indices", _indicesBuffer);
            ComputeShader.Dispatch(2, quadCount, 1, 1);

            var indices = new int[indicesCount];
            _indicesBuffer.GetData(indices);

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
            _voxelBuffer?.Release();
            _featurePointBuffer?.Release();
            _quadsBuffer?.Release();
            _indicesBuffer?.Release();
            _verticesArgumentBuffer?.Release();
        }
    }
}
