using System;
using System.Net.Sockets;
using Neovim.UnixTransport;
using Mono.Unix;
using MsgPack.Rpc;
using MsgPack.Rpc.Client;

namespace AttachementDemo
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("To get the address of a running nvim process, run '!echo $NVIM_LISTEN_ADDRESS'");
			Console.Write ("Please enter that here: ");
			string endpoint = Console.ReadLine ();
			RpcClientConfiguration conf = new RpcClientConfiguration ();
			conf.TransportManagerProvider = (RpcClientConfiguration cfg) => new UnixTransportManager (cfg);
			RpcClient c = new RpcClient (new UnixEndPoint (endpoint), conf);

			var o = c.Call ("vim_get_api_info");

			Console.Write (o);

			c.Shutdown ();
		}
	}
}