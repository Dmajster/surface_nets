using Assets.Constructive_Solid_Geometry;
using Assets.Signed_Distance_Function;
using UnityEngine;

namespace Assets
{
    public class Test : MonoBehaviour
    {
        public void Start()
        {
            var chunkManager = FindObjectOfType<ChunkManager>();

            //var terrain = new SDFTerrain();
            var sphere = new SdfSphere(new Vector3(32, 32, 32), 30);
            var sphere2 = new SdfSphere(new Vector3(56, 45, 32), 25);
            var union = new Difference(sphere, sphere2);

            for (var i = 0; i < chunkManager.Chunks.Length; i++)
            {
                chunkManager.CreateChunk(i);
                var chunk = chunkManager.Chunks[i];
                chunkManager.PopulateChunk(chunk, union);
                chunkManager.RenderChunk(chunk);
            }
        }
    }
}
