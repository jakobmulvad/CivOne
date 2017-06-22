// CivOne
//
// To the extent possible under law, the person who associated CC0 with
// CivOne has waived all copyright and related or neighboring rights
// to CivOne.
//
// You should have received a copy of the CC0 legalcode along with this
// work. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.

using System;
using System.Linq;
using CivOne.Advances;
using CivOne.Buildings;
using CivOne.Enums;
using CivOne.Governments;
using CivOne.Interfaces;
using CivOne.Tasks;
using CivOne.Templates;
using CivOne.Tiles;
using CivOne.Units;

using Democratic = CivOne.Governments.Democracy;

namespace CivOne
{
	internal class AIExperimental : BaseInstance
	{
		internal static double getThreatLevelAt(int x, int y, byte playerNumber) {

			if (Game.GetUnits(x, y).Any(u => u.Owner != playerNumber)) {
				// Enemy unit occupies this tile - INFINITE THREAT!
				return Double.PositiveInfinity;
			}

			return Game.GetUnits()
				.Where(u => u.Owner != playerNumber && Common.DistanceToTile(x, y, u.X, u.Y) < 20)
				.Select(u => u.Attack / Common.TrueDistanceToTile(x, y, u.X, u.Y))
				.Sum();
		}

		internal static void Move(IUnit unit)
		{
			Player player = Game.GetPlayer(unit.Owner);

			if (unit is Settlers)
			{
				ITile tile = unit.Tile;

				bool hasCity = (tile.City != null);
				bool validCity = (tile is Grassland || tile is River || tile is Plains) && (tile.City == null);
				bool validIrrigaton = (tile is Grassland || tile is River || tile is Plains || tile is Desert) && (tile.City == null) && (!tile.Mine) && (!tile.Irrigation) && tile.Cross().Any(x => x.IsOcean || x is River || x.Irrigation);
				bool validMine = (tile is Mountains || tile is Hills) && (tile.City == null) && (!tile.Mine) && (!tile.Irrigation);
				bool validRoad = (tile.City == null) && tile.Road;
				int nearestCity = 255;
				int nearestOwnCity = 255;
				double threatLevel = getThreatLevelAt(unit.X, unit.Y, unit.Owner);

				if (threatLevel > 2.0d) {
					Console.WriteLine($"AI: A settler of {player.LeaderName} feels threatened ({threatLevel})");
					var safeTiles = tile.GetBorderTiles()
						.Where(x => !x.IsOcean)
						.OrderBy(x => getThreatLevelAt(x.X, x.Y, unit.Owner));
					
					if (safeTiles.Count() > 0) {
						unit.MoveTo(safeTiles.First().X, safeTiles.First().Y);
						return;
					}
				}

				if (Game.GetCities().Any()) nearestCity = Game.GetCities().Min(x => Common.DistanceToTile(x.X, x.Y, tile.X, tile.Y));
				if (Game.GetCities().Any(x => x.Owner == unit.Owner)) nearestOwnCity = Game.GetCities().Where(x => x.Owner == unit.Owner).Min(x => Common.DistanceToTile(x.X, x.Y, tile.X, tile.Y));
				
				if (validCity && nearestCity > 3)
				{
					GameTask.Enqueue(Orders.FoundCity(unit as Settlers));
					return;
				}
				else
				{
					var distanceToCityTiles = tile.GetBorderTiles()
						.Where(x => !x.IsOcean)
						.OrderBy(x => Game.GetCities().Min(c => Common.DistanceToTile(c.X, c.Y, x.X, x.Y)));

					if (distanceToCityTiles.Count() > 0) {
						var distanceTile = distanceToCityTiles.First();

						if (distanceTile != tile) {
							unit.MoveTo(distanceToCityTiles.First().X, distanceToCityTiles.First().Y);
							return;
						}
					}

					/*switch (Common.Random.Next(5 * nearestOwnCity))
					{
						case 0:
							if (validRoad)
							{
								GameTask.Enqueue(Orders.BuildRoad(unit));
								return;
							}
							break;
						case 1:
							if (validIrrigaton)
							{
								GameTask.Enqueue(Orders.BuildIrrigation(unit));
								return;
							}
							break;
						case 2:
							if (validMine)
							{
								GameTask.Enqueue(Orders.BuildMines(unit));
								return;
							}
							break;
					}*/
				}

				for (int i = 0; i < 1000; i++)
				{
					int relX = Common.Random.Next(-1, 2);
					int relY = Common.Random.Next(-1, 2);
					if (relX == 0 && relY == 0) continue;
					if (unit.Tile[relX, relY] is Ocean) continue;
					unit.MoveTo(relX, relY);
					return;
				}
				unit.SkipTurn();
				return;
			}
			else if (unit is Militia || unit is Phalanx || unit is Musketeers || unit is Riflemen || unit is MechInf)
			{
				unit.Fortify = true;
				while (unit.Tile.City != null && unit.Tile.Units.Count(x => x is Militia || x is Phalanx || x is Musketeers || x is Riflemen || x is MechInf) > 2)
				{
					IUnit disband = null;
					IUnit[] units = unit.Tile.Units.Where(x => x != unit).ToArray();
					if ((disband = unit.Tile.Units.FirstOrDefault(x => x is Militia)) != null) { Game.DisbandUnit(disband); continue; }
					if ((disband = unit.Tile.Units.FirstOrDefault(x => x is Phalanx)) != null) { Game.DisbandUnit(disband); continue; }
					if ((disband = unit.Tile.Units.FirstOrDefault(x => x is Musketeers)) != null) { Game.DisbandUnit(disband); continue; }
					if ((disband = unit.Tile.Units.FirstOrDefault(x => x is Riflemen)) != null) { Game.DisbandUnit(disband); continue; }
					if ((disband = unit.Tile.Units.FirstOrDefault(x => x is MechInf)) != null) { Game.DisbandUnit(disband); continue; }
				}
			}
			else
			{
				if (unit.Class != UnitClass.Land) Game.DisbandUnit(unit);

				for (int i = 0; i < 1000; i++)
				{
					if (unit.GotoX == -1 || unit.GotoY == -1)
					{
						int gotoX = Common.Random.Next(-5, 6);
						int gotoY = Common.Random.Next(-5, 6);
						if (gotoX == 0 && gotoY == 0) continue;
						if (!player.Visible(unit.X + gotoX, unit.Y + gotoY)) continue;

						unit.GotoX = unit.X + gotoX;
						unit.GotoY = unit.Y + gotoY;
						continue;
					}

					if (unit.GotoX != -1 && unit.GotoY != -1)
					{
						int distance = unit.Tile.DistanceTo(unit.GotoX, unit.GotoY);
						ITile[] tiles = (unit as BaseUnit).MoveTargets.OrderBy(x => x.DistanceTo(unit.GotoX, unit.GotoY)).ThenBy(x => x.Movement).ToArray();
						if (tiles.Length == 0 || tiles[0].DistanceTo(unit.GotoX, unit.GotoY) > distance)
						{
							// No valid tile to move to, cancel goto
							unit.GotoX = -1;
							unit.GotoY = -1;
							continue;
						}
						else if (tiles[0].DistanceTo(unit.GotoX, unit.GotoY) == distance)
						{
							// Distance is unchanged, 50% chance to cancel goto
							if (Common.Random.Next(0, 100) < 50)
							{
								unit.GotoX = -1;
								unit.GotoY = -1;
								continue;
							}
						}

						if (!unit.MoveTo(tiles[0].X - unit.X, tiles[0].Y - unit.Y))
						{
							// The code below is to prevent the game from becoming stuck...
							if (Common.Random.Next(0, 100) < 67)
							{
								unit.GotoX = -1;
								unit.GotoY = -1;
								continue;
							}
							else if (Common.Random.Next(0, 100) < 67)
							{
								unit.SkipTurn();
								return;
							}
							else
							{
								Game.DisbandUnit(unit);
								return;
							}
						}
						return;
					}
				}
			}
			unit.SkipTurn();
		}
		internal static void CityProduction(City city)
		{
			if (city == null || city.Size == 0 || city.Tile == null) return;

			Player player = Game.GetPlayer(city.Owner);
			IProduction production = null;

			// Set random production
			if (production == null)
			{
				IProduction[] items = city.AvailableProduction.ToArray();
				production = items[Common.Random.Next(items.Length)];
			}

			city.SetProduction(production);
		}
	}
}