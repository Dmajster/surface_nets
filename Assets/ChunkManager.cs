using Assets.Dual_Contouring;
using Assets.Dual_Contouring.Structs;
using System;
using Assets.Signed_Distance_Function.Interface;
using UnityEngine;

namespace Assets
{
    public enum ChunkStatus
    {
        Idle,
        Active,
        Updating
    }

    [Serializable]
    public struct ChunkGameObject
    {
        public Chunk ChunkData;
        public GameObject GameObject;
        public ChunkStatus Status;
    }

    [RequireComponent(typeof(ChunkMeshGenerator))]
    public class ChunkManager : MonoBehaviour
    {
        public Vector3Int Size;
        public Vector3Int ChunkSize;
        public ChunkGameObject[] Chunks;
        public ChunkMeshGenerator ChunkMeshGenerator;

        public bool DrawChunks;

        public void Awake()
        {
            ChunkMeshGenerator = GetComponent<ChunkMeshGenerator>();
            Chunks = new ChunkGameObject[Size.x * Size.y * Size.z];
        }

        public void OnDrawGizmos()
        {
            DrawChunkBoundingBoxes();
        }

        public int GetIndex(Vector3 position)
        {
            return (int)(Size.z * position.z + position.y * (Size.x * Size.z) + position.x);
        }

        public void CreateChunk(Vector3 position)
        {
            var index = GetIndex(position);

            CreateChunk(index);
        }

        public void CreateChunk(int index)
        {
            if ( index >= Chunks.Length)
            {
                throw new IndexOutOfRangeException();
            }

            var position = GetPosition(index);
            var newGameObject = new GameObject($"Chunk ({position.x},{position.y},{position.z})");
            newGameObject.transform.parent = transform;

            var worldPosition = new Vector3(
                position.x * ChunkSize.x,
                position.y * ChunkSize.y,
                position.z * ChunkSize.z
            );

            newGameObject.transform.position = worldPosition; 

            var newMeshFilter = newGameObject.AddComponent<MeshFilter>();
            var newMeshRenderer = newGameObject.AddComponent<MeshRenderer>();
            var newChunkData = new Chunk(ChunkSize, worldPosition);

            Debug.Log(index);
            Chunks[index].GameObject = newGameObject;
            Chunks[index].ChunkData = newChunkData;
        }

        public void PopulateChunk(ChunkGameObject chunk, ISignedDistanceFunction sdf)
        {
            for (var i = 0; i < chunk.ChunkData.Voxels.Length; i++)
            {
                var position = chunk.ChunkData.Position + chunk.ChunkData.GetPosition(i);

                chunk.ChunkData.Voxels[i].Density = sdf.Value(position);
            }
        }

        public void RenderChunk(ChunkGameObject chunk)
        {
            var meshFilter = chunk.GameObject.GetComponent<MeshFilter>();
            meshFilter.mesh = ChunkMeshGenerator.CreateChunkMesh(chunk.ChunkData);
        }

        public Vector3 GetPosition(int index)
        {
            return new Vector3(
                (int) (index % Size.z),
                (int) (index / (Size.x * Size.z)),
                (int) (index % (Size.x * Size.z) / Size.z)
            );
        }

        public void DrawChunkBoundingBoxes()
        {
            if (!DrawChunks || Chunks.Length == 0)
            {
                return;
            }

            for (var x = 0; x < Size.x; x++)
            {
                for (var y = 0; y < Size.y; y++)
                {
                    for (var z = 0; z < Size.z; z++)
                    {
                        var position = new Vector3(
                            x * ChunkSize.x + ChunkSize.x / 2,
                            y * ChunkSize.y + ChunkSize.y / 2,
                            z * ChunkSize.z + ChunkSize.z / 2
                        );

                        var status = Chunks[GetIndex(new Vector3(x, y, z))].Status;

                        switch (status)
                        {
                            case ChunkStatus.Idle:
                                Gizmos.color = Color.gray;
                                break;
                            case ChunkStatus.Active:
                                Gizmos.color = Color.green;
                                break;
                            case ChunkStatus.Updating:
                                Gizmos.color = Color.red;
                                break;
                            default:
                                Debug.Log("This is not supposed to be happening...");
                                break;
                        }

                        Gizmos.DrawWireCube(position, ChunkSize );
                    }
                }
            }
        }
    }
}
