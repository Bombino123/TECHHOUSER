using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
internal sealed class RazorImportNamespaceAttribute : Attribute
{
	[NotNull]
	public string Name { get; private set; }

	public RazorImportNamespaceAttribute([NotNull] string name)
	{
		Name = name;
	}
}
