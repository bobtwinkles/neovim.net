using System;
using System.IO;

namespace Neovim.Msgpack
{
	public interface MsgPackMarshalable
	{
		void WriteTo (Transcoder s);

		void ReadFrom (Transcoder s);
	}
}

