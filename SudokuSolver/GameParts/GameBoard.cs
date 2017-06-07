namespace SudokuSolver.GameParts
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Exceptions;

	public class GameBoard
    {
        public int Dimension { get; private set; }
        public int BlockDimension { get; private set; }
        public bool EditMode { get; set; }

        private Cell[,] CellMatrix;
        private List<List<Cell>> Rows;
        private List<List<Cell>> Columns;
        private List<List<Cell>> Groups;

		public int GroupIndex(int row, int col) => (row / BlockDimension) * BlockDimension + (col / BlockDimension);
		public Cell GetCell(int row, int col) => CellMatrix[row, col];

		public GameBoard(int dimension = 9)
        {
            BlockDimension = (int)Math.Sqrt(dimension);
            if (Math.Pow(BlockDimension, 2) != dimension)
                throw new InvalidBoardSizeException("Invalid board size.");

            Dimension = dimension;
            CellMatrix = new Cell[dimension, dimension];
            Rows = new List<List<Cell>>();
            Columns = new List<List<Cell>>();
            Groups = new List<List<Cell>>();
            for (int i = 0; i < dimension; i++)
            {
                Rows.Add(new List<Cell>());
                for (int j = 0; j < dimension; j++)
                {
                    if (i == 0)
                    {
                        Columns.Add(new List<Cell>());
                        Groups.Add(new List<Cell>());
                    }

                    var cell = new Cell(this, i, j);
                    CellMatrix[i, j] = cell;
                    Rows[i].Add(cell);
                    Columns[j].Add(cell);
                    int groupIndex = GroupIndex(i, j);
                    Groups[groupIndex].Add(cell);
                }
            }
        }

        public GameBoard(int dimension, string puzzleData) : this(dimension)
        {
            int r = 0;
            int c = 0;
            var sb = new StringBuilder();
            foreach (char ch in puzzleData)
                if ((ch >= '1' && ch <= '9') || ch == 'x')
                    sb.Append(ch);

            puzzleData = sb.ToString();
            foreach (char ch in puzzleData)
            {
                if (ch != 'x')
                {
                    int cellValue = (int)(ch - '0');
                    SetCellValueAndAffectShadowedCells(r, c, cellValue, false/*mutable*/);
                }

                c = (c + 1) % dimension;
                r += (c == 0) ? 1 : 0;
            }
        }


        public List<Cell> GetGroup(int index) { return Groups[index]; }
        public List<Cell> GetGroup(int row, int col) { return Groups[GroupIndex(row, col)]; }
        public List<List<Cell>> GetAllGroups() { return Groups; }

        public List<List<Cell>> GetAllLines(int coord)
        {
            List<List<Cell>> returnValue = null;
            switch (coord)
            {
                case Coord.ROWS: returnValue = Rows; break;
                case Coord.COLUMNS: returnValue = Columns; break;
                case Coord.SECTION: returnValue = Groups; break;
                default:
                    throw new ArgumentException("Invalid argument 'coord'.  Must be 0, 1, or 2");
            }

            return returnValue;
        }

        public List<Cell> GetLine(int coord, int index) 
        { 
            List<Cell> returnValue = null;
            switch (coord)
            {
                case Coord.ROW: returnValue = Rows[index]; break;
                case Coord.COL: returnValue = Columns[index]; break;
                case Coord.SECTIONS: returnValue = Groups[index]; break;
                default:
                    throw new ArgumentException("Parameter coord is invalid.");
            }

            return returnValue;
        }

        public void SetCellValueAndAffectShadowedCells(int row, int col, int value, bool mutable)
        {
            Cell cell = CellMatrix[row, col];
            cell.SetValue(value, mutable);
            cell.ShadowedCells.ForEach(c => c.SetPencilMark(value, false/*pencilMarkActive*/));
        }

        public int GetEmptyCellCount()
        {
            int returnValue = GetAllLines(Coord.ROWS)
                .SelectMany(t => t)
                .Count(cell => cell.Value == 0);

            return returnValue;
        }

        public bool HasColorizedCells()
        {
            Cell firstColorizedCell = GetAllLines(Coord.ROWS)
                .SelectMany(t => t)
                .FirstOrDefault(t => t.IsAnyPencilmarkColorized);

            return firstColorizedCell != null;
        }

        public void ClearPMColorization()
        {
            GetAllLines(Coord.ROWS)
                .SelectMany(t => t)
                .ToList()
                .ForEach(cell => cell.ClearAllPencilmarkColors());
        }

        public List<Cell> GetAllColorizedCells()
        {
            List<Cell> returnValue = GetAllLines(Coord.ROWS)
                 .SelectMany(t => t)
                 .Where(t => t.IsAnyPencilmarkColorized)
                 .ToList();

            return returnValue;
        }

        public void RecalculatePencilMarks()
        {
            int mask = Mask.ForSettingPencilMarks(Dimension);

            // clear all the pencilmarks
            List<Cell> allCells = GetAllLines(Coord.ROWS)
                .SelectMany(t => t)
                .ToList();

            allCells.ForEach(t => t.SetMultiplePencilMarks(mask));

            // and set the values again
            foreach (Cell cell in allCells)
                if (cell.Value != 0)
                {
                    SetCellValueAndAffectShadowedCells(cell.RowIndex, cell.ColIndex,
                        cell.Value, false/*mutable*/);
                }
        }

        public void ClearAllCells()
        {
            List<Cell> allCells = GetAllLines(Coord.ROWS)
                .SelectMany(t => t)
                .ToList();

            allCells.ForEach(cell => cell.ClearCellValue());
            RecalculatePencilMarks();
        }
    }
}
