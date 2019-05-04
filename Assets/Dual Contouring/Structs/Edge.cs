using System;

namespace Assets.Dual_Contouring.Structs
{
    [Serializable]
    public struct Edge
    {
        public int Index1;
        public int Index2;

        public Edge(int index1, int index2)
        {
            Index1 = index1;
            Index2 = index2;
        }
    }
}