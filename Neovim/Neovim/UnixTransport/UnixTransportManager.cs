using MsgPack.Rpc;
using MsgPack.Rpc.Client;
using MsgPack.Rpc.Client.Protocols;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Neovim.UnixTransport
{
	public class UnixTransportManager : ClientTransportManager<UnixClientTransport>
	{
		static ObjectPool<UnixClientTransport> _transportPool = null;

		public UnixTransportManager (RpcClientConfiguration config) : base (config)
		{
			if (_transportPool == null) { 
				_transportPool = new OnTheFlyObjectPool<UnixClientTransport> ((ObjectPoolConfiguration cfg) => new UnixClientTransport (this), config.CreateTransportPoolConfiguration ());
			}
			base.SetTransportPool (_transportPool);
		}

		protected sealed override Task<ClientTransport> ConnectAsyncCore (EndPoint ep)
		{
			TaskCompletionSource<ClientTransport> source = new TaskCompletionSource<ClientTransport> ();
			var context = new SocketAsyncEventArgs ();
			context.RemoteEndPoint = ep;
			context.Completed += this.OnCompleted;

			var connectingSocket = new Socket (SocketType.Stream, ProtocolType.Tcp);//new Socket (ep.AddressFamily, SocketType.Stream, ProtocolType.Unspecified);
			context.UserToken =
				Tuple.Create (
				source,
				this.BeginConnectTimeoutWatch (
					() => {
						// Cancel ConnectAsync.
						connectingSocket.Close ();
					}
				)
					, connectingSocket
			);
			if (!connectingSocket.ConnectAsync (context)) {
				this.OnCompleted (null, context);
			}
			return source.Task;
		}

		private void OnCompleted (object sender, SocketAsyncEventArgs e)
		{

			var socket = sender as Socket;
			var userToken = e.UserToken as Tuple<TaskCompletionSource<ClientTransport>, ConnectTimeoutWatcher, Socket>;
			var taskCompletionSource = userToken.Item1;
			var watcher = userToken.Item2;
			if (watcher != null) {
				this.EndConnectTimeoutWatch (watcher);
			}
			var error = this.HandleSocketError (userToken.Item3 ?? socket, e);
			if (error != null) {
				taskCompletionSource.SetException (error.Value.ToException ());
				return;
			}
			switch (e.LastOperation) {
			case SocketAsyncOperation.Connect:
				{
					this.OnConnected (userToken.Item3, e, taskCompletionSource);
					break;
				}
			default:
				{
					taskCompletionSource.SetException (new InvalidOperationException (String.Format ("Unknown socket operation : {0}", e.LastOperation)));
					break;
				}
			}
		}

		private void OnConnected (Socket connectSocket, SocketAsyncEventArgs context, TaskCompletionSource<ClientTransport> taskCompletionSource)
		{
			try {
				if (connectSocket == null || !connectSocket.Connected) {
					// canceled.
					taskCompletionSource.SetException (
						new Exception (
							String.Format ("Timeout: {0}", this.Configuration.ConnectTimeout)
						)
					);
					return;
				}
				taskCompletionSource.SetResult (this.GetTransport (connectSocket));
			} finally {
				context.Dispose ();
			}
		}

		protected sealed override UnixClientTransport GetTransportCore (Socket bindingSocket)
		{
			if (bindingSocket == null) {
				throw new InvalidOperationException (String.Format ("'bindingSocket' is required in {0}.", this.GetType ()));
			}
			var transport = base.GetTransportCore (bindingSocket);
			this.BindSocket (transport, bindingSocket);
			return transport;
		}
	}
}

