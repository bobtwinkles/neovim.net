using System;
using YamlDotNet.Core;

namespace Neovim.MsgPackRPC.Types
{
	/// <summary>
	//// Represents a remote vimscript map.
	/// </summary>
	public class RemoteMap
	{
		private string _getMethod;
		private string _setMethod;

		/// <summary>
		/// Create a reference to a remote vimscript map
		/// <param name="getMethod">The name of a msgpack-rpc call which will return the value of a key of the remote dictionary</param>
		/// <param name="setMethod">The name of a msgpack-rpc call which will set a key in the remote dictionary</param>"> 
		/// </summary>
		public RemoteMap (string getMethod, string setMethod)
		{
			if (getMethod == null) {
				throw new ArgumentNullException ("getMethod");
			}
			this._getMethod = getMethod;
			this._setMethod = setMethod;
		}

		public string this [string key] {
			get {
				return "TODO";
			}
			set {
				if (this._setMethod == null) {
					throw new InvalidOperationException ("Dictionary does not have a setter, so set is nonsensical");
				}
			}
		}
	}
}

