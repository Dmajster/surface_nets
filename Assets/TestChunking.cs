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

            //Sphere = new SdfSphere(new Vector3(16, 16, 16), 15f);

            //var index = _chunkManager.GetIndex(new Vector3(0, 0, 0));

            //_chunkManager.CreateChunk(index);
            //_chunkManager.UpdateChunk(_chunkManager.Chunks[index], Sphere);
            //_chunkManager.RenderChunk(_chunkManager.Chunks[index]);
            //_chunkManager.UpdateChunks(terrain);
        }

        public bool Forward = true;
        public float X = 16;
        public float Speed = 10;

        public void Update()
        {

            if (Forward)
            {
                if (X > 128)
                {
                    Forward = false;
                }


            }
            else
            {
                if (X < 0)
                {
                    Forward = true;
                }
            }

            X += (Forward ? Speed : -Speed) * Time.deltaTime;

            Sphere = new SdfSphere(new Vector3(X, 16, 8), 8);

            _chunkManager.UpdateChunks(Sphere);

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
