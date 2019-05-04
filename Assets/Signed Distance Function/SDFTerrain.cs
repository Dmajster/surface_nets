using Assets.Signed_Distance_Function.Interface;
using UnityEngine;

namespace Assets.Signed_Distance_Function
{
    public class SdfTerrain : ISignedDistanceFunction
    {
        public Vector3 Minimum { get; set; }
        public Vector3 Maximum { get; set; }
        

        public SdfTerrain()
        {
        }

        public float Value(Vector3 position)
        {
            float height = Mathf.PerlinNoise(position.x / 512, position.z / 512) * 128;
            height += Mathf.PerlinNoise(position.x / 256, position.z / 256) * 64;
            height += Mathf.PerlinNoise(position.x / 128, position.z / 128) * 32;
            height += Mathf.PerlinNoise(position.x / 64, position.z / 64) * 16;
            return position.y - height;
        }
    }
}
