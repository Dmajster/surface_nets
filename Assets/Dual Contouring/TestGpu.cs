using UnityEngine;

namespace Assets.Dual_Contouring
{
    [RequireComponent(
        typeof(MeshFilter),
        typeof(MeshRenderer))]
    public class TestGpu : MonoBehaviour
    {
        public ChunkGpu Chunk;
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;
        public ComputeShader ComputeShader;

        private void Start()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();

            Chunk = new ChunkGpu(Vector3.zero, new Vector3(32,32,32));
            Chunk.ComputeShader = ComputeShader;
            Chunk.PopulateSphere();

            var start = Time.realtimeSinceStartup;

            MeshFilter.mesh = Chunk.ConstructMesh();

            var executionTime = Time.realtimeSinceStartup - start;

            Debug.Log($"GPU time: {executionTime}s");
        }
    }
}
