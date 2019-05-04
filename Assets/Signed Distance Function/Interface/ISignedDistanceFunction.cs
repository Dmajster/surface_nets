using UnityEngine;

namespace Assets.Signed_Distance_Function.Interface
{
    public interface ISignedDistanceFunction
    {
        Vector3 Minimum { get; set; }
        Vector3 Maximum { get; set; }

        float Value(Vector3 position);
    }
}
