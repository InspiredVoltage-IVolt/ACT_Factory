namespace ACT.Core.Factory
{
    public class PluginInfo
    {
        public string PluginHash = "";
        public string FriendlyName = "";
        public string Description = "";
        public string FileName = "";
        public int FileSize = 0;
        public string FileVersion = "";
        public string PackageName = "";
        public string GitLocation = "";
        public string Homepage = "";
        public string Author = "";

        // CACHE INFO

        // All Class Names
        public List<(string ClassName, Guid GeneratedCacheID)> ClassNames = new List<(string ClassName, Guid GeneratedCacheID)>();

        // All Methods Key = Class ID
        public SortedDictionary<Guid, List<MethodInfo>> ClassMethods = new SortedDictionary<Guid, List<MethodInfo>>();

        // All Interfaces Key = Method ID
        public SortedDictionary<Guid, List<InterfaceInfo>> MethodInterfaces = new SortedDictionary<Guid, List<InterfaceInfo>>();

        public List<byte> File = new List<byte>();

        public struct MethodInfo
        {
            public Type MethodType;
            public string Name;
            public Guid ID;
        }

        public struct InterfaceInfo
        {
            public Guid ID;
            public string Name;
            public Type InterfaceType;
        }
    }
}
