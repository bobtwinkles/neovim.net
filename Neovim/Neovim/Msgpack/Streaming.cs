// vim: noexpandtab ts=4 sts=4 sw=4 colorcolumn=120
using System;
using System.Collections.Generic;
using System.IO;

namespace Neovim.Msgpack
{
	public class EncapsulatedValue : IMarshalable
	{
		public MsgpackType Type {
			get;
			private set;
		}

		public object Value {
			get;
			private set;
		}

		private void EnsureType (object v, Type t)
		{
			if (!(v.GetType ().IsSubclassOf (t))) {
				throw new ArgumentException (String.Format ("Value must be {0}", t));
			}
		}

		private int GetWidthRank (Type itype)
		{
			if (itype == typeof(sbyte))
				return 0;
			if (itype == typeof(byte))
				return 1;
			if (itype == typeof(Int16))
				return 2;
			if (itype == typeof(UInt16))
				return 3;
			if (itype == typeof(Int32))
				return 4;
			if (itype == typeof(UInt32))
				return 5;
			if (itype == typeof(Int64))
				return 6;
			if (itype == typeof(UInt64))
				return 7;
			throw new ArgumentException (String.Format ("unknown integer format {0}", itype));
		}

		private void EnsureSafe (object i, Type t)
		{
			if (GetWidthRank (i.GetType ()) > GetWidthRank (t)) {
				throw new ArgumentException (String.Format ("Value of type {0} too large to be contained in type {1}", i.GetType (), t));
			}
		}

		internal EncapsulatedValue(MsgpackReader r) {
			this.ReadFrom(r);
		}

		public EncapsulatedValue (MsgpackType type, object val)
		{
			if (val == null && type != MsgpackType.NIL) {
				throw new ArgumentNullException ("val");
			}

			Type = type;
			Value = val;
			// type checking!
			switch (type) {
			case MsgpackType.POSFIXINT:
					// Should be safe for most integer types, otherwise will blow up (which is just fine... we don't
					// actually *want* anything other than bytes here, really...
				int i = (int)val;
				if (i < 0) {
					throw new ArgumentOutOfRangeException ("val", i, "Should be >= 0");
				}
				if (i > 255) {
					throw new ArgumentOutOfRangeException ("val", i, "Should be < 256");
				}
				break;
			case MsgpackType.MAP32:
			case MsgpackType.MAP16:
			case MsgpackType.FIXMAP:
				EnsureType (val, typeof(PackedMap));
				break;
			case MsgpackType.ARRAY32:
			case MsgpackType.ARRAY16:
			case MsgpackType.FIXARRAY:
				EnsureType (val, typeof(PackedList));
				break;
			case MsgpackType.STRING32:
			case MsgpackType.STRING16:
			case MsgpackType.STRING8:
			case MsgpackType.FIXSTR:
				EnsureType (val, typeof(string));
				break;
			case MsgpackType.NIL:
				if (val != null) {
					throw new ArgumentException ("Value must be null for a NIL");
				}
				break;
			case MsgpackType.FALSE:
			case MsgpackType.TRUE:
				EnsureType (val, typeof(bool));
				break;
			case MsgpackType.BIN32:
			case MsgpackType.BIN16:
			case MsgpackType.BIN8:
				EnsureType (val, typeof(byte[]));
				break;
			case MsgpackType.EXT32:
			case MsgpackType.EXT16:
			case MsgpackType.EXT8:
			case MsgpackType.FIXEXT1:
			case MsgpackType.FIXEXT2:
			case MsgpackType.FIXEXT4:
			case MsgpackType.FIXEXT8:
			case MsgpackType.FIXEXT16:
				EnsureType (val, typeof(IMarshalable));
				break;
			case MsgpackType.SINGLE:
				EnsureType (val, typeof(Single));
				break;
			case MsgpackType.DOUBLE:
				EnsureType (val, typeof(Double));
				break;
			case MsgpackType.BYTE:
				EnsureSafe (val, typeof(byte));
				break;
			case MsgpackType.UINT16:
				EnsureSafe (val, typeof(UInt16));
				break;
			case MsgpackType.UINT32:
				EnsureSafe (val, typeof(UInt32));
				break;
			case MsgpackType.UINT64:
				EnsureSafe (val, typeof(UInt64));
				break;
			case MsgpackType.INT8:
				EnsureSafe (val, typeof(sbyte));
				break;
			case MsgpackType.INT16:
				EnsureSafe (val, typeof(Int16));
				break;
			case MsgpackType.INT32:
				EnsureSafe (val, typeof(Int32));
				break;
			case MsgpackType.INT64:
				EnsureSafe (val, typeof(Int64));
				break;
			case MsgpackType.NEGFIXINT:
				i = (int)val;
				if (i < -32) {
					throw new ArgumentOutOfRangeException("val", i, "Negative fixints can't be less than -32");
				}
				if (i > 0) {
					throw new ArgumentOutOfRangeException("val", i, "Negative fixints can't be positive");
				}
				break;
			}
		}

