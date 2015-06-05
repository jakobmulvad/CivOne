// CivOne
//
// To the extent possible under law, the person who associated CC0 with
// CivOne has waived all copyright and related or neighboring rights
// to CivOne.
//
// You should have received a copy of the CC0 legalcode along with this
// work. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.

using CivOne.Enums;
using CivOne.Interfaces;
using CivOne.Templates;

namespace CivOne.Units
{
	internal class MechInf : BaseUnit
	{
		public MechInf() : base(5, 6, 6, 3)
		{
			Class = UnitClass.Land;
			Type = Unit.MechInf;
			Name = "Mech. Inf.";
			RequiredTech = null;
			ObsoleteTech = null;
			SetIcon('C', 0, 0);
		}
	}
}