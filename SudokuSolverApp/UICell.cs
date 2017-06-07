namespace SudokuSolverApp
{
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using SudokuSolver.Extensions;
	using SudokuSolver.GameParts;
	using SudokuSolver.RuleData;

	public class UICell : Control
    {
        internal UIGameBoard UiGameBoard { get; set; }
        internal Cell Cell { get; set; }

        private List<PMFinding> HighlightedPMFindings;

        private static int CellWidth;
        private const int PmsPerLine = 3;
        private string NO_PM_MARKER = " \u00B7";
        private static Color NORMAL_BACK_COLOR = Color.White;

        private static FontFamily PM_FONT_FAMILY = new FontFamily("Verdana");
        private static FontFamily PM_EDIT_FONT_FAMILY = new FontFamily("Arial Black"); //"Helvetica-Black");
        private static FontFamily SOLVED_FONT_FAMILY = new FontFamily("Franklin Gothic Demi");
        
        public UICell() : base() { }

        public UICell(Cell cell, int cellWidth)
        {
            Cell = cell;
            CellWidth = cellWidth;
            Width = Height = cellWidth;
            BackColor = Color.White;
            MouseClick += new MouseEventHandler(OnEditModeClick);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            HighlightedPMFindings = HighlightedPMFindings ?? new List<PMFinding>();

            if (Cell == null)  // in design mode
                return;

            bool editMode = (Cell.Board == null) ? false : Cell.Board.EditMode;
            int dim = (Cell.Board == null) ? 9 : Cell.Board.Dimension;
            Graphics graphics = pe.Graphics;
            float x, y;
            float fontHeightPixels = Height / 6.0f;
            int pmGridMargin = Height / 14;
            int pmGridHeight = (int)fontHeightPixels + 5;
            int pmGridWidth = (int)fontHeightPixels + 6;
            using (Font pmFont = new Font(editMode ? PM_EDIT_FONT_FAMILY : PM_FONT_FAMILY,
                fontHeightPixels,
                FontStyle.Regular,
                GraphicsUnit.Pixel))

            using (Font pmBoldFont = new Font(editMode ? PM_EDIT_FONT_FAMILY : PM_FONT_FAMILY,
                fontHeightPixels,
                FontStyle.Bold,
                GraphicsUnit.Pixel))
            {
                if (Cell.Value == 0)
                {
                    List<int> pencilmarks = Cell.PMSignature.ListOfBits();
                    List<int> pmsToRemove = Cell.ListOfPMsForRemoval;
                    int limit = editMode ? dim : pencilmarks.Count;
                    x = y = pmGridMargin;
                    for (int i = 0; i < limit; i++)
                    {
                        x = pmGridMargin + pmGridWidth * (i % 3);
                        y = pmGridMargin + pmGridHeight * (i / 3);
                        int pmValue = editMode ? (i + 1) : pencilmarks[i];

                        List<PMFinding> highlightList = HighlightedPMFindings
                            .Where(pmf => pmf.Value == pmValue)
                            .ToList();

                        if (highlightList.Count == 0)
                            highlightList.Add(null);

                        foreach (PMFinding singleFinding in highlightList)
                        {
                            bool pmWillBeRemoved = pmsToRemove.Contains(pmValue);
                            Pen workPen = pmWillBeRemoved ? Pens.Red : Pens.Red;
                            Pen workPen2 = pmWillBeRemoved ? Pens.Red : Pens.Red;
                            Brush[] brushes = (singleFinding == null) ? null : BoardTraits.DictPMMappings[singleFinding.Role];
                            Font workFont = (editMode) ? pmBoldFont : pmFont;
                            string singlePM = Cell.IsPencilmarkSet(pmValue) ? pmValue.ToString() : NO_PM_MARKER;
                            if (singleFinding != null)
                            {
                                if (pmWillBeRemoved || brushes[0] == Brushes.Transparent)
                                {
                                    graphics.DrawRectangle(workPen2, x - 2, y - 1, pmGridWidth - 3, pmGridHeight - 2);
                                    graphics.DrawRectangle(workPen, x - 3, y - 2, pmGridWidth - 1, pmGridHeight);
                                }

                                if (brushes[0] != Brushes.Transparent)
                                    graphics.FillRectangle(brushes[0], x - 1, y, pmGridWidth - 4, pmGridHeight - 3);

                                graphics.DrawString(singlePM, workFont, brushes[1], x, y);
                            }
                            else
                                graphics.DrawString(singlePM, workFont, editMode ? Brushes.Green : Brushes.Navy, x, y);
                        }
                    }
                }
                else
                {
                    float fsize = Height * 0.75f;
                    x = Width * 0.10f;
                    y = Height * 0.05f;
                    Font valueFont = new Font(SOLVED_FONT_FAMILY, fsize, FontStyle.Bold, GraphicsUnit.Pixel);
                    Brush workBrush = Cell.Mutable ? Brushes.MediumBlue : Brushes.Black;
                    graphics.DrawString(Cell.Value.ToString(), valueFont, workBrush, x, y);

                    if (editMode)
                    {
                        graphics.DrawString("C L E A R", pmFont, Brushes.Green, 4, Height - 10);
                    }
                }
            }
        }

        public void OnEditModeClick(object sender, MouseEventArgs args)
        {
            if (!Cell.Board.EditMode)
                return;

            UIGameBoard uiBoard = Form1.MainForm.UIGameBoard;
            if (Cell.Value > 0)
            {
                Cell.ClearCellValue();
                Cell.Board.RecalculatePencilMarks();
            }
            else
            {
                int widthSection = Width / 3;
                int heightSection = (int)((float)Height / 3.3f);
                int x = args.X / widthSection;
                int y = args.Y / heightSection;
                int option = 3 * y + x + 1;
                List<int> pencilMarks = Cell.PMSignature.ListOfBits();
                if (!Cell.IsPencilmarkSet(option))
                    return;

                List<Cell> emptyCells = Cell.ShadowedCells
                    .Where(cell => cell.IsPencilmarkSet(option) && cell.PencilMarkCount == 1)
                    .ToList();

                if (emptyCells.Count > 0)
                {
                    List<UICell> emptyUiCells = emptyCells
                        .Select(cell => UiGameBoard.UiCellMatrix[cell.RowIndex, cell.ColIndex])
                        .ToList();

                    UiGameBoard.HighlightCells(emptyUiCells, Color.Yellow, 1.0, 
                        new List<PMFinding> 
                        { 
                            new PMFinding(option, PMRole.ChainColor1)
                        });

                    return;
                }

                Cell.Board.SetCellValueAndAffectShadowedCells(Cell.RowIndex, Cell.ColIndex, option, false/*mutable*/);
            }

            uiBoard.Invalidate(true/*invalidateChildren*/);
            uiBoard.Update();
        }

        public void Highlight(CellRole cellRole, List<PMFinding> pmFindings)
        {
            HighlightedPMFindings = pmFindings;
            BackColor = BoardTraits.dictCellColorMappings[cellRole];
        }

        public void EndHighlight()
        {
            BackColor = BoardTraits.dictCellColorMappings[CellRole.None];
            HighlightedPMFindings = null;
            Invalidate();
            Update();
        }
    }
}
