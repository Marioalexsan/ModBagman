using System.ComponentModel;

namespace System.Runtime.CompilerServices;

// Used for allowing init-style setters in .NET Framework
// This is a hack according to Microsoft, so preferably don't use it in your own code and stick to modern .NET versions
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit { }