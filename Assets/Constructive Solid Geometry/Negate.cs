using Assets.Signed_Distance_Function.Interface;
using UnityEngine;

namespace Assets.Constructive_Solid_Geometry
{
    public class Negate : ISignedDistanceFunction
    {
        public Vector3 Minimum { get; set; }
        public Vector3 Maximum { get; set; }

        public readonly ISignedDistanceFunction Sdf;

        public Negate(ISignedDistanceFunction sdf)
        {
            Minimum = sdf.Minimum;
            Maximum = sdf.Maximum;

            Sdf = sdf;
        }

        public float Value(Vector3 position)
        {
            return Sdf.Value(position) * -1;
        }
    }
}
