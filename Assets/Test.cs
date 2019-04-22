using UnityEngine;

namespace Assets
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class Test : MonoBehaviour
    {
        public Chunk Chunk;
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;

        private void Start()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();

            Chunk = new Chunk(Vector3.zero, new Vector3(64,64,64));
            Chunk.PopulateSphere();

            var start = Time.realtimeSinceStartup;
            MeshFilter.mesh = Chunk.ConstructMesh();
            var executionTime = Time.realtimeSinceStartup - start;
            Debug.Log($"{executionTime}s");
        }
        

        private void OnDrawGizmos()
        {
            /*
            for (var i = 0; i < Chunk.Voxels.Length; i++)
            {
                Gizmos.DrawWireCube(Chunk.GetPosition(i)+ new Vector3(0.5f,0.5f,0.5f), Vector3.one);
            }
            
            if (Chunk.FeaturePoints != null)
            {
                foreach (var featurePoint in Chunk.FeaturePoints.Values)
                {
                    Gizmos.DrawSphere(featurePoint, 0.1f);
                }
            }
*/
        }
    }
}
