using Unity.Entities;
using UnityEngine;

namespace Scenes.UniqueMeshTests.UniqueMeshTests
{
    public class MeshMaterialAuthoring : MonoBehaviour
    {
        public Material material;

        public class MeshMaterialAuthoringBaker : Baker<MeshMaterialAuthoring>
        {
            public override void Bake(MeshMaterialAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MeshMaterialComponentData { Material = authoring.material });
            }
        }
    }

    public struct MeshMaterialComponentData : IComponentData
    {
        public UnityObjectRef<Material> Material;
    }
}