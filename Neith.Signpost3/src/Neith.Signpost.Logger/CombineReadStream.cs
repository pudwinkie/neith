using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Neith.Signpost.Logger
{
    public class CombineReadStream : Stream
    {
        public Stream InputA { get; private set; }
        public Stream InputB { get; private set; }

        public override void Close()
        {
            InputA.Close();
            InputB.Close();
            base.Close();
        }

        public CombineReadStream(Stream a, Stream b)
        {
            InputA = a;
            InputB = b;
        }

        private bool EofInputA = false;
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!EofInputA) {
                var rc = InputA.Read(buffer, offset, count);
                if (rc == 0) EofInputA = true;
                else return rc;
            }

            return InputB.Read(buffer, offset, count);
        }


        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length { get { return InputA.Length + InputB.Length; } }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
