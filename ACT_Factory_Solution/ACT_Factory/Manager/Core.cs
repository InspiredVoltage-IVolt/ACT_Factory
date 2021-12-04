using ACT.Core.Extensions;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace ACT.Core.Factory.Manager
{
    public static class Core
    {
        public static bool Ready = false;

        public static List<Exception> AllExceptionsFound = new List<Exception>();

        internal static SortedDictionary<Type, List<PluginInfo>> PluginCache = new SortedDictionary<Type, List<PluginInfo>>();

        static Core()
        {
            PluginCache.Clear();
            // Try and Load VIA Saved File
        }

        public static byte[] PackageAllCacheData()
        {
            return null;
        }

        /// <summary>
        /// Gathers all the classes and plugins that implement Any Type and Caches The Data To Memory and Disk
        /// </summary>
        /// <param name="DirectoriesToSearch"></param>
        /// <param name="CacheAllTypes"></param>
        /// <returns>Total DLL Count , Total Types Found</returns>
        public static (int? DLLCount, int? TypeMatches) Init(List<string> DirectoriesToSearch, bool IncludeBasePath = true)
        {
            return Init(null, IncludeBasePath, DirectoriesToSearch);
        }

        /// <summary>
        /// Gathers all the classes and plugins that implement the Type list that is passed
        /// </summary>
        /// <param name="DirectoriesToSearch"></param>
        /// <returns>Total DLL Count , Total Types Found</returns>
        public static (int? DLLCount, int? TypeMatches) Init(List<Type> InterfacesToSearchFor, bool IncludeBasePath = true, List<string> DirectoriesToSearch = null)
        {
            List<string> AllPathsToSearch = new List<string>();
            bool _AllInterfaces = true; bool _SpecificDirectories = false;
            List<string> BaseDirectories = new List<string>();
            List<string> AllDllsFound = new List<string>();
            (int DLLCount, int TypeMatches) _tmpReturn;
            List<string> _AllInterfaceNamesFound = new List<string>();

            if (InterfacesToSearchFor != null) { _AllInterfaces = false; }
            if (DirectoriesToSearch != null) { _SpecificDirectories = true; }
            if (DirectoriesToSearch != null)
            {
                if (DirectoriesToSearch.Exists(x => System.IO.Directory.Exists(x) == true))
                {
                    if (_SpecificDirectories) { BaseDirectories.AddRange(DirectoriesToSearch.Where(x => System.IO.Directory.Exists(x) == true)); }
                }
            }

            if (IncludeBasePath) { BaseDirectories.Add(AppDomain.CurrentDomain.BaseDirectory); }
            if (BaseDirectories.Count() == 0) { return (0, 0); }

            ConcurrentBag<PluginInfo> _PluginCache = new ConcurrentBag<PluginInfo>();

            foreach (string path in BaseDirectories)
            {
                AllPathsToSearch.AddRange(path.GetAllSubDirectories());
            }

            AllDllsFound = GetAllDLLs(BaseDirectories);
            /// TODO CHECK IF NEEDED
            AllDllsFound.Sort();
            _tmpReturn.DLLCount = AllDllsFound.Count();

            Parallel.ForEach(AllDllsFound, dll =>
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(dll);
                    PluginInfo PluginInfo = new PluginInfo();

                    Guid _DLLID = Guid.NewGuid();
                    PluginInfo.ClassMethods.Add(_DLLID, new List<PluginInfo.MethodInfo>());

                    foreach (Type exportedType in assembly.GetExportedTypes())
                    {
                        if (exportedType.IsClass && exportedType.FullName != null)
                        {
                            var _tmpMethod = new PluginInfo.MethodInfo() { ID = Guid.NewGuid(), MethodType = exportedType, Name = exportedType.FullName };
                            PluginInfo.ClassMethods[_DLLID].Add(_tmpMethod);

                            if (exportedType.GetInterfaces().Length > 0)
                            {
                                PluginInfo.MethodInterfaces.Add(_tmpMethod.ID, new List<PluginInfo.InterfaceInfo>());
                                foreach (var interfacesFound in exportedType.GetInterfaces())
                                {
                                    if (_AllInterfaces)
                                    {
                                        if (interfacesFound.FullName != null)
                                        {
                                            _AllInterfaceNamesFound.Add(interfacesFound.FullName);
                                            var _tmpInterface = new PluginInfo.InterfaceInfo() { ID = _tmpMethod.ID, Name = interfacesFound.FullName, InterfaceType = interfacesFound };
                                        }
                                    }
                                    else if (InterfacesToSearchFor != null && InterfacesToSearchFor.Contains(interfacesFound))
                                    {
                                        if (interfacesFound.FullName != null)
                                        {
                                            _AllInterfaceNamesFound.Add(interfacesFound.FullName);
                                            var _tmpInterface = new PluginInfo.InterfaceInfo() { ID = _tmpMethod.ID, Name = interfacesFound.FullName, InterfaceType = interfacesFound };
                                        }
                                    }
                                }
                            }
                        }
                    }

                    _PluginCache.Add(PluginInfo);
                }
                catch (Exception ex)
                {
                    AllExceptionsFound.Add(ex);
                    //TODO LOG ERROR  throw new TypeLoadException("Error Locating " + typeof(T).FullName, ex);
                }
            });

            if (_AllInterfaces == false && InterfacesToSearchFor != null)
            {
                _tmpReturn.TypeMatches = InterfacesToSearchFor.Count(x => x.FullName != null && _AllInterfaceNamesFound.Contains(x.FullName));
            }
            else { _tmpReturn.TypeMatches = _AllInterfaceNamesFound.Count(); }

            return _tmpReturn;
        }

        /// <summary>
        /// Gets All Dlls Fast
        /// </summary>
        /// <param name="DirectortiesToSearch"></param>
        /// <returns>List Of All the Dlls Found</returns>
        public static List<string> GetAllDLLs(List<String> DirectortiesToSearch)
        {
            if (DirectortiesToSearch == null || DirectortiesToSearch.Count == 0) { return new List<string>(); }

            ConcurrentBag<string> _DLLs = new ConcurrentBag<string>();
            Parallel.ForEach(DirectortiesToSearch, baseDirectory =>
            {
                baseDirectory.GetAllFilesFromPath(@"([a-zA-Z0-9\s_\\.\-\(\):])+(.dll)$", true).ForEach(fudll => _DLLs.Add(fudll));
            });
            return _DLLs.ToList();
        }

        /// <summary>
        /// Load The DLL and return an instance of the specified class
        /// </summary>
        /// <typeparam name="T">Type to Return</typeparam>
        /// <param name="FileLocation">Location of the dll file</param>
        /// <param name="ClassName">Class Name To Return</param>
        /// <param name="Arguments">Any Arguments needed for the init of the class, default is null</param>
        /// <param name="CacheAllTypes">Cache All Types For Future Lookup</param>
        /// <returns>instance of class</returns>
        public static T LoadDLL<T>(string FileLocation, string ClassName, List<object> Arguments = null, bool CacheAllTypes = false)
        {
            Assembly _Assembly = null;
            List<object> _Arguments = null;

            if (!FileLocation.ToLower().EndsWith(".dll"))
            {
                return default(T);
            }

            if (!File.Exists(FileLocation)) { return default(T); }

            // Get Directory From File Location
            string _FilePath = FileLocation.Substring(0, FileLocation.LastIndexOf("\\")) + "\\";

            //  Load File - 
            try { _Assembly = Assembly.LoadFile(FileLocation); }
            catch (Exception ex) { throw new Exception("Error Loading Assemnly: " + FileLocation, ex); }

            string typeName = "";
            try
            {
                bool flag = false;
                foreach (Type exportedType in _Assembly.GetExportedTypes())
                {
                    foreach (Type type in exportedType.GetInterfaces())
                    {
                        if (type.FullName == typeof(T).FullName)
                        {
                            typeName = exportedType.FullName;
                            if (ClassName == exportedType.Name)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (flag) { break; }
                }

                if (typeName == "" || typeName == null)
                {
                    throw new TypeLoadException("Plugin is not of Type " + typeof(T).FullName);
                }
            }
            catch (Exception ex)
            {
                throw new TypeLoadException("Error Locating " + typeof(T).FullName, ex);
            }

            if (Arguments != null)
            {
                _Arguments = new List<object>();
                _Arguments.AddRange(Arguments);
            }
            else { _Arguments = new List<object>(); }

            if (_Arguments.Count == 0)
            {
                try
                {
                    var _InstanceResult = _Assembly.CreateInstance(typeName, true, BindingFlags.CreateInstance, null, null, CultureInfo.CurrentCulture, null);
                    if (_InstanceResult == null) { throw new TypeLoadException("Error Creating Instance of " + typeof(T).FullName); }

                    return (T)_InstanceResult;
                }
                catch (Exception ex)
                {
                    throw new TypeLoadException("Error Locating " + typeof(T).FullName, ex);
                }
            }
            else
            {
                var _InstanceResult = _Assembly.CreateInstance(typeName, true, BindingFlags.CreateInstance, null, _Arguments.ToArray(), CultureInfo.CurrentCulture, null);
                if (_InstanceResult == null) { throw new TypeLoadException("Error Creating Instance of " + typeof(T).FullName); }

                return (T)_InstanceResult;
            }
        }

        /// <summary>
        /// Loads all of the available defined types from a Dll.  Must be blank constructors
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <param name="FileLocation">DLL File Locations</param>
        /// <returns>List of all the DLLS Loaded</returns>
        public static List<T> LoadDLL<T>(string FileLocation)
        {
            List<T> objList = new List<T>();
            if (!FileLocation.ToLower().EndsWith(".dll"))
            {
                return objList;
            }

            if (FileLocation.GetDirectoryFromFileLocation().NullOrEmpty())
            {
                FileLocation = AppDomain.CurrentDomain.BaseDirectory.EnsureDirectoryFormat().FindFileReturnPath(FileLocation).EnsureDirectoryFormat() + FileLocation;
            }

            if (FileLocation.NullOrEmpty())
            {
                return objList;
            }

            Assembly assembly = Assembly.LoadFile(FileLocation);
            try
            {
                bool flag = false;
                foreach (Type exportedType in assembly.GetExportedTypes())
                {
                    foreach (Type type in exportedType.GetInterfaces())
                    {
                        if (type.FullName == typeof(T).FullName)
                        {
                            string fullName = exportedType.FullName;
                            objList.Add((T)assembly.CreateInstance(fullName, true, BindingFlags.CreateInstance, null, null, CultureInfo.CurrentCulture, null));
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    throw new TypeLoadException("Plugin is not of Type " + typeof(T).FullName);
                }
            }
            catch (Exception ex)
            {
                throw new TypeLoadException("Error Locating " + typeof(T).FullName, ex);
            }
            return objList;
        }

        /// <summary>
        /// Loads all of the available defined types from a Dll.  Must be blank constructors
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <param name="SearchAssembly">Assembly to Search</param>
        /// <returns>List of all the DLLS Loaded</returns>
        public static List<T> LoadDLL<T>(Assembly SearchAssembly)
        {
            List<T> objList = new List<T>();
            if (SearchAssembly == null) { return objList; }
            Assembly assembly = SearchAssembly;
            try
            {
                foreach (Type exportedType in assembly.GetExportedTypes())
                {
                    foreach (Type type in exportedType.GetInterfaces())
                    {
                        if (type.FullName == null || exportedType.FullName == null) { continue; }
                        if (type.FullName == typeof(T).FullName)
                        {
                            string fullName = exportedType.FullName;
                            var _InstanceResult = assembly.CreateInstance(fullName, true, BindingFlags.CreateInstance, null, null, CultureInfo.CurrentCulture, null);
                            if (_InstanceResult == null)
                            {
                                // TODO Log
                                continue;
                            }
                            objList.Add((T)_InstanceResult);
                        }
                    }
                }
            }
            catch //(Exception ex)
            {
                throw;
                // TODO Log;
            }
            return objList;
        }
    }
}
