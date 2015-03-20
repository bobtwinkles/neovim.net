using System;
using System.IO;

namespace Neovim.Msgpack
{
	public interface IMarshalable
	{
		void WriteTo (Transcoder s);

		void ReadFrom (Transcoder s);
	}
}

