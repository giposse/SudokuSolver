namespace SudokuSolverApp
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using SudokuSolver;
    using SudokuSolver.GameParts;
    using SudokuSolver.Interfaces;
    using SudokuSolver.Rules;
    using SudokuSolver.RuleData;

    public class UIGameBoard : Panel
    {
        private static int ThinGapSize = 1;
        private static int ThickGapIncrement = 3;
        private static int CellWidth;

        private GameBoard _gameBoard;
        internal UICell[,] UiCellMatrix;
        private List<UICell> HighlightedCells;
        private Timer cellsTimer;

        public GameBoard Board
        {
            get { return _gameBoard; }
            
            set
            {
                Controls.Clear();
                _gameBoard = value;
                UiCellMatrix = new UICell[_gameBoard.Dimension, _gameBoard.Dimension];
                value.GetAllLines(Coord.ROWS)
                    .SelectMany(t => t)
                    .ToList()
                    .Select(cell => CreateUICell(cell, CellWidth))
                    .ToList()
                    .ForEach(uiCell =>
                        {
                            Controls.Add(uiCell);
                            UiCellMatrix[uiCell.Cell.RowIndex, uiCell.Cell.ColIndex] = uiCell;
                            uiCell.UiGameBoard = this;
                        });
            }
        }

        private Color BACK_COLOR = Color.Black;

        public UIGameBoard() : this(411, Color.Black, new GameBoard(9, @"
1xx xxx xxx // x2x xxx xxx // xx3 xxx xxx
xxx 4xx xxx // xxx x5x xxx // xxx xx6 xxx
xxx xxx 7xx // xxx xxx x8x // xxx xxx xx9
"))
        {
        }

        public UIGameBoard(int width, Color backColor, GameBoard board)
        {
            HighlightedCells = new List<UICell>();
            cellsTimer = new Timer();
            cellsTimer.Tick += TurnOffHighlighting;
            cellsTimer.Enabled = false;
            cellsTimer.Stop();

            Board = board;
            int dimension = board.Dimension;
            int blockDimension = board.BlockDimension;
            int boardWidth = width;
            int totalGapWidth = ThinGapSize * (dimension + 1) +
                ThickGapIncrement * (1 + blockDimension);

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint, true);

            Width = Height = (((boardWidth - totalGapWidth) / dimension) * dimension) + totalGapWidth;
            CellWidth = (Width - totalGapWidth) / dimension;

            Board.GetAllLines(Coord.ROWS)
                .SelectMany(t => t)
                .ToList()
                .Select(cell => CreateUICell(cell, CellWidth))
                .ToList()
                .ForEach(uicell => Controls.Add(uicell));

            BackColor = BACK_COLOR;
        }

        private UICell CreateUICell(Cell cell, int cellWidth)
        {
            int col = cell.ColIndex;
            int row = cell.RowIndex;
            int x = 1 + (col * CellWidth) + (1 + col) * ThinGapSize +
                (1 + col / Board.BlockDimension) * ThickGapIncrement;

            int y = 1 + (row * CellWidth) + (1 + row) * ThinGapSize +
                (1 + row / Board.BlockDimension) * ThickGapIncrement;

            UICell returnValue = new UICell(cell, cellWidth);
            returnValue.Location = new Point(x, y);
            return returnValue;
        }

        private UICell UICellFromCell(Cell cell)
        {
            return UiCellMatrix[cell.RowIndex, cell.ColIndex];
        }

        internal void HighlightCells(SolveInfo solveInfo)
        {
            HighlightedCells.Clear();
            foreach (RuleFinding action in solveInfo.Actions)
            {
                UICell uiCell = UICellFromCell(action.Cell);
                uiCell.Highlight(action.CellRole, action.PencilmarkDataList);
                HighlightedCells.Add(uiCell);
            }
        }


        internal void HighlightCells(List<UICell> uiCellList, 
            Color backgroundColor, 
            double highlightSeconds,
            List<PMFinding> pmFindings)
        {
            uiCellList.ForEach(uiCell => uiCell.Highlight(CellRole.Pattern, pmFindings));
            if (highlightSeconds > 0.0)
            {
                cellsTimer.Interval = (int)(highlightSeconds * 1000.0);
                cellsTimer.Enabled = true;
                cellsTimer.Start();
            }

            HighlightedCells.AddRange(uiCellList);
        }

        internal void TurnOffHighlighting(object sender = null, EventArgs args = null)
        {
            cellsTimer.Enabled = false;
            HighlightedCells.ForEach(uicell => uicell.EndHighlight());
            HighlightedCells.Clear();
        }
    }
}
