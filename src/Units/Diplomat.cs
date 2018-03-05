// CivOne
//
// To the extent possible under law, the person who associated CC0 with
// CivOne has waived all copyright and related or neighboring rights
// to CivOne.
//
// You should have received a copy of the CC0 legalcode along with this
// work. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.

using CivOne.Advances;
using CivOne.Enums;
using CivOne.Tasks;
using CivOne.Tiles;

namespace CivOne.Units
{
	internal class Diplomat : BaseUnitLand
	{
		protected override bool Confront(int relX, int relY)
		{
			ITile moveTarget = Map[X, Y][relX, relY];

			if (moveTarget.City != null)
			{
					GameTask.Enqueue(Show.DiplomatCity(moveTarget.City, this));
					return true;
			}

			IUnit[] units = moveTarget.Units;

			if (units.Length == 1)
			{
				IUnit unit = units[0];

				if (unit.Owner != Owner && unit is BaseUnitLand)
				{
					GameTask.Enqueue(Show.DiplomatBribe(unit as BaseUnitLand, this));
					return true;
				}
			}

			Movement = new MoveUnit(relX, relY);
			Movement.Done += MoveEnd;
			GameTask.Insert(Movement);
			return true;
		}

		internal void KeepMoving(IUnit unit)
		{
			Movement = new MoveUnit(unit.X - X, unit.Y - Y);
			Movement.Done += MoveEnd;
			GameTask.Enqueue(Movement);
		}
		
		public Diplomat() : base(3, 0, 0, 2)
		{
			Type = UnitType.Diplomat;
			Name = "Diplomat";
			RequiredTech = new Writing();
			ObsoleteTech = null;
			SetIcon('C', 1, 0);
		}
	}
}