using UnityEngine;

namespace Assets.Dual_Contouring
{
    public class Test : MonoBehaviour
    {

        public Vector3Int Size = new Vector3Int(64, 64, 64);

        public int RepeatCount = 100;

        public ComputeShader ComputeShader;
        public ComputeBuffer ResultBuffer;

        private void Start()
        {
            ResultBuffer = new ComputeBuffer(Size.x * Size.y * Size.z, sizeof(uint));
            ComputeShader.SetBuffer(0, "Data", ResultBuffer);
            ComputeShader.SetBuffer(1, "Data", ResultBuffer);
            ComputeShader.SetBuffer(2, "Data", ResultBuffer);
            ComputeShader.SetVector("Size", (Vector3)Size);

            var averageTime = 0f;

            ComputeShader.Dispatch(0, Size.x, Size.y, Size.z);
            for (var i = 0; i < RepeatCount; i++)
            {
                var startTime = Time.realtimeSinceStartup;
                ComputeShader.Dispatch(0, Size.x, Size.y, Size.z);
                var endTime = Time.realtimeSinceStartup;

                averageTime += endTime - startTime;
            }

            averageTime /= RepeatCount;

            Debug.Log($"1 by 1 by 1: {averageTime}");

            averageTime = 0f;

            ComputeShader.Dispatch(1, Size.x / 8, Size.y, Size.z / 8);
            for (var i = 0; i < RepeatCount; i++)
            {
                var startTime = Time.realtimeSinceStartup;
                ComputeShader.Dispatch(1, Size.x / 8, Size.y, Size.z / 8);
                var endTime = Time.realtimeSinceStartup;

                averageTime += endTime - startTime;
            }

            averageTime /= RepeatCount;

            Debug.Log($"8 by 1 by 8: {averageTime}");

            averageTime = 0f;

            ComputeShader.Dispatch(2, Size.x / 8, Size.y / 8, Size.z / 8);
            for (var i = 0; i < RepeatCount; i++)
            {
                var startTime = Time.realtimeSinceStartup;
                ComputeShader.Dispatch(2, Size.x / 8, Size.y / 8, Size.z / 8);
                var endTime = Time.realtimeSinceStartup;

                averageTime += endTime - startTime;
            }

            averageTime /= RepeatCount;

            Debug.Log($"8 by 8 by 8: {averageTime}");
        }
    }
}
