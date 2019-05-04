using Assets.Signed_Distance_Function.Interface;
using UnityEngine;

namespace Assets.Constructive_Solid_Geometry
{
    public class Intersect : ISignedDistanceFunction
    {
        public Vector3 Minimum { get; set; }
        public Vector3 Maximum { get; set; }

        public readonly ISignedDistanceFunction Sdf1;
        public readonly ISignedDistanceFunction Sdf2;

        public Intersect(ISignedDistanceFunction sdf1, ISignedDistanceFunction sdf2)
        {
            Minimum = Vector3.Min(sdf1.Minimum, sdf2.Minimum);
            Maximum = Vector3.Max(sdf1.Maximum, sdf2.Maximum);

            Sdf1 = sdf1;
            Sdf2 = sdf2;
        }

        public float Value(Vector3 position)
        {
            return Mathf.Max(Sdf1.Value(position), Sdf2.Value(position));
        }
    }
}
