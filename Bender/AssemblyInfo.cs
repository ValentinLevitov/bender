using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]

// This instruction is for Mock<Bender.ILogger> in the Tests library since ILogger is internal
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
