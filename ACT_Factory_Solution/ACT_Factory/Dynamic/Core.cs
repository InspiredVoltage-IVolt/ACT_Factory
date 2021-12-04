using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace ACT.Core.Factory.Dynamic
{
    public static class Core
    {
        public static void CompileCSharpCode(string codeToCompile, CompilerParameters compilerOptions)
        {

            string _CompilationAttemptID = Guid.NewGuid().ToString();
            string _LogFileName = _CompilationAttemptID.Replace("-", "") + ".log";
            string _ExceptionMessage = "Error Compiling Code: AttemptLogged To: " + _CompilationAttemptID;
            CompilerParameters _CompilerOptions = null;

            if (compilerOptions != null) { _CompilerOptions = compilerOptions; }
            else
            {
                _CompilerOptions = new CompilerParameters();
                _CompilerOptions.GenerateExecutable = false;
                _CompilerOptions.GenerateInMemory = false;
            }

            CSharpCodeProvider _CodeProvider = new CSharpCodeProvider();
            CompilerResults _CompilerResults = _CodeProvider.CompileAssemblyFromSource(_CompilerOptions, codeToCompile);

            if (_CompilerResults == null) { throw new Exception(_ExceptionMessage); }
            if (_CompilerResults.Errors.Count > 0) { throw new Exception(_ExceptionMessage); }
            if (_CompilerResults.CompiledAssembly == null) { throw new Exception(_ExceptionMessage); }



            //var type = compile.CompiledAssembly.GetType("Abc");
            //var abc = Activator.CreateInstance(type);

            //var method = type.GetMethod("Get");
            //var result = method.Invoke(abc, null);
        }
    }
}
