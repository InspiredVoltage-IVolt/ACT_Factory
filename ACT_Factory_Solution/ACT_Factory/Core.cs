namespace ACT.Core.Factory
{
    public interface IFactoryCore
    {
        string Name { get; set; }
        string Description { get; set; }
        Guid ID { get; set; }
        string BasePath { get; set; }
        List<string> WorkingPaths { get; set; }
        List<Type> ManagedInterfaces { get; set; }
        List<(Type InterfaceType, List<string> FilePath)> LocatedPlugins { get; set; }
        List<(Type InterfaceType, List<string> FilePath)> LocatedCode { get; set; }
    }

    public static class Core
    {

        static Core()
        {
            // Constructor
        }

        public static void LoadFactoryCore(Guid ID) { }
        public static void DisposeFactoryCore(Guid? ID, string Name) { }


    }

    internal class _Core
    {
        _Core()
        {

        }

        ~_Core()
        {
            // Clean up Everything
        }
    }
}
