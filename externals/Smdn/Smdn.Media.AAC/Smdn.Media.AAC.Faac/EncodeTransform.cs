// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using Smdn.Interop;

namespace Smdn.Media.AAC.Faac {
  /*
   * http://www3.atwiki.jp/esperance/pages/15.html
   * libfaac: docs/libfaac.html
   * libfaac: frontend/main.c
   */
  public class EncodeTransform : ICryptoTransform {
    /*
     * ICryptoTransform properties
     */
    public bool CanTransformMultipleBlocks {
      get { return true; }
    }

    public bool CanReuseTransform {
      get { return false; }
    }

    public int InputBlockSize {
      get { return inputBlockSize; }
    }

    public int OutputBlockSize {
      get { return outputBlockSize; }
    }

    /*
     * other properties
     */
    public int TotalSampleCount {
      get { return totalSampleCount; }
    }

    public IEnumerable<ChunkSizeOffset> ChunkSizesAndOffsets {
      get { return chunkSizesAndOffsets; }
    }

    [CLSCompliant(false)]
    public EncodeTransform(WAVE_FORMAT format)
      : this(format, MpegVersion.Mpeg4, AacObjectType.Low)
    {
    }

    [CLSCompliant(false)]
    public EncodeTransform(WAVE_FORMAT format, MpegVersion mpegVersion, AacObjectType aacObjectType)
    {
      if (!libfaac.IsAvailable)
        throw new NotSupportedException("libfaac is not found or is not available");

      var pcmformat = WAVEFORMATEX.CreateLinearPcmFormat(format);

      if (pcmformat.wBitsPerSample != 16)
        throw new NotSupportedException("unsupported: bits per sample != 16");

      hEncoder = libfaac.faacEncOpen(pcmformat.nSamplesPerSec, (uint)pcmformat.nChannels, out inputSamples, out maxOutputBytes);

      if (hEncoder == IntPtr.Zero)
        throw new NotSupportedException("faacEncOpen failed");

      unsafe {
        FaacEncConfiguration* config = libfaac.faacEncGetCurrentConfiguration(hEncoder);

        (*config).useTns = 0;
        (*config).allowMidside = 1;
        //(*config).bitRate = (bitrate / pcmformat.nChannels);
        //(*config).bandWidth = 
        (*config).mpegVersion = mpegVersion;
        (*config).aacObjectType = aacObjectType;
        (*config).inputFormat = FaacInput.Bits16;
        (*config).outputFormat = StreamFormat.ADTS;

        if (0 == libfaac.faacEncSetConfiguration(hEncoder, config))
          throw new NotSupportedException("unsupported format");
      }

      this.bytesPerSample = pcmformat.wBitsPerSample / 8;
      this.inputBlockSize = (int)(inputSamples * bytesPerSample); // XXX: uint -> int
      this.outputBlockSize = (int)maxOutputBytes; // XXX: uint -> int
      this.unencodedBuffer = new CoTaskMemoryBuffer(inputBlockSize);
      this.unencodedBufferOffset = 0;
    }

    ~EncodeTransform()
    {
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (hEncoder != IntPtr.Zero) {
          libfaac.faacEncClose(hEncoder);
          hEncoder = IntPtr.Zero;
        }

        if (unencodedBuffer != null) {
          unencodedBuffer.Dispose();
          unencodedBuffer = null;
        }
      }
    }

    void IDisposable.Dispose()
    {
      Clear();
    }

    public void Clear()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public byte[] GetAsc()
    {
      CheckDisposed();

      var ppAsc = IntPtr.Zero;
      uint length;

      try {
        libfaac.faacEncGetDecoderSpecificInfo(hEncoder, out ppAsc, out length);

        if (ppAsc == IntPtr.Zero) {
          return null;
        }
        else {
          var ret = new byte[length];

          Marshal.Copy(ppAsc, ret, 0, (int)length); // XXX: uint -> int

          return ret;
        }
      }
      finally {
        if (ppAsc != IntPtr.Zero)
          // XXX: free()
          Marshal.FreeHGlobal(ppAsc);
      }
    }

    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
      CheckDisposed();

