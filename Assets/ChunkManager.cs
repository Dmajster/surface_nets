using Assets.Dual_Contouring;
using Assets.Dual_Contouring.Structs;
using Assets.Signed_Distance_Function.Interface;
using System;
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

        public Vector3 EffectMinimum;
        public Vector3 EffectMaximum;

        public void Awake()
        {
            ChunkMeshGenerator = GetComponent<ChunkMeshGenerator>();
            Chunks = new ChunkGameObject[Size.x * Size.y * Size.z];
        }

        public void OnDrawGizmos()
        {
            DrawChunkBoundingBoxes();
            DrawEffectArea();
        }

        private void DrawEffectArea()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(Vector3.Lerp(EffectMinimum, EffectMaximum, 0.5f), EffectMaximum - EffectMinimum);
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
            if (index >= Chunks.Length)
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
            newMeshRenderer.material = ChunkMeshGenerator.ChunkMaterial;
            var newChunkData = new Chunk(ChunkSize, worldPosition);

            //Debug.Log(index);
            Chunks[index].GameObject = newGameObject;
            Chunks[index].ChunkData = newChunkData;
        }

        public void UpdateChunk(ChunkGameObject chunk, ISignedDistanceFunction sdf)
        {
            for (var i = 0; i < chunk.ChunkData.Voxels.Length; i++)
            {
                var position = chunk.ChunkData.Position + chunk.ChunkData.GetPosition(i);

                chunk.ChunkData.Voxels[i].Density = sdf.Value(position);
            }
        }

        public void UpdateChunks(ISignedDistanceFunction sdf)
        {
            var minX = Mathf.FloorToInt(sdf.Minimum.x / ChunkSize.x);
            var maxX = Mathf.CeilToInt(sdf.Maximum.x / ChunkSize.x);

            var minY = Mathf.FloorToInt(sdf.Minimum.y / ChunkSize.y);
            var maxY = Mathf.CeilToInt(sdf.Maximum.y / ChunkSize.y);

            var minZ = Mathf.FloorToInt(sdf.Minimum.z / ChunkSize.z);
            var maxZ = Mathf.CeilToInt(sdf.Maximum.z / ChunkSize.z);

            EffectMaximum = new Vector3(maxX * ChunkSize.x, maxY * ChunkSize.y, maxZ * ChunkSize.z);
            EffectMinimum = new Vector3(minX * ChunkSize.x, minY * ChunkSize.y, minZ * ChunkSize.z);

            for (var x = minX; x < maxX; x++)
            {
                for (var y = minY; y < maxY; y++)
                {
                    for (var z = minZ; z < maxZ; z++)
                    {
                        var position = new Vector3(x, y, z);
                        var index = GetIndex(position);

                        if (index < 0 || index >= Chunks.Length)
                        {
                            continue;
                        }

                        //Debug.Log($"{index} > {Chunks.Length}");

                        

                        if (Chunks[index].GameObject == null)
                        {
                            CreateChunk(position);
                        }

                        var chunk = Chunks[index];

                        chunk.Status = ChunkStatus.Active;

                        UpdateChunk(chunk, sdf);
                        RenderChunk(chunk);
                    }
                }
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
                 index % Size.z,
                 index / (Size.x * Size.z),
                 index % (Size.x * Size.z) / Size.z
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

                        Gizmos.DrawWireCube(position, ChunkSize);
                    }
                }
            }
        }
    }
}
