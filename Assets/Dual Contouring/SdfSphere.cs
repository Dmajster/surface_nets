using System.Runtime.InteropServices;
using Assets.Dual_Contouring.Structs;
using UnityEngine;

namespace Assets.Dual_Contouring
{
    public class SdfSphere : MonoBehaviour
    {
        public ComputeShader SphereComputeShader;

        public void Add(Chunk chunk, Vector3 position, float radius )
        {
            //Sphere variables
            SphereComputeShader.SetBuffer(0,"Voxels", chunk.ComputeBuffer);
            SphereComputeShader.SetVector("Position", position);
            SphereComputeShader.SetFloat("Radius", radius);
            SphereComputeShader.SetBool("Populated", chunk.Populated);

            //Chunk variables
            SphereComputeShader.SetVector("Size", chunk.Size);

            SphereComputeShader.Dispatch(0, (int)chunk.Size.x / 8, (int)chunk.Size.y / 8, (int)chunk.Size.z / 8);
            chunk.Populated = true;
        }

        public void Remove(Chunk chunk, Vector3 position, float radius)
        {
            //Sphere variables
            SphereComputeShader.SetBuffer(1, "Voxels", chunk.ComputeBuffer);
            SphereComputeShader.SetVector("Position", position);
            SphereComputeShader.SetFloat("Radius", radius);
            SphereComputeShader.SetBool("Populated", chunk.Populated);

            //Chunk variables
            SphereComputeShader.SetVector("Size", chunk.Size);

            SphereComputeShader.Dispatch(1, (int)chunk.Size.x / 8, (int)chunk.Size.y / 8, (int)chunk.Size.z / 8);
            chunk.Populated = true;
        }
    }
}
