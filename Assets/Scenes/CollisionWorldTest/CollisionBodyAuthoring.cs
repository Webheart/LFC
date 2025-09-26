using Unity.Entities;
using UnityEngine;

namespace CollisionWorldTest
{
    public class CollisionBodyAuthoring : MonoBehaviour
    {
        public bool TypeTwo;

        class Baker : Baker<CollisionBodyAuthoring>
        {
            public override void Bake(CollisionBodyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (authoring.TypeTwo) AddComponent<BodyTwo>(entity);
                else AddComponent<BodyOne>(entity);
            }
        }
    }
}