using System.Reflection;
using System.Runtime.InteropServices;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ad64129d-7599-4f38-9279-824cea9fc64d")]

// Required for Lambdas
[assembly: LambdaSerializer(typeof(JsonSerializer))]
