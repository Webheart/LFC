using Unity.Entities;
using UnityEngine;

namespace CollisionWorldTest
{
    public class CollisionBodyAuthoring : MonoBehaviour
    {
        public bool TypeTwo;
        public bool Raycaster;

        class Baker : Baker<CollisionBodyAuthoring>
        {
            public override void Bake(CollisionBodyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                if (authoring.TypeTwo) AddComponent<BodyTwo>(entity);
                else AddComponent<BodyOne>(entity);
                
                if(authoring.Raycaster) AddComponent<Raycaster>(entity);
            }
        }
    }
}