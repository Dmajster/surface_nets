using Assets.Dual_Contouring;
using Assets.Dual_Contouring.Structs;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets
{
    [RequireComponent(typeof(ChunkMeshGenerator))]
    [RequireComponent(typeof(SdfSphere))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TestChunking : MonoBehaviour
    {
        public ChunkMeshGenerator ChunkMeshGenerator;
        public SdfSphere Sphere;

        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;

        public Chunk Chunk;

        public void Start()
        {
            ChunkMeshGenerator = GetComponent<ChunkMeshGenerator>();
            Sphere = GetComponent<SdfSphere>();

            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();

            Chunk = new Chunk(new Vector3(32,32,32), new Vector3(0,0,0)  );

        }

        public void FixedUpdate()
        {
            var position = new Vector3(
                Random.Range(1, 32),
                Random.Range(1, 32),
                Random.Range(1, 32)
            );

            var radius = Random.Range(1, 16);

            var add = Random.Range(0, 2);

            if (add == 1)
            {
                Debug.Log("add");
                Sphere.Add(Chunk, position, radius);
            }
            else
            {
                Debug.Log("remove");
                Sphere.Remove(Chunk, position, radius);
            }
            
            MeshFilter.mesh = ChunkMeshGenerator.CreateChunkMesh(Chunk);
            //Chunk.ComputeBuffer = new ComputeBuffer(Chunk.GetSize, Marshal.SizeOf<Voxel>());

            if (!Chunk.Populated)
            {
                Chunk.Populated = true;
            }
        }

        public void OnDestroy()
        {
            Chunk.ComputeBuffer.Dispose();
        }
    }
}
