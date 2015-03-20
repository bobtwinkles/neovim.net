// vim: noexpandtab ts=4 sts=4 sw=4 colorcolumn=120
using System;
using System.Collections.Generic;
using System.IO;

namespace Neovim.Msgpack
{
	public enum MsgpackType : byte
	{
		POSFIXINT = 0x00,
		FIXMAP = 0x80,
		FIXARRAY = 0x90,
		FIXSTR = 0xA0,
		NIL = 0xC0,

		FALSE = 0xC2,
		TRUE = 0xC3,

		BIN8 = 0xC4,
		BIN16 = 0xC5,
		BIN32 = 0xC6,

		EXT8 = 0xC7,
		EXT16 = 0xC8,
		EXT32 = 0xC9,

		SINGLE = 0xCA,
		DOUBLE = 0xCB,

		BYTE = 0xCC,
		UINT16 = 0xCD,
		UINT32 = 0xCE,
		UINT64 = 0xCF,

		INT8 = 0xD0,
		INT16 = 0xD1,
		INT32 = 0xD2,
		INT64 = 0xD3,

		FIXEXT1 = 0xD4,
		FIXEXT2 = 0xD5,
		FIXEXT4 = 0xD6,
		FIXEXT8 = 0xD7,
		FIXEXT16 = 0xD8,

		STRING8 = 0xD9,
		STRING16 = 0xDA,
		STRING32 = 0xDB,

		ARRAY16 = 0xDC,
		ARRAY32 = 0xDD,

		MAP16 = 0xDE,
		MAP32 = 0xDF,
		NEGFIXINT = 0xE0
	}

	public class MsgpackWriter : IDisposable
	{
		private readonly Stream _transport;
		private BigEndianBinaryWriter _writer;

		public MsgpackWriter (Stream transport)
		{
			this._transport = transport;
			this._writer = new BigEndianBinaryWriter (_transport);
		}

		public void Dispose ()
		{
			_transport.Dispose ();
			_writer = null;
		}

		#region Writers

		public void WritePosFixInt (byte i)
		{
			if (i > 127) { // max value of a 7-bit positive integer
				throw new ArgumentOutOfRangeException ("i", i, "PosFixInt greater than 127");
			}
			i = (byte)(i & 0x7F);
			_writer.Write (i);
		}

		public void WriteNegFixInt (sbyte i)
		{
			//      1110000b
			if (i > 0) {
				throw new ArgumentOutOfRangeException ("i", i, "NegFixInt greater than 0");
			}
			if (i < -32) {
				throw new ArgumentOutOfRangeException ("i", i, "Negative fixint < -32");
			}
			_writer.Write (i);
		}

		public void WriteNil ()
		{
			_writer.Write ((byte)0xC0);
		}

		public void WriteBool (bool b)
		{
			if (b) {
				_writer.Write ((byte)0xC3);
			} else {
				_writer.Write ((byte)0xC2);
			}
		}

		public void WriteByte (byte i)
		{
			_writer.Write ((byte)0xCC);
			_writer.Write (i);
		}

		public void WriteUInt16 (UInt16 i)
		{
			_writer.Write ((byte)0xCD);
			_writer.Write (i);
		}

		public void WriteUInt32 (UInt32 i)
		{
			_writer.Write ((byte)0xCE);
			_writer.Write (i);
		}

		public void WriteUInt64 (UInt64 i)
		{
			_writer.Write ((byte)0xCF);
			_writer.Write (i);
		}

		public void WriteInt8 (sbyte i)
		{
			_writer.Write ((byte)0xD0);
			_writer.Write (i);
		}

		public void WriteInt16 (Int16 i)
		{
			_writer.Write ((byte)0xD1);
			_writer.Write (i);
		}

		public void WriteInt32 (Int32 i)
		{
			_writer.Write ((byte)0xD2);
			_writer.Write (i);
		}

		public void WriteInt64 (Int64 i)
		{
			_writer.Write ((byte)0xD3);
			_writer.Write (i);
		}

		public void WriteSingle (Single s)
		{
			_writer.Write ((byte)0xCA);
			_writer.Write (s);
		}

		public void WriteDouble (Double d)
		{
			_writer.Write ((byte)0xCB);
			_writer.Write (d);
		}

		public void WriteString (string s)
		{
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes (s);
			if (bytes.Length < 32) {
				_writer.Write ((byte)(0xA0 | bytes.Length));
			} else if (bytes.Length < 256) {
				_writer.Write (0xD9);
				_writer.Write ((byte)bytes.Length);
			} else if (bytes.Length < 0x1000) { // (2 ^ 16)
				_writer.Write (0xDA);
				_writer.Write ((UInt16)bytes.Length);
			} else {
				_writer.Write (0xDB);
				_writer.Write ((UInt32)bytes.Length);
			}
			_transport.Write (bytes, 0, bytes.Length);
		}

