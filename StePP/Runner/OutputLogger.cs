using System;
using System.IO;
using System.Text;

namespace StePP.Runner
{
    public class OutputLogger : Stream
    {
        private readonly Stream _baseStream;
        private readonly byte[] _prefix;

        public OutputLogger(string path, string prefix)
        {
            _prefix = Encoding.UTF8.GetBytes(prefix);

            _baseStream = path == "-" ? Console.OpenStandardOutput() : File.OpenWrite(path);
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var prefixedBuffer = new byte[_prefix.Length + count];
            Buffer.BlockCopy(_prefix, 0, prefixedBuffer, 0, _prefix.Length);
            Buffer.BlockCopy(buffer, offset, prefixedBuffer, _prefix.Length, count);
            _baseStream.Write(prefixedBuffer, 0, prefixedBuffer.Length);
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }
    }
}
