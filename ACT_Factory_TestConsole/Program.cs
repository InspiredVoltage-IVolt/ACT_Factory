using System.Reflection;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Assembly a = Assembly.LoadFile(@"D:\IVolt_Releases\ACT_Factory\Windows\Debug\net6.0\ACT_Factory.dll");
            string s = a.ImageRuntimeVersion;

            Console.WriteLine(s);
            Console.ReadKey();
        }
    }
}