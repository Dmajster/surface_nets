using Assets;
using Assets.Constructive_Solid_Geometry;
using Assets.Signed_Distance_Function;
using UnityEngine;

public class FuckMyShitUpFam : MonoBehaviour
{

    private ChunkManager _chunkManager;
    private TestChunking _testChunking;

    public SdfSphere Sphere;

    public void Start()
    {
        _chunkManager = FindObjectOfType<ChunkManager>();
        _testChunking = FindObjectOfType<TestChunking>();
    }

    public void Update()
    {
        Sphere = new SdfSphere(transform.position, 5f);

        _testChunking.Terrain = new Union(_testChunking.Terrain, Sphere);

        _chunkManager.UpdateChunks(_testChunking.Terrain);
    }
}
