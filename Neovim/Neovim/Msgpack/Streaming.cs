using System;
using System.Collections.Generic;

namespace Neovim.Msgpack
{
	public class EncapsulatedValue
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
				EnsureType (val, typeof(MsgPackMarshalable));
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
				break;
			}
		}
	}

	public class PackedMap : Dictionary<EncapsulatedValue, EncapsulatedValue>, MsgPackMarshalable
	{
		public void WriteTo(Transcoder tc) {
			// TODO
		}

		public void ReadFrom(Transcoder tc) {
			//TODO
		}
	}

	public class PackedList : MsgPackMarshalable
	{
		public EncapsulatedValue[] Values {
			get;
			set;
		}

		int Length {
			get { return Values.Length; }
		}

		/// Internal constructor for reading from a transcoder
		internal PackedList (Transcoder client, int length)
		{
			Values = new EncapsulatedValue[length];
			for (int i = 0; i < Length; ++i) {
				Values [i] = client.ReadEncapsulated();
			}
		}

		public void WriteTo(Transcoder tc) {
			// TODO
		}

		public void ReadFrom(Transcoder tc) {
			//TODO
		}
	}
}
