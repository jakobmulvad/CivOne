// CivOne
//
// To the extent possible under law, the person who associated CC0 with
// CivOne has waived all copyright and related or neighboring rights
// to CivOne.
//
// You should have received a copy of the CC0 legalcode along with this
// work. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.

using CivOne.Graphics;

namespace CivOne.Advances
{
	public interface IAdvance : ICivilopedia
	{
		byte Id { get; }
		IAdvance[] RequiredTechs { get; }
		Palette OriginalColours { get; }
		bool Requires(byte id);
		bool Is<T>() where T : IAdvance;
		bool Not<T>() where T : IAdvance;
	}
}