using Assets.Signed_Distance_Function.Interface;
using UnityEngine;

namespace Assets.Signed_Distance_Function
{
    public class SdfSphere : ISignedDistanceFunction
    {
        public Vector3 Minimum { get; set; }
        public Vector3 Maximum { get; set; }

        public readonly Vector3 Position;
        public readonly float Radius;

        public SdfSphere(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;

            var offset = new Vector3(Radius, Radius, Radius);
            Minimum = Position - offset - Vector3.one;
            Maximum = Position + offset + Vector3.one;
        }

        public float Value(Vector3 position)
        {
            return Mathf.Pow(Position.x - position.x, 2) + Mathf.Pow(Position.y - position.y, 2) + Mathf.Pow(Position.z - position.z, 2) - Mathf.Pow(Radius, 2);
        }
    }
}