		public void WriteBytes (byte[] bytes)
		{
			if (bytes.Length < 256) {
				_writer.Write ((byte)0xC4);
				_writer.Write ((byte)bytes.Length);
			} else if (bytes.Length < 0x1000) { // (2 ^ 16)
				_writer.Write ((byte)0xC5);
				_writer.Write ((UInt16)bytes.Length);
			} else {
				_writer.Write ((byte)0xC6);
				_writer.Write ((UInt32)bytes.Length);
			}
			_transport.Write (bytes, 0, bytes.Length);
		}

		public void WriteArray (PackedList obj)
		{
			if (obj.Length < 16) {
				_writer.Write ((byte)(0x90 | obj.Length));
			} else if (obj.Length < 0x1000) {
				_writer.Write ((byte)0xDC);
				_writer.Write ((UInt16)obj.Length);
			} else {
				_writer.Write ((byte)0xDD);
				_writer.Write ((UInt32)obj.Length);
			}
			foreach (var i in obj.Values) {
				i.WriteTo (this);
			}
		}

		public void WriteDictionary (PackedMap dict)
		{
			if (dict.Count < 16) {
				_writer.Write ((byte)(0x80 | dict.Count));
			} else if (dict.Count < 0x1000) {
				_writer.Write ((byte)0xDE);
				_writer.Write ((UInt16)dict.Count);
			} else {
				_writer.Write ((byte)0xDF);
				_writer.Write ((UInt32)dict.Count);
			}
			foreach (var s in dict) {
				s.Key.WriteTo (this);
				s.Value.WriteTo (this);
			}
		}

		public void WriteObjectlength (sbyte type, int length)
		{
			if (length == 1) {
				_writer.Write ((byte)0xD4);
			} else if (length == 2) {
				_writer.Write ((byte)0xD5);
			} else if (length == 4) {
				_writer.Write ((byte)0xD6);
			} else if (length == 8) {
				_writer.Write ((byte)0xD7);
			} else if (length == 16) {
				_writer.Write ((byte)0xD8);
			} else if (length < 256) {
				_writer.Write ((byte)0xC7);
				_writer.Write ((byte)length);
			} else if (length < 0x1000) {
				_writer.Write ((byte)0xC8);
				_writer.Write ((UInt16)length);
			} else {
				_writer.Write ((byte)0xC9);
				_writer.Write ((UInt32)length);
			}
			_writer.Write (type);
		}

		#endregion
	}

	public class MsgpackReader : IDisposable
	{
		private readonly Stream _transport;
		private BigEndianBinaryReader _reader;
		private byte _peek;
		private bool _did_peek;

		public MsgpackReader (Stream transport)
		{
			this._transport = transport;
			this._reader = new BigEndianBinaryReader (_transport);
			this._peek = 0;
			this._did_peek = false;
		}

		public void Dispose ()
		{
			_transport.Dispose ();
			_reader = null;
		}

		#region reader utilities

		private byte ReadTransport ()
		{
			if (_did_peek) {
				_did_peek = false;
				return _peek;
			}
			int r = _transport.ReadByte ();
			if (r < 0) {
				throw new InvalidDataException ("Unexpected end of stream");
			}
			return (byte)r;
		}

		private byte PeekTransport ()
		{
			byte b = ReadTransport ();
			_peek = b;
			_did_peek = true;
			return _peek;
		}

		private void ExpectType (byte type)
		{
			int b = ReadTransport ();
			if (b != type) {
				throw new InvalidDataException (String.Format ("Unexpected type code {0:X}, expected {1:X}", b, type));
			}
		}

		#endregion

		#region reader

		public byte ReadPosFixInt ()
		{
			int b = ReadTransport ();
			if ((b & 0x80) != 0) {
				throw new InvalidDataException ("MSB of positive fixint was set");
			}
			if (b >= 128 || b < 0) {
				throw new InvalidDataException (String.Format ("Positive fixint was {0}, out of range [0-127]", b));
			}
			return (byte)b;
		}

		public sbyte ReadNegFixInt ()
		{
			int b = ReadTransport ();
			if ((b & 0xe0) != 0xe0) {
				throw new InvalidDataException ("Three MSB of negative fixint were not set");
			}
			return (sbyte)b;
		}

		public bool ReadBool ()
		{
			int b = ReadTransport ();
			if (b == 0xC3) {
				return true;
			}
			if (b == 0xC2) {
				return false;
			}
			throw new InvalidDataException (String.Format ("Expected boolean, but got {0:X}", b));
		}

		public byte ReadByte ()
		{
			ExpectType (0xCC);
			return ReadTransport ();
		}

		public UInt16 ReadUInt16 ()
		{
			ExpectType (0xCD);
			return _reader.ReadUInt16 ();
		}

		public UInt32 ReadUInt32 ()
		{
			ExpectType (0xCE);
			return _reader.ReadUInt32 ();
		}

		public UInt64 ReadUInt64 ()
		{
			ExpectType (0xCF);
			return _reader.ReadUInt64 ();
		}

		public byte ReadInt8 ()
		{
			ExpectType (0xD0);
			return ReadTransport ();
		}

