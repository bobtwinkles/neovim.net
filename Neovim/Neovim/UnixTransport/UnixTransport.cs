using MsgPack.Rpc.Client;
using MsgPack.Rpc.Client.Protocols;
using System.Net;
using System.Net.Sockets;

namespace Neovim.UnixTransport
{
	public class UnixClientTransport : ClientTransport
	{
		protected override bool CanResumeReceiving {
			get { return true; }
		}

		public UnixClientTransport (UnixTransportManager man) : base (man)
		{
		}

		protected override void ShutdownSending ()
		{
			try {
				this.BoundSocket.Shutdown (SocketShutdown.Send);
			} catch (SocketException ex) {
				if (ex.SocketErrorCode != SocketError.NotConnected) {
					throw;
				}
			}

			base.ShutdownSending ();
		}

		protected sealed override void ShutdownReceiving ()
		{
			try {
				this.BoundSocket.Shutdown (SocketShutdown.Send);
			} catch (SocketException ex) {
				if (ex.SocketErrorCode != SocketError.NotConnected) {
					throw;
				}
			}

			base.ShutdownReceiving ();
		}

		protected sealed override void SendCore (ClientRequestContext context)
		{
			if (!this.BoundSocket.SendAsync (context.SocketContext)) {
				context.SetCompletedSynchronously ();
				this.OnSent (context);
			}
		}

		protected sealed override void ReceiveCore (ClientResponseContext context)
		{
			if (!this.BoundSocket.ReceiveAsync (context.SocketContext)) {
				context.SetCompletedSynchronously ();
				this.OnReceived (context);
			}
		}

	}
}

