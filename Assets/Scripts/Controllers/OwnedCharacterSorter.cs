using System;
using System.Collections.Generic;
using DeckBuilding.Data;

namespace DeckBuilding.Controllers
{
    public static class OwnedCharacterSorter
    {
        public static List<OwnedCharacter> Sort(IReadOnlyList<OwnedCharacter> source, SortMode mode)
        {
            var result = new List<OwnedCharacter>(source);

            Comparison<OwnedCharacter> comparison = mode switch
            {
                SortMode.CostOrder => (a, b) => a.Master.Cost.CompareTo(b.Master.Cost),
                SortMode.HpOrder => (a, b) => a.Master.Hp.CompareTo(b.Master.Hp),
                _ => (a, b) => a.AcquisitionOrder.CompareTo(b.AcquisitionOrder),
            };

            result.Sort(comparison);
            return result;
        }

        public static SortMode Next(SortMode current)
        {
            int modeCount = Enum.GetValues(typeof(SortMode)).Length;
            return (SortMode)(((int)current + 1) % modeCount);
        }

        public static string ToDisplayLabel(SortMode mode) => mode switch
        {
            SortMode.CostOrder => "コスト順",
            SortMode.HpOrder => "HP順",
            _ => "入手順",
        };
    }
}