		public Int16 ReadInt16 ()
		{
			ExpectType (0xD1);
			return _reader.ReadInt16 ();
		}

		public Int32 ReadInt32 ()
		{
			ExpectType (0xD2);
			return _reader.ReadInt32 ();
		}

		public Int64 ReadInt64 ()
		{
			ExpectType (0xD3);
			return _reader.ReadInt64 ();
		}

		public Single ReadSingle ()
		{
			ExpectType (0xCA);
			return _reader.ReadSingle ();
		}

		public Double ReadDouble ()
		{
			ExpectType (0xCB);
			return _reader.ReadDouble ();
		}

		public string ReadString ()
		{
			byte b = ReadTransport ();
			uint strlen;
			if ((b & 0xA0) == 0xA0) { // 32 char or less string
				strlen = b ^ (uint)0xA0; // unset the header bits
			} else if (b == 0xD9) { // UInt8 length string
				strlen = ReadTransport ();
			} else if (b == 0xDA) { // UInt16 length string
				strlen = _reader.ReadUInt16 ();
			} else if (b == 0xDB) { // UInt32 length string
				strlen = _reader.ReadUInt32 ();
			} else {
				throw new InvalidDataException (String.Format ("Expected a string header but got {0:X}", b));
			}
			var data = new byte[strlen];
			_transport.Read (data, 0, (int)strlen);
			return System.Text.Encoding.UTF8.GetString (data);
		}

		public byte[] ReadBytes ()
		{
			byte b = ReadTransport ();
			uint len;
			switch (b) {
			case 0xC4:
				len = ReadTransport ();
				break;
			case 0xC5:
				len = ReadUInt16 ();
				break;
			case 0xC6:
				len = ReadUInt32 ();
				break;
			default:
				throw new InvalidDataException (String.Format ("Expected a bytes header but got {0:X}", b));
			}
			var tr = new byte[len];
			_transport.Read (tr, 0, (int)len);
			return tr;
		}

		/// This logic needs to be in here since msgpack encodes the length in the type sometimes
		public void ReadArray(PackedList o) {
			uint length;
			byte b = ReadTransport();
			if ((b & 0x90) == 0x90) {
				length = (uint)(b & (~0x90));
			} else if (b == 0xDC) {
				length = _reader.ReadUInt16();
			} else if (b == (0xDD)) {
				length = _reader.ReadUInt32();
			} else {
				throw new InvalidDataException(String.Format ("Expected a list header, but got {0:X}", b));
			}
			o.Values = new EncapsulatedValue[length];
			for (var i = 0; i < length; ++i) {
				o.Values[i] = ReadEncapsulated();
			}
		}

		/// This logic needs to be in here since msgpack encodes the length in the type sometimes
		public void ReadDictionary(PackedMap dic) {
			uint length;
			byte b = ReadTransport();
			if ((b & 0x80) == 0x80) {
				length = (uint)(b & (~0x80));
			} else if (b == 0xDE) {
				length = _reader.ReadUInt16();
			} else if (b == 0xDF) {
				length = _reader.ReadUInt32();
			} else {
				throw new InvalidDataException(String.Format ("Expected a map header, but got {0:X}", b));
			}
			for (var i = 0; i < length; ++i) {
				EncapsulatedValue key = ReadEncapsulated();
				dic[key] = ReadEncapsulated();
			}
		}

		public void ReadObjectHead(out sbyte type, out uint length)
		{
			byte b = ReadTransport();
			switch(b) {
				case 0xD4: length = 1; break;
				case 0xD5: length = 2; break;
				case 0xD6: length = 4; break;
				case 0xD7: length = 8; break;
				case 0xD8: length = 16; break;
				case 0xC7: length = _reader.ReadByte(); break;
				case 0xC8: length = _reader.ReadUInt16(); break;
				case 0xC9: length = _reader.ReadUInt32(); break;
				default:
						   throw new InvalidDataException(String.Format ("Expected object header, but got {0:X}", b));
			}
			type = _reader.ReadInt8();
		}

		public MsgpackType PeekType ()
		{
			byte b = PeekTransport ();
			if ((b & 0x80) != 0) {
				return MsgpackType.POSFIXINT;
			}
			if ((b & 0x80) == 0x80) {
				return MsgpackType.FIXMAP;
			}
			if ((b & 0x90) == 0x90) {
				return MsgpackType.FIXARRAY;
			}
			if ((b & 0xA0) == 0xA0) {
				return MsgpackType.FIXSTR;
			}
			if ((b & 0xE0) == 0xE0) {
				return MsgpackType.NEGFIXINT;
			}
			if (b == 0xC1) { //guaranteed unused by the spec
				throw new InvalidDataException ("Got a 0xC1 byte when reading type");
			}
			return (MsgpackType)b;
		}

		public EncapsulatedValue ReadEncapsulated ()
		{
			return new EncapsulatedValue(this);
		}

		#endregion
	}
}
