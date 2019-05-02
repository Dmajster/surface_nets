using UnityEngine;

namespace Assets.Dual_Contouring
{
    [RequireComponent(
        typeof(MeshFilter),
        typeof(MeshRenderer))]
    public class Test : MonoBehaviour
    {
        public Chunk Chunk;
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;

        private void Start()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();

            Chunk = new Chunk(Vector3.zero, new Vector3(8,8,8));
            Chunk.PopulateSphere();

            var start = Time.realtimeSinceStartup;

            MeshFilter.mesh = Chunk.ConstructMesh();

            var executionTime = Time.realtimeSinceStartup - start;

            Debug.Log($"{executionTime}s");
        }
    }
}
