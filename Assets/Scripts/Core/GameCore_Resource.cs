using Resource.ResourceModules;

namespace Core
{
    public partial class GameCore
    {
        private void RegisterAllResourceModules()
        {
            Resource.RegisterResourceModule(new MainResourceModule());
        }
    }
}