using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Dual_Contouring.Structs
{
    [Serializable]
    public struct Chunk
    {
        public ComputeBuffer ComputeBuffer;
        public Vector3 Position;
        public Vector3 Size;
        public bool Populated;

        public int GetSize => (int) (Size.x * Size.y * Size.z);

        public Chunk(Vector3 size, Vector3 position)
        {
            Position = position;
            Size = size;
            ComputeBuffer = new ComputeBuffer((int)(Size.x * Size.y * Size.z), Marshal.SizeOf<Voxel>() );
            Populated = false;
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


