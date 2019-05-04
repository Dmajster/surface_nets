using Assets.Signed_Distance_Function;
using UnityEngine;

namespace Assets
{
    public class Test : MonoBehaviour
    {
        public void Start()
        {
            var chunkManager = FindObjectOfType<ChunkManager>();

            var sphere = new SdfSphere(new Vector3(24,16,16), 16f);

            var chunkIndex = new Vector3(0, 0, 0);
            chunkManager.CreateChunk(chunkIndex);
            var chunk = chunkManager.Chunks[chunkManager.GetIndex(chunkIndex)];
            chunkManager.PopulateChunk(chunk, sphere);
            chunkManager.RenderChunk(chunk);

            chunkIndex = new Vector3(1, 0, 0);
            chunkManager.CreateChunk(chunkIndex);
            chunk = chunkManager.Chunks[chunkManager.GetIndex(chunkIndex)];
            chunkManager.PopulateChunk(chunk, sphere);
            chunkManager.RenderChunk(chunk);

            chunkIndex = new Vector3(0, 1, 0);
            chunkManager.CreateChunk(chunkIndex);
            chunk = chunkManager.Chunks[chunkManager.GetIndex(chunkIndex)];
            chunkManager.PopulateChunk(chunk, sphere);
            chunkManager.RenderChunk(chunk);
        }
    }
}
