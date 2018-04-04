using System;
using System.IO;
using System.Text;

namespace StePP.Runner
{
    public class OutputLogger : Stream
    {
        private Stream _baseStream;
        private bool _doPrefix;
        private byte[] _prefix;

        public OutputLogger(Stream stream, string prefix)
        {
            _baseStream = stream;
            SetupPrefix(prefix);
        }

        public OutputLogger(string path)
        {
            SetupBaseStream(path);
            _prefix = new byte[0];
        }

        public OutputLogger(string path, string prefix)
        {
            SetupBaseStream(path);
            SetupPrefix(prefix);
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

        public void WriteLine(string line)
        {
            var bytes = Encoding.UTF8.GetBytes(line + "\n");
            Write(bytes, 0, bytes.Length);
        }

        public override void Flush() => _baseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

        public override void SetLength(long value) => _baseStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_doPrefix)
            {
                _baseStream.Write(buffer, offset, count);
                return;
            }

            var dateBytes = GetDateStampBytes();
            var prefixBytes = _prefix;

            var bytes = new byte[dateBytes.Length + prefixBytes.Length + count];

            Buffer.BlockCopy(dateBytes, 0, bytes, 0, dateBytes.Length);
            Buffer.BlockCopy(prefixBytes, 0, bytes, dateBytes.Length, prefixBytes.Length);
            Buffer.BlockCopy(buffer, offset, bytes, dateBytes.Length + prefixBytes.Length, count);

            _baseStream.Write(bytes, 0, bytes.Length);
        }

        public override void Close()
        {
            _baseStream.Close();
            base.Close();
        }

        private void SetupPrefix(string prefix)
        {
            _prefix = Encoding.UTF8.GetBytes(prefix + " | ");
            _doPrefix = true;
        }

        private void SetupBaseStream(string path) => _baseStream = path == "-" ? Console.OpenStandardOutput() : File.OpenWrite(path);

        private static byte[] GetDateStampBytes() => Encoding.UTF8.GetBytes(DateTime.Now.ToString("s") + " | ");
    }
}