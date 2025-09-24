using Latios;
using Scenes.UniqueMeshTests.UniqueMeshTests;

namespace DefaultNamespace
{
    public partial class SimulationSuperSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddManagedSystem<UniqueMeshesSystem>();
        }
    }
}