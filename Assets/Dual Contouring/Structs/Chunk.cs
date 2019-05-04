using System;
using UnityEngine;

namespace Assets.Dual_Contouring.Structs
{
    [Serializable]
    public class Chunk
    {
        public readonly Voxel[] Voxels;
        public readonly Vector3 Position;
        public readonly Vector3 Size;

        public Chunk(Vector3 size, Vector3 position)
        {
            Position = position;
            Size = size;
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
    }
}