		public void WriteTo (MsgpackWriter o)
		{
			switch (Type) {
			case MsgpackType.POSFIXINT:
				o.WritePosFixInt((byte)Value);
				break;
			case MsgpackType.MAP32:
			case MsgpackType.MAP16:
			case MsgpackType.FIXMAP:
				o.WriteDictionary((PackedMap)Value);
				break;
			case MsgpackType.ARRAY32:
			case MsgpackType.ARRAY16:
			case MsgpackType.FIXARRAY:
				o.WriteArray((PackedList)Value);
				break;
			case MsgpackType.STRING32:
			case MsgpackType.STRING16:
			case MsgpackType.STRING8:
			case MsgpackType.FIXSTR:
				o.WriteString((string)Value);
				break;
			case MsgpackType.NIL:
				o.WriteNil();
				break;
			case MsgpackType.FALSE:
			case MsgpackType.TRUE:
				o.WriteBool((bool)Value);
				break;
			case MsgpackType.BIN32:
			case MsgpackType.BIN16:
			case MsgpackType.BIN8:
				o.WriteBytes((byte[])Value);
				break;
			case MsgpackType.EXT32:
			case MsgpackType.EXT16:
			case MsgpackType.EXT8:
			case MsgpackType.FIXEXT1:
			case MsgpackType.FIXEXT2:
			case MsgpackType.FIXEXT4:
			case MsgpackType.FIXEXT8:
			case MsgpackType.FIXEXT16:
				((IMarshalable)Value).WriteTo(o);
				break;
			case MsgpackType.SINGLE:
				o.WriteSingle((Single)Value);
				break;
			case MsgpackType.DOUBLE:
				o.WriteDouble((Double)Value);
				break;
			case MsgpackType.BYTE:
				o.WriteByte((byte)Value);
				break;
			case MsgpackType.UINT16:
				o.WriteUInt16((UInt16)Value);
				break;
			case MsgpackType.UINT32:
				o.WriteUInt32((UInt32)Value);
				break;
			case MsgpackType.UINT64:
				o.WriteUInt64((UInt64)Value);
				break;
			case MsgpackType.INT8:
				o.WriteInt8((sbyte)Value);
				break;
			case MsgpackType.INT16:
				o.WriteInt16((Int16)Value);
				break;
			case MsgpackType.INT32:
				o.WriteInt32((Int32)Value);
				break;
			case MsgpackType.INT64:
				o.WriteInt64((Int64)Value);
				break;
			case MsgpackType.NEGFIXINT:
				o.WriteNegFixInt((sbyte)Value);
				break;
			}
		}

		public void ReadFrom(MsgpackReader r)
		{
			MsgpackType type = r.PeekType ();
			switch (type) {
			case MsgpackType.POSFIXINT:
				Value = r.ReadPosFixInt ();
				break;
			case MsgpackType.MAP32:
			case MsgpackType.MAP16:
			case MsgpackType.FIXMAP:
				Value = new PackedMap(r);
				break;
			case MsgpackType.ARRAY32:
			case MsgpackType.ARRAY16:
			case MsgpackType.FIXARRAY:
				Value = new PackedList(r);
				break;
			case MsgpackType.STRING32:
			case MsgpackType.STRING16:
			case MsgpackType.STRING8:
			case MsgpackType.FIXSTR:
				Value = r.ReadString ();
				break;
			case MsgpackType.NIL:
				Value = null;
				break;
			case MsgpackType.FALSE:
			case MsgpackType.TRUE:
				Value = r.ReadBool ();
				break;
			case MsgpackType.BIN32:
			case MsgpackType.BIN16:
			case MsgpackType.BIN8:
				Value = r.ReadBytes ();
				break;
			case MsgpackType.EXT32:
			case MsgpackType.EXT16:
			case MsgpackType.EXT8:
			case MsgpackType.FIXEXT16:
			case MsgpackType.FIXEXT8:
			case MsgpackType.FIXEXT4:
			case MsgpackType.FIXEXT2:
			case MsgpackType.FIXEXT1:
				throw new Exception ("Not implemented");
			case MsgpackType.SINGLE:
				Value = r.ReadSingle ();
				break;
			case MsgpackType.DOUBLE:
				Value = r.ReadDouble ();
				break;
			case MsgpackType.BYTE:
				Value = r.ReadByte ();
				break;
			case MsgpackType.UINT16:
				Value = r.ReadUInt16 ();
				break;
			case MsgpackType.UINT32:
				Value = r.ReadUInt32 ();
				break;
			case MsgpackType.UINT64:
				Value = r.ReadUInt64 ();
				break;
			case MsgpackType.INT8:
				Value = r.ReadInt8 ();
				break;
			case MsgpackType.INT16:
				Value = r.ReadInt16 ();
				break;
			case MsgpackType.INT32:
				Value = r.ReadInt32 ();
				break;
			case MsgpackType.INT64:
				Value = r.ReadInt64 ();
				break;
			case MsgpackType.NEGFIXINT:
				Value = r.ReadNegFixInt ();
				break;
			default:
				throw new InvalidDataException ("Unable to determine an appropriate type");
			}
			Type = type;
		}
	}

	public class PackedMap : Dictionary<EncapsulatedValue, EncapsulatedValue>, IMarshalable
	{
		internal PackedMap(MsgpackReader r) {
			this.ReadFrom(r);
		}

		public void WriteTo (MsgpackWriter tc)
		{
			// we could theoretically do the writeout here, but it seems bvetter to keep it symetrical with the read
			tc.WriteDictionary(this);
		}

		public void ReadFrom (MsgpackReader tc)
		{
			// Since msgpack ocasionally encodes the length of the dictionary in the type byte, we can't actually do
			// the parsing here
			tc.ReadDictionary(this);
		}
	}

	public class PackedList : IMarshalable
	{
		public EncapsulatedValue[] Values {
			get;
			set;
		}

		public int Length {
			get { return Values.Length; }
		}

		/// Internal constructor for reading from a transcoder
		internal PackedList (MsgpackReader client)
		{
			client.ReadArray(this);
		}

		public void WriteTo (MsgpackWriter tc)
		{
			// we could theoretically do the writeout here, but it seems bvetter to keep it symetrical with the read
			tc.WriteArray(this);
		}

		public void ReadFrom (MsgpackReader tc)
		{
			// Since msgpack ocasionally encodes the length of the dictionary in the type byte, we can't actually do
			// the parsing here
			tc.ReadArray(this);
		}
	}
}
