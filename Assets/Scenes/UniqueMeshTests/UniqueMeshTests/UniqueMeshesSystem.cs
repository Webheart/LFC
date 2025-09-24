using Latios.Kinemation;
using Latios.Transforms;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

namespace Scenes.UniqueMeshTests.UniqueMeshTests
{
    public partial class UniqueMeshesSystem : SystemBase
    {
        private bool _createdMeshes = false;
        private bool _switchedMeshes = false;
        
        private Entity _entity1;
        private Entity _entity2;
        
        private Mesh _placeholderMesh;
        protected override void OnCreate()
        {
            RequireForUpdate<MeshMaterialComponentData>();
        }
        

        private void CreateUniqueMeshEntity(Entity entity)
        {
            var materialComponent = SystemAPI.GetSingleton<MeshMaterialComponentData>();
            
            var entitiesGraphicsSystem = World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            BatchMaterialID terrainMaterialBatchId = entitiesGraphicsSystem.RegisterMaterial(materialComponent.Material);
            BatchMeshID batchMeshID = entitiesGraphicsSystem.RegisterMesh(_placeholderMesh);
            
            // Create a RenderMeshDescription using the convenience constructor
            // with named parameters.
            RenderMeshDescription desc = new RenderMeshDescription(
                shadowCastingMode: ShadowCastingMode.On,
                receiveShadows: true);
            
            RenderMeshUtility.AddComponents(
                entity,
                EntityManager,
                desc,
                new MaterialMeshInfo
                {
                    Material = (int) terrainMaterialBatchId.value,
                    Mesh = (int) batchMeshID.value,
                });

            EntityManager.AddComponent<UniqueMeshConfig>(entity);

            NativeList<UniqueMeshPosition> vertices =  new NativeList<UniqueMeshPosition>(1500 * 1500 * 3, WorldUpdateAllocator);
            NativeList<UniqueMeshIndex> indices = new NativeList<UniqueMeshIndex>(1500 * 1500 * 3, WorldUpdateAllocator);
            
            // Add triangles
            for (int i = 0; i < 1500; i++)
            {
                for (int j = 0; j < 1500; j++)
                {
                    vertices.Add(new UniqueMeshPosition{ position = new float3(i, 0, j) });
                    indices.Add(new UniqueMeshIndex { index = vertices.Length - 1 });
                    vertices.Add(new UniqueMeshPosition{ position = new float3(i, 0, j + 0.9f) });
                    indices.Add(new UniqueMeshIndex { index = vertices.Length - 1 });
                    vertices.Add(new UniqueMeshPosition{ position = new float3(i + 0.9f, 0, j) });
                    indices.Add(new UniqueMeshIndex { index = vertices.Length - 1 });
                }
            }
            
            var verticesBuffer = EntityManager.AddBuffer<UniqueMeshPosition>(entity);
            verticesBuffer.AddRange(vertices.AsArray());
            var indicesBuffer = EntityManager.AddBuffer<UniqueMeshIndex>(entity);
            indicesBuffer.AddRange(indices.AsArray());
            
            var worldTransform = new WorldTransform
            {
                worldTransform = new TransformQvvs(0, quaternion.identity)
            };
            
            EntityManager.SetComponentData(entity, new RenderBounds()
            {
                Value = new AABB
                {
                    Center = 750,
                    Extents = 1500,
                }
            });
            
            EntityManager.AddComponentData(entity, worldTransform);
        }
        
        private void CreateMeshes()
        {
            _createdMeshes = true;
            _placeholderMesh = new Mesh();
            
            _entity1 = EntityManager.CreateEntity();
            _entity2 = EntityManager.CreateEntity();

            CreateUniqueMeshEntity(_entity1);
            CreateUniqueMeshEntity(_entity2);
            
            EntityManager.SetComponentEnabled<MaterialMeshInfo>(_entity2, false);
        }

        protected override void OnUpdate()
        {
            if (!_createdMeshes) CreateMeshes();


            if (!_switchedMeshes && SystemAPI.Time.ElapsedTime > 7)
            {
                Debug.Log("Switching meshes");
                _switchedMeshes = true;
                EntityManager.SetComponentEnabled<MaterialMeshInfo>(_entity1, false);
                EntityManager.SetComponentEnabled<MaterialMeshInfo>(_entity2, true);
            }
        }
    }
}