      if (inputBuffer == null)
        throw new ArgumentNullException("inputBuffer");
      if (inputOffset < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("inputOffset", inputOffset);
      if (inputCount < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("inputCount", inputCount);
      if (inputBuffer.Length - inputCount < inputOffset)
        throw ExceptionUtils.CreateArgumentAttemptToAccessBeyondEndOfArray("inputOffset", inputBuffer, inputOffset, inputCount);

      if (outputBuffer == null)
        throw new ArgumentNullException("outputBuffer");
      if (outputOffset < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("outputOffset", outputOffset);
#if false
      if (outputBuffer.Length - inputCount < outputOffset)
        throw ExceptionUtils.CreateArgumentAttemptToAccessBeyondEndOfArray("outputOffset", outputBuffer, outputOffset, inputCount);
#endif

      var ret = 0;
      var outputBufferSize = outputBuffer.Length - outputOffset;

      unsafe {
        fixed (byte* outptr = outputBuffer)
        fixed (byte* inptr = inputBuffer) {
          byte* outputBufferPtr = outptr;

          for (;;) {
            int sampleCount = inputCount / bytesPerSample;

            if (inputSamples < sampleCount)
              sampleCount = (int)inputSamples;

            int inputLength = sampleCount * bytesPerSample;
            int* inputBufferPtr;

            if (0 < unencodedBufferOffset) {
              unencodedBuffer.Write(inputBuffer, inputOffset, inputLength, unencodedBufferOffset);
              inputBufferPtr = (int*)unencodedBuffer;
            }
            else {
              inputBufferPtr = (int*)(inptr + inputOffset);
            }

            inputOffset += inputLength;
            inputCount  -= inputLength;

            var encoded = libfaac.faacEncEncode(hEncoder, inputBufferPtr, (uint)sampleCount, outputBufferPtr, (uint)outputBufferSize);

            ret += encoded;
            outputBufferPtr  += encoded;
            outputBufferSize -= encoded;

            totalSampleCount += sampleCount;

            if (0 < encoded) {
              chunkSizesAndOffsets.Add(new ChunkSizeOffset(encoded, chunkOffset));
              chunkOffset += encoded;
            }

            unencodedBufferOffset = 0;

            if (inputCount < bytesPerSample) {
              unencodedBuffer.Write(inputBuffer, inputOffset, inputCount, 0);
              unencodedBufferOffset = inputCount;
              break;
            }
          }
        }
      }

      return ret;
    }

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
      CheckDisposed();

      if (inputBuffer == null)
        throw new ArgumentNullException("inputBuffer");
      if (inputOffset < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("inputOffset", inputOffset);
      if (inputCount < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("inputCount", inputCount);
      if (inputBuffer.Length - inputCount < inputOffset)
        throw ExceptionUtils.CreateArgumentAttemptToAccessBeyondEndOfArray("inputOffset", inputBuffer, inputOffset, inputCount);
      if (InputBlockSize < inputCount)
        throw ExceptionUtils.CreateArgumentMustBeLessThanOrEqualTo("InputBlockSize", "inputCount", inputCount);

      var outputBuffer = new byte[(1 + inputCount / inputBlockSize) * outputBlockSize];
      var len = TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);

      Array.Resize(ref outputBuffer, len);

      return outputBuffer;
    }

    private void CheckDisposed()
    {
      if (hEncoder == IntPtr.Zero)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private IntPtr hEncoder = IntPtr.Zero;
    private uint inputSamples;
    private uint maxOutputBytes;
    private int inputBlockSize;
    private int outputBlockSize;
    private int bytesPerSample;
    private CoTaskMemoryBuffer unencodedBuffer;
    private int unencodedBufferOffset;
    private int totalSampleCount = 0;
    private int chunkOffset = 0;
    private List<ChunkSizeOffset> chunkSizesAndOffsets = new List<ChunkSizeOffset>();
  }
}
