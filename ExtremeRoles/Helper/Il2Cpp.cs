using System;
using System.IO;
using System.Runtime.CompilerServices;
using HarmonyLib;

using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;

using ExtremeRoles.Module;

namespace ExtremeRoles.Helper
{
    public static class Il2Cpp
    {
        public static object TryCast(this Il2CppObjectBase self, Type type)
        {
            return AccessTools.Method(
                self.GetType(),
                nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
        }
        public static StreamWrapper ToIl2Cpp(this Stream stream)
        {
            return new StreamWrapper(stream);
        }
    }

    [Il2CppRegister]
    public sealed class StreamWrapper : Il2CppSystem.IO.Stream
    {
        private readonly Stream _stream;

#pragma warning disable 8618
        public StreamWrapper(IntPtr ptr) : base(ptr) { }
#pragma warning restore 8618

        public StreamWrapper(Stream stream) : base(ClassInjector.DerivedConstructorPointer<StreamWrapper>())
        {
            ClassInjector.DerivedConstructorBody(this);
            _stream = stream;
        }

        [HideFromIl2Cpp]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Span<byte> GetSpan(
            Il2CppStructArray<byte> buffer, int offset, int count)
        {
            var rawBuffer = (byte*)buffer.Pointer + 4 * IntPtr.Size;
            return new Span<byte>(rawBuffer + offset, count);
        }

        public override int Read(Il2CppStructArray<byte> buffer, int offset, int count)
        {
            return _stream.Read(GetSpan(buffer, offset, count));
        }

        public override void Write(Il2CppStructArray<byte> buffer, int offset, int count)
        {
            _stream.Write(GetSpan(buffer, offset, count));
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override void Close()
        {
            _stream.Close();
        }

        public override void Dispose()
        {
            _stream.Dispose();
        }

        public override long Seek(long offset, Il2CppSystem.IO.SeekOrigin origin)
        {
            return _stream.Seek(offset, origin switch
            {
                Il2CppSystem.IO.SeekOrigin.Begin => SeekOrigin.Begin,
                Il2CppSystem.IO.SeekOrigin.Current => SeekOrigin.Current,
                Il2CppSystem.IO.SeekOrigin.End => SeekOrigin.End,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            });
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }
    }
}
