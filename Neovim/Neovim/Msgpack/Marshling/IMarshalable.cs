// vim: noexpandtab ts=4 sts=4 sw=4 colorcolumn=120
namespace Neovim.Msgpack
{
	public interface IMarshalable
	{
		void WriteTo (MsgpackWriter s);

		void ReadFrom (MsgpackReader s);
	}
}

