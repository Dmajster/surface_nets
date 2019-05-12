using System;
using UnityEngine;

namespace Assets.Dual_Contouring.Structs
{
    [Serializable]
    public struct Chunk
    {
        public Voxel[] Voxels;
        
        public Vector3 Position;
        public Vector3 Size;

        public Chunk(Vector3 size, Vector3 position)
        {
            Position = position;
            Size = size;
            Voxels = new Voxel[(int)size.x * (int)size.y * (int)size.z];
            for (var i = 0; i < Voxels.Length; i++)
            {
                Voxels[i].Density = float.PositiveInfinity;
            }
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
    }
}


