using System.IO;
using System.Text;

namespace TSModMail.Application.Entities;

/// <summary>
/// A wrapper over <c cref="MemoryStream"/> for Strings.
/// </summary>
internal class StringStream : MemoryStream
{
    public StringStream() : base() { }

    public StringStream(byte[] buffer) : base(buffer) { }

    public StringStream(int capacity) : base(capacity) { }

    public StringStream(byte[] buffer, bool writable) : base(buffer, writable) { }

    public StringStream(byte[] buffer, int index, int count)
        : base(buffer, index, count) { }

    public StringStream(byte[] buffer, int index, int count, bool writable)
        : base(buffer, index, count, writable) { }

    public StringStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible)
        : base(buffer, index, count, writable, publiclyVisible) { }

    /// <summary>
    /// Automatically encode the given string and append it to the stream.
    /// </summary>
    public void WriteString(string value, Encoding? encoding = null)
    {
        Write((encoding ?? Encoding.UTF8).GetBytes(value));
    }
}
