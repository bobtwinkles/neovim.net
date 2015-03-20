// vim: noexpandtab ts=4 sts=4 sw=4 colorcolumn=120
using System;
using System.IO;

namespace Neovim.Msgpack
{
	public class BigEndianBinaryReader
	{
		private byte[] _buffer;
		private Stream _stream;

		public BigEndianBinaryReader (Stream s)
		{
			_buffer = new byte[8]; // Size of the largest type we natively support reading in
			_stream = s;
		}

		private void ReadBytes (int count)
		{
			if (count > _buffer.Length) {
				throw new ArgumentOutOfRangeException (String.Format ("Count {} > buffer size {}", count, _buffer.Length));
			}
			_stream.Read (_buffer, 0, count);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer, 0, count);
			}
		}

		#region integer types

		public Int64 ReadInt64 ()
		{
			ReadBytes (8);
			return BitConverter.ToInt64 (_buffer, 0);
		}

		public UInt64 ReadUInt64 ()
		{
			ReadBytes (8);
			return BitConverter.ToUInt64 (_buffer, 0);
		}

		public Int32 ReadInt32 ()
		{
			ReadBytes (4);
			return BitConverter.ToInt32 (_buffer, 0);
		}

		public UInt32 ReadUInt32 ()
		{
			ReadBytes (4);
			return BitConverter.ToUInt32 (_buffer, 0);
		}

		public Int16 ReadInt16 ()
		{
			ReadBytes (2);
			return BitConverter.ToInt16 (_buffer, 0);
		}

		public UInt16 ReadUInt16 ()
		{
			ReadBytes (2);
			return BitConverter.ToUInt16 (_buffer, 0);
		}

		public byte ReadByte ()
		{
			ReadBytes (1);
			return _buffer [0];
		}

		public sbyte ReadInt8 ()
		{
			ReadBytes (1);
			return ((sbyte)_buffer [0]);
		}

		#endregion

		#region floating point types

		public Single ReadSingle ()
		{
			ReadBytes (4);
			return BitConverter.ToSingle (_buffer, 0);
		}

		public Double ReadDouble ()
		{
			ReadBytes (8);
			return BitConverter.ToDouble (_buffer, 0);
		}

		#endregion
	}

	public class BigEndianBinaryWriter
	{
		private byte[] _buffer;
		private Stream _stream;

		public BigEndianBinaryWriter (Stream s)
		{
			_stream = s;
		}

		#region integer types

		public void Write (Int64 arg)
		{
			_buffer = BitConverter.GetBytes (arg);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer);
			}
			_stream.Write (_buffer, 0, _buffer.Length);
		}

		public void Write (UInt64 arg)
		{
			_buffer = BitConverter.GetBytes (arg);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer);
			}
			_stream.Write (_buffer, 0, _buffer.Length);
		}

		public void Write (Int32 arg)
		{
			_buffer = BitConverter.GetBytes (arg);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer);
			}
			_stream.Write (_buffer, 0, _buffer.Length);
		}

		public void Write (UInt32 arg)
		{
			_buffer = BitConverter.GetBytes (arg);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer);
			}
			_stream.Write (_buffer, 0, _buffer.Length);
		}

		public void Write (Int16 arg)
		{
			_buffer = BitConverter.GetBytes (arg);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer);
			}
			_stream.Write (_buffer, 0, _buffer.Length);
		}

		public void Write (UInt16 arg)
		{
			_buffer = BitConverter.GetBytes (arg);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer);
			}
			_stream.Write (_buffer, 0, _buffer.Length);
		}

		public void Write (byte arg)
		{
			_buffer = BitConverter.GetBytes (arg);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer);
			}
			_stream.Write (_buffer, 0, _buffer.Length);
		}

		public void Write (sbyte arg)
		{
			_stream.WriteByte (unchecked((byte)arg));
		}

		#endregion

		#region floating point types

		public void Write (Single arg)
		{
			_buffer = BitConverter.GetBytes (arg);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer);
			}
			_stream.Write (_buffer, 0, _buffer.Length);
		}

		public void Write (Double arg)
		{
			_buffer = BitConverter.GetBytes (arg);
			if (BitConverter.IsLittleEndian) {
				Array.Reverse (_buffer);
			}
			_stream.Write (_buffer, 0, _buffer.Length);
		}

		#endregion
	}
}

