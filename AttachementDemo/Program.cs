using System;
using System.Net.Sockets;
using Mono.Unix;
using Neovim.Msgpack;

namespace AttachementDemo
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			for (int i = 0; i < 256; ++i) {
				Console.WriteLine ("{0} {1}", i, (MsgpackType)i);
			}
			Console.WriteLine ("To get the address of a running nvim process, run '!echo $NVIM_LISTEN_ADDRESS'");
			Console.Write ("Please enter that here: ");
			//string endpoint = Console.ReadLine ();

//			var o = c.Call ("vim_get_api_info");
//
//			Console.Write (o);

//			c.Shutdown ();
		}
	}
}