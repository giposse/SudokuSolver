namespace SudokuSolver.GameParts
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;

	public class Cell
    {
        public GameBoard Board { get; set; }
        private int _pencilMarks;

        private int _pmColors; // 3 bits per color for each pencilmark
        public const int COLOR_MASK_SIZE = 3;
        private List<int> ToBeRemovedPencilmarks;
        public int ClashedPM { get; private set; }
        public PMColor ClashedPMColor { get; private set; }

        public int Value { get; set; }
        public bool Mutable { get; set; }
        public int PencilMarkCount { get; private set; }

        public int RowIndex { get; private set; }
        public int ColIndex { get; private set; }



		public bool IsAnyPencilmarkColorized => _pmColors != 0;
		public bool HasPencilmarksMarkedForRemoval => ToBeRemovedPencilmarks.Count > 0;
		public int PMSignature => _pencilMarks;
		public int GroupIndex => Board.GroupIndex(RowIndex, ColIndex);

		public bool IsPencilmarkSet(int value) => (_pencilMarks & (1 << value)) != 0;
		public void MarkPencilmarksForRemoval(params int[] pms) => ToBeRemovedPencilmarks.AddRange(pms);

		public bool CanSee(Cell cell2) => RowIndex == cell2.RowIndex || ColIndex == cell2.ColIndex ||
				GroupIndex == cell2.GroupIndex;

		/// <summary>
		/// Constructor to be used by external callers to create individual cells.
		/// Within the assembly, no one should call this constructor.
		/// </summary>
		/// <param name="value">Value for the Cell</param>
		/// <param name="row">0-based index of the row</param>
		/// <param name="col">0-based index of the column</param>
		public Cell(int value, int row, int col, bool mutable = false)
        {
            Value = value;
            PencilMarkCount = 0;
            _pencilMarks = 0;
            RowIndex = row;
            ColIndex = col;
            Mutable = mutable;
            ToBeRemovedPencilmarks = new List<int>();
        }

        internal Cell(GameBoard board, int row, int col)
        {
            RowIndex = row;
            ColIndex = col;

            Board = board;
            Mutable = true;
            _pencilMarks = Mask.ForSettingPencilMarks(Board.Dimension);
            PencilMarkCount = Board.Dimension;
            ToBeRemovedPencilmarks = new List<int>();
        }

        internal Cell Clone()
        {
            Cell returnValue = new Cell(Value, this.RowIndex, ColIndex);
            returnValue.Mutable = Mutable;
            returnValue.Board = Board;
            returnValue._pencilMarks = _pencilMarks;
            returnValue.PencilMarkCount = PencilMarkCount;
            return returnValue;
        }


        #region Pencilmark Color handling

        public void ColorizePencilmark(int pm, PMColor color)
        {
            int shiftSize = pm * COLOR_MASK_SIZE;
            int colorBits = (int)color << shiftSize;
            int mask = ~((int)PMColor.MASK << shiftSize);
            _pmColors = (_pmColors & mask) | colorBits;
        }

        public bool ColorizePencilmarkWithClashDetection(int pm, PMColor color)
        {
            int shiftSize = pm * COLOR_MASK_SIZE;
            int mask = ((int)PMColor.MASK << shiftSize);
            PMColor prevColor = (PMColor)((_pmColors & mask) >> shiftSize);
            bool clashDetected = prevColor != PMColor.NONE && prevColor != color;
            PMColor finalColor = color;
            if (clashDetected)
            {
                // TODO: The clashed color is the original color which generated the path
                // to the contradicion, not the prevColor
                ClashedPMColor = prevColor;
                ClashedPM = pm;
            }

            int colorBits = (int)finalColor << shiftSize;
            _pmColors = (_pmColors & ~mask) | colorBits;
            return clashDetected;
        }

        public bool IsPencilmarkColorized(int pm)
        {
            int shiftSize = pm * COLOR_MASK_SIZE;
            int mask = (int)PMColor.MASK << shiftSize;
            return (_pmColors & mask) != 0;
        }

        public void ClearAllPencilmarkColors()
        {
            _pmColors = 0;
            ToBeRemovedPencilmarks.Clear();
        }

        public int ColorizedPencilmarkCount
        {
            get
            {
                int pmColors = _pmColors;
                int mask = (int)PMColor.MASK;
                int returnValue = 0;
                while (pmColors != 0)
                {
                    pmColors >>= COLOR_MASK_SIZE;
                    if ((pmColors & mask) != 0)
                        returnValue++;
                }

                return returnValue;
            }
        }

        public List<int> ListOfColorizedPencilmarks()
        {
            var returnValue = new List<int>();
            int pmColors = _pmColors;
            int mask = (int)PMColor.MASK;
            int pm = 1;
            while (pmColors != 0)
            {
                pmColors >>= COLOR_MASK_SIZE;
                if ((pmColors & mask) != 0)
                    returnValue.Add(pm);

                pm++;
            }

            return returnValue;
        }

        public List<int> ListOfPMsForRemoval
        {
            get
            {
                var returnValue = new List<int>();
                if (ToBeRemovedPencilmarks != null)
                    returnValue.AddRange(ToBeRemovedPencilmarks);

                return returnValue;
            }
        }

        public PMColor GetPencilmarkColor(int pm)
        {
            int shiftSize = pm * COLOR_MASK_SIZE;
            int mask = (int)PMColor.MASK << shiftSize;
            int returnValue = (_pmColors & mask) >> shiftSize;
            return (PMColor)returnValue;
        }

        public List<int> GetPMsWithColor(PMColor searchColor)
        {
            var returnValue = new List<int>();
            int pmColors = _pmColors;
            int pm = 1;
            while (pmColors != 0)
            {
                pmColors >>= COLOR_MASK_SIZE;
                if ((pmColors & (int)searchColor) == (int)searchColor)
                    returnValue.Add(pm);

                pm++;
            }

            return returnValue;
        }

        public PMColor GetColorOfOnlyColorizedPM()
        {
            int mask = (int)PMColor.MASK;
            int returnValue = 0;
            int workColors = _pmColors;

            if (_pmColors == 0)
                throw new InvalidOperationException("Cell does not have any colorized pencilmark.");

            while (workColors != 0)
            {
                workColors >>= COLOR_MASK_SIZE;
                returnValue = workColors & mask;
                if (returnValue != (int)PMColor.NONE)
                    break;
            }

            if ((workColors & ~(int)PMColor.MASK) != 0)
                throw new InvalidOperationException("Cell has more than one pencilmark colorized.");

            return (PMColor)returnValue;
        }

        /// <summary>
        /// If a cell has 2 pencilmarks colorized with the same color, this property will return 
        /// the color.  If there is no color duplication in the pencilmarks, the property will be
        /// PMColor.NONE
        /// </summary>
        public PMColor DuplicatePMColor
        {
            get
            {
                int dim = Board.Dimension;
                var colorList = new List<PMColor>();
                int pmColors = _pmColors;
                while (pmColors != 0)
                {
                    pmColors >>= COLOR_MASK_SIZE;
                    int singleColor = pmColors & (int)PMColor.MASK;
                    if (singleColor != (int)PMColor.NONE)
                    {
                        if (colorList.Contains((PMColor)singleColor))
                            return (PMColor)singleColor;

                        colorList.Add((PMColor)singleColor);
                    }
                }

                return PMColor.NONE;
            }
        }

        /// <summary>
        /// Method to return the pencilmark mask for uncolorized pencilmarks.  
        /// </summary>
        /// <returns>returns a mask to check pencilmarks, not to check colorization.  It just excludes
        /// the pencilmarks which have been colorized in the cell.  The returned mask is a pencilmark
        /// mask, not a colorization mask.</returns>
        public int PMUncolorizedSignature
        {
            get
            {
                int returnValue = 0;
                int workBit = 1 << Board.Dimension;
                int colorMask = (int)PMColor.MASK << (COLOR_MASK_SIZE * Board.Dimension);
                while (workBit != 1)
                {
                    if ((workBit & _pencilMarks) != 0 && (colorMask & _pmColors) == 0)
                        returnValue |= workBit;

                    workBit >>= 1;
                    colorMask >>= COLOR_MASK_SIZE;
                }

                return returnValue;
            }
        }

        #endregion

        internal int GetCoordinate(int coordNumber)
        {
            int returnValue;
            switch (coordNumber)
            {
                case Coord.ROW: returnValue = RowIndex; break;
                case Coord.COL: returnValue = ColIndex; break;
                case Coord.SECTION: returnValue = GroupIndex; break;
                default:
                    throw new ArgumentException("Argument coordNumber must be 1 for Row, or 2 for Column.");
            }

            return returnValue;
        }

        internal PencilMarkInfo PencilMarkInfo
        {
            get
            {
                var returnValue = new PencilMarkInfo()
                {
                    Cell = this,
                    Count = PencilMarkCount,
                    Signature = _pencilMarks
                };

                return returnValue;
            }
        }

        /// <summary>
        /// Method to fill a cell with a value without changing any pencilmarks.
        /// </summary>
        /// <param name="value">Value for the cell</param>
        /// <param name="mutable">When mutable is false, it means the number is part
        /// of the original puzzle and not a solution value.</param>
        internal void SetValue(int value, bool mutable = true)
        {
            Value = value;
            Mutable = mutable;
            _pencilMarks = 0;
            PencilMarkCount = 0;
        }

        public void ClearCellValue()
        {
            SetPencilMark(Value, false/*pencilMarkActive*/);
            Value = 0;
        }

        public bool SetPencilMark(int value, bool pencilMarkActive)
        {
            int settingMask = 1 << value;
            int clearingMask = ~settingMask;

            if (!pencilMarkActive && (_pencilMarks & settingMask) != 0)
            {
                _pencilMarks &= clearingMask;
                PencilMarkCount--;
                return true;
            }

            if (pencilMarkActive && (_pencilMarks & settingMask) == 0)
            {
                _pencilMarks |= settingMask;
                PencilMarkCount++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to clear multiple pencilmarks at once.  For example if the 
        /// signature is 22d (16H) (0x10110) it will clear pencilmarks 1, 2, and 4.
        /// The LSB is pencilmark 0 which is not used.
        /// </summary>
        /// <param name="signature">signature on what to clear.   Any bit set to 1 in the
        /// signature is a pencilmark to be CLEARED.</param>
        /// <returns></returns>
        internal void ClearMultiplePencilMarks(int signature)
        {
            int changedBits = (signature & _pencilMarks).NumberOfBitsSet();
            PencilMarkCount -= changedBits;
            _pencilMarks &= ~signature;
        }

        internal void SetMultiplePencilMarks(int signature)
        {
            int changedBits = (signature ^ _pencilMarks).NumberOfBitsSet();
            PencilMarkCount += changedBits;
            _pencilMarks |= signature;
        }

        public List<Cell> ShadowedCells
        {
            get
            {
                List<Cell> returnValue = Board.GetLine(Coord.ROW, RowIndex)
					.Where(c => c.Value == 0)
                    .Union(Board.GetLine(Coord.COL, ColIndex).Where(c => c.Value == 0))
                    .Union(Board.GetLine(Coord.SECTION, GroupIndex).Where(c => c.Value == 0))
                    .Except(new List<Cell> { this })
                    .ToList();

                return returnValue;
            }
        }
    }
}
