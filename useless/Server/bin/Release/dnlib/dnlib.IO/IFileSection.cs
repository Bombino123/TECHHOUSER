namespace dnlib.IO;

public interface IFileSection
{
	FileOffset StartOffset { get; }

	FileOffset EndOffset { get; }
}
