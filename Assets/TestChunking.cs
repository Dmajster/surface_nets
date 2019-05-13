using Assets.Signed_Distance_Function;
using UnityEngine;

namespace Assets
{
    public class TestChunking : MonoBehaviour
    {
        public SdfSphere Sphere;
        public SdfSphere Sphere2;

        private ChunkManager _chunkManager;

        public void Start()
        {
            _chunkManager = FindObjectOfType<ChunkManager>();

            var terrain = new SdfTerrain();
            _chunkManager.UpdateChunks(terrain);
        }

        public void FixedUpdate()
        {
        }


        public void OnDrawGizmos()
        {
            if (Sphere == null)
            {
                return;
            }

            var center = Vector3.Lerp(Sphere.Minimum, Sphere.Maximum, 0.5f);
            var size = Sphere.Maximum - Sphere.Minimum;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
