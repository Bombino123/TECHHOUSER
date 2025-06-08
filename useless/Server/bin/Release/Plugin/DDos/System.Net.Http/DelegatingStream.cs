using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http;

internal abstract class DelegatingStream : Stream
{
	public override extern bool CanRead { get; }

	public override extern bool CanSeek { get; }

	public override extern bool CanWrite { get; }

	public override extern long Length { get; }

	public override extern long Position { get; set; }

	public override extern int ReadTimeout { get; set; }

	public override extern bool CanTimeout { get; }

	public override extern int WriteTimeout { get; set; }

	protected extern DelegatingStream(Stream innerStream);

	protected override extern void Dispose(bool disposing);

	public override extern long Seek(long offset, SeekOrigin origin);

	public override extern int Read(byte[] buffer, int offset, int count);

	public override extern IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state);

	public override extern int EndRead(IAsyncResult asyncResult);

	public override extern int ReadByte();

	public override extern Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

	public override extern void Flush();

	public override extern Task FlushAsync(CancellationToken cancellationToken);

	public override extern void SetLength(long value);

	public override extern void Write(byte[] buffer, int offset, int count);

	public override extern IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state);

	public override extern void EndWrite(IAsyncResult asyncResult);

	public override extern void WriteByte(byte value);

	public override extern Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
}
