namespace SudokuSolver.GameParts
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;

	public class AlmostLockedSet
    {
        public List<Cell> Cells;
        public int PmsInSet;
        public int GroupDimension;
        public int Coordinate;
        public int CoordIndex;  // row, column, or section index
        public Int64[] CellMap;

        public const int Int64BitSize = 64;

        private AlmostLockedSet()
        {
        }

        public static ALSMatch AreAlsXZ(AlmostLockedSet als1, AlmostLockedSet als2)
        {
            int len = als1.CellMap.Length;
            for (int i = 0; i < len; i++)
                if ((als1.CellMap[i] & als2.CellMap[i]) != 0)
                    return null;

            if (!als1.CanSee(als2))
                return null;

            // Set A:  N cells with (N + 1) candidates
            // Set B:  M cells with (M + 1) candidates
            // With 1 common restricted pencilmark, and 1 uncommon restricted pencilmark,
            // the total number of pencilmarks in an ALS-XZ pattern is M + N, and the
            // number of common ones must be 2.
            int commonPMsSignature = als1.PmsInSet & als2.PmsInSet;
            if (commonPMsSignature.NumberOfBitsSet() < 2)
                return null;

            List<int> commonPMCandidates = commonPMsSignature.ListOfBits();

            Func<List<Cell>, int, List<Cell>> fnCellsWithPM = (cellList, pm) => cellList
                .Where(cell => cell.IsPencilmarkSet(pm))
                .ToList();

            Cell unShadowed = null;
            int restrictedCommonPM = 0;

            List<Cell> commonCells1;
            List<Cell> commonCells2;
            // find out if one of the common pencilmarks can see all the ones in the other group.
            foreach (int pm in commonPMCandidates)
            {
                commonCells1 = fnCellsWithPM(als1.Cells, pm);
                commonCells2 = fnCellsWithPM(als2.Cells, pm);

                unShadowed = commonCells1.FirstOrDefault(cell1 =>
                    commonCells2.FirstOrDefault(cell2 => !cell1.CanSee(cell2)) != null);

                if (unShadowed == null)
                {
                    restrictedCommonPM = pm;
                    break;
                }
            }

            if (restrictedCommonPM == 0)
                return null;

            commonPMCandidates.Remove(restrictedCommonPM);
            foreach (int pmc in commonPMCandidates)
            {
                // if the commonPMs have shadowed cells with the pencilmark, then it's a good
                // ALS pattern
                commonCells1 = fnCellsWithPM(als1.Cells, pmc);
                commonCells2 = fnCellsWithPM(als2.Cells, pmc);

                IEnumerable<Cell> intersectAll = commonCells1
                    .First()
                    .ShadowedCells
                    .Where(cell => cell.IsPencilmarkSet(pmc));

                if (!intersectAll.Any())
                    continue;

                Action<List<Cell>, int> cumulativeIntersect = (cellList, skipCount) =>
                            cellList
                            .Skip(skipCount)
                            .Select(cell => cell.ShadowedCells.Where(cell2 => cell2.IsPencilmarkSet(pmc)))
                            .ToList()
                            .ForEach(singleList => intersectAll = intersectAll.Intersect(singleList));

                cumulativeIntersect(commonCells1, 1);
                if (intersectAll.Any())
                    cumulativeIntersect(commonCells2, 0);

                // this if is not a bogus if with negated condition of the previous one, since
                // cumulativeIntersect will change the value of intersectAll
                if (!intersectAll.Any())
                    continue;

                ALSMatch returnValue = new ALSMatch()
                {
                    Als1 = als1,
                    Als2 = als2,
                    commonPencilmark = pmc,
                    commonRestrictedPencilmark = restrictedCommonPM,
                    AffectedCells = intersectAll.ToList()
                };

                return returnValue;
            }

            return null;
        }

        /// <summary>
        /// 2 sections can only see each other if the row or column of the chute they are in
        /// is the same.
        /// </summary>
        /// <param name="als2"></param>
        /// <returns></returns>
        private bool CanSee(AlmostLockedSet als2)
        {
            bool returnValue = Coordinate != Coord.SECTION || als2.Coordinate != Coord.SECTION ||
                (CoordIndex / GroupDimension) == (als2.CoordIndex / GroupDimension) ||
                (CoordIndex % GroupDimension) == (als2.CoordIndex % GroupDimension);

            return returnValue;
        }

        /// <summary>
        /// Searches for all the ALS in a line.
        /// </summary>
        /// <param name="line">Line of cells.  Can be a row, a column, or a section</param>
        /// <param name="coord">Coordinate that indicates the type of line.</param>
        /// <returns></returns>
        public static List<AlmostLockedSet> CreateALSsFromLine(List<Cell> line, int coord)
        {
            int indexCoord = line[0].GetCoordinate(coord);
            List<int> indexes = line
                .Select((v, i) => new { Cell = v, Index = i })
                .Where(t => t.Cell.PMSignature != 0)
                .Select(t => t.Index)
                .ToList();

            var combinations = new List<List<int>>();
            Combinations(combinations, indexes);
            List<AlmostLockedSet> returnValue = combinations
                .Select(indexList => indexList.Select(index => line[index]))
                .Select(cellList => ALSFromCellList(cellList, line.Count, coord, indexCoord))
                .Where(als => als != null)
                .ToList();

            return returnValue;
        }

        /// <summary>
        /// Very simple method to create all the combinations (order does not matter)
        /// for a list of integers.
        /// </summary>
        /// <param name="combinations">List of integers.  Passed by reference to be
        /// loaded with the combination values.</param>
        /// <param name="indexes">Numbers to combine</param>
        private static void Combinations(List<List<int>> combinations, List<int> indexes)
        {
            if (indexes.Count == 0)
                return;

            int head = indexes.Last();
            indexes.RemoveAt(indexes.Count - 1);
            Combinations(combinations, indexes);
            var partial = new List<List<int>>();
            combinations.ForEach(combList => partial.Add(new List<int>(combList)));
            partial.ForEach(pList => pList.Add(head));
            combinations.AddRange(partial);
            combinations.Add(new List<int> { head });
        }

        /// <summary>
        /// Creates an AlmostLockedSet object from a list of cells.
        /// </summary>
        /// <param name="cellCollection">List of cells making the ALS</param>
        /// <param name="dimension">Size of a line or column or group.</param>
        /// <param name="coordinate">Indicates the type of housing for the ALS</param>
        /// <param name="index">Index of the coordinate (e.g. index of the row, or 
        /// index of the column, or index of the group).</param>
        /// <returns></returns>
        private static AlmostLockedSet ALSFromCellList(IEnumerable<Cell> cellCollection, int dimension,
            int coordinate, int index)
        {
            List<int> perfectSquares = new List<int> { 0, 1, 4, 9, 16, 25 };
            int dimSquared = dimension * dimension;
            int intsNeeded = (dimSquared / Int64BitSize) + (((dimSquared % Int64BitSize) == 0) ? 0 : 1);
            int pmMask = 0;
            Int64[] cellMap = new Int64[intsNeeded];
            List<Cell> cellList = cellCollection
                .ToList();

            int groupSize = perfectSquares.IndexOf(dimension);
            cellList.ForEach(cell => 
                {
                    pmMask |= cell.PMSignature;
                    int cellPos = cell.RowIndex * dimension + cell.ColIndex;
                    int mapIndex = cellPos / Int64BitSize;
                    int bitIndex = cellPos % Int64BitSize;

#pragma warning disable 0675
					cellMap[mapIndex] |= (long)((ulong)(1 << bitIndex));
#pragma warning restore 0675
				});

            AlmostLockedSet returnValue = (pmMask.NumberOfBitsSet() == cellList.Count + 1) ?
                new AlmostLockedSet()
                    {
                        Cells = cellList,
                        PmsInSet = pmMask,
                        GroupDimension = groupSize,
                        Coordinate = coordinate,
                        CoordIndex = index,
                        CellMap = cellMap
                    }
                    :
                null;

            return returnValue;
        }
    }
}
