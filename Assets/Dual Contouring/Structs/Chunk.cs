using System;
using UnityEngine;

namespace Assets.Dual_Contouring.Structs
{
    [Serializable]
    public class Chunk
    {
        public readonly Voxel[] Voxels;
        public readonly Vector3 Size;

        public Chunk(Vector3 size)
        {
            Size = size;
            Voxels = new Voxel[(int)size.x * (int)size.y * (int)size.z];
        }
    }
}


