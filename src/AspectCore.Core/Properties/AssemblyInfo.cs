using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("AspectCore.Core")]
[assembly: AssemblyTrademark("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("54ffb88e-6e90-4e57-886d-a8430039da23")]

#if DEBUG
[assembly: InternalsVisibleTo("AspectCore.Core.Benchmark")]
#else
[assembly: InternalsVisibleTo("AspectCore.Core.Benchmark, PublicKey=" +
                              "0024000004800000940000000602000000240000525341310004000001000100e5a34dfa0bd597" +
                              "39067521c28b809e6653358a008148f35c8d3357dc02d90ef3eb3365fb55903bdcd14dbfe2b73a" +
                              "10361c71c948b5ffcec2bf17e6c7a2ef98494d34d6e00d671b32566d153b8139d1caa0d5a9b071" +
                              "e15b6849fbabea83fc9b8b6abf959e606f5e51b268a6a6c2d4757bbc3ae33689373faaedf61077" +
                              "59678c9b")]
#endif
