using Neovim.MsgPackRPC.Types;
using MsgPack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neovim.MsgPackRPC
{
	public static class RPCUtils
	{
		public static void Parse (MessagePackObject obj, out int[] data)
		{
			data = new int[0];
		}

		public static void Parse (MessagePackObject obj, out string[] data)
		{
			data = new string[0];
		}

		public static void Parse (MessagePackObject obj, out int i)
		{
			i = (int)obj;
		}

		public static void Parse (MessagePackObject obj, out bool i)
		{
			i = (bool)obj;
		}

		public static void Parse (MessagePackObject obj, out string data)
		{
			data = "";
		}

		public static void Parse (MessagePackObject obj, out Dictionary<string, string> data)
		{
			data = null;
		}

		public static void Parse (MessagePackObject obj, out Buffer data)
		{
			data = new Buffer ();
		}

		public static void Parse (MessagePackObject obj, out Buffer[] data)
		{
			data = new Buffer[0];
		}

		public static void Parse (MessagePackObject obj, out Tabpage data)
		{
			data = new Tabpage ();
		}

		public static void Parse (MessagePackObject obj, out Tabpage[] data)
		{
			data = new Tabpage[0];
		}

		public static void Parse (MessagePackObject obj, out Window data)
		{
			data = new Window ();
		}

		public static void Parse (MessagePackObject obj, out Window[] data)
		{
			data = new Window[0];
		}

		public static void Parse (Task<MessagePackObject> obj, out Task data)
		{
			data = (Task)obj;
		}

		public static void Parse (Task<MessagePackObject> obj, out Task<string> data)
		{
			data = obj.ContinueWith ((res) => {
				return "science!";
			});
		}
	}
}

