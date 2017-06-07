namespace SudokuSolverApp
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;
	using SudokuSolver;
	using SudokuSolver.GameParts;
	using SudokuSolver.Interfaces;

	public partial class Form1 : Form
    {
        private enum GameReadingStatus
        {
            None,
            InGameTitle,
            ReadingGameLines,
            InError
        }

        public static Form1 MainForm;
        private static List<UICell> UIDigits = new List<UICell>();
        private List<ISolutionStep> PuzzleSolution { get; set; }
        private Dictionary<string, string> DictionaryDemoGames;
        private int CurrentStep;

        public Form1()
        {
            DictionaryDemoGames = new Dictionary<string, string>();
            MainForm = this;
            InitializeComponent();

            UIGameBoard.Board = new GameBoard(9, @"
xxx xx3 xxx // xx8 47x 35x // xxx 589 64x
x7x xxx 89x // xx3 xxx 2xx // x86 xxx x1x
x31 725 xxx // x29 x18 4xx // xxx 3xx xxx
");

            ResetButtonStates();
            UIGameBoard.BringToFront();
            UIGameBoard.Board = UIGameBoard.Board;
        }

        private void ButtonEnterGame_Click(object sender, EventArgs e)
        {
            bool newEditMode = !UIGameBoard.Board.EditMode;
            UIGameBoard.Board.EditMode = newEditMode;
            ButtonEditGame.Text = newEditMode ? "End Edit" : "Edit Game";
            Invalidate(true);
            Update();
        }

        private void OnClearAllCells(object sender, EventArgs e)
        {
            UIGameBoard.Board.ClearAllCells();
            ButtonEditGame_Click(null, null);
        }

        private void ButtonEditGame_Click(object sender, EventArgs e)
        {
            if (PuzzleSolution != null && CurrentStep > 0)
                ButtonUndoAll_Click(null, null);

            UIGameBoard.TurnOffHighlighting();
            UIGameBoard.Board.EditMode = true;
            ButtonEditGame.Visible = false;
            ButtonSolve.Visible = false;
            ButtonEndEdit.Visible = true;
            UIGameBoard.Invalidate(true/*invalidateChildren*/);
            UIGameBoard.Update();
        }

        private void ButtonEndEdit_Click(object sender, EventArgs e)
        {
            UIGameBoard.Board.EditMode = false;
            ButtonEditGame.Visible = true;
            ButtonEndEdit.Visible = false;
            ButtonSolve.Visible = true;
            UIGameBoard.Invalidate(true/*invalidateChildren*/);
            UIGameBoard.Update();
        }

        private void OnSolvePuzzle_Click(object sender, EventArgs e)
        {
            bool couldSolve;
            UIGameBoard.SuspendLayout();
            PuzzleSolution = GameSolver.Solve(UIGameBoard.Board, out couldSolve);
            if (!couldSolve)
            {
                new TooHardBox().ShowDialog();
            }

            CurrentStep = PuzzleSolution.Count;
            ButtonUndoAll_Click(null, null);
            ButtonSolve.Visible = false;
            UpdateRuleButtons();
            UIGameBoard.ResumeLayout(true/*performLayout*/);

        }

        private void UpdateRuleButtons()
        {
            if (PuzzleSolution == null || PuzzleSolution.Count == 0)
                return;

            ButtonUndoAll.Enabled = ButtonStepBack.Enabled = (CurrentStep > 0);
            ButtonRedoAll.Enabled = ButtonStepForward.Enabled = (CurrentStep < PuzzleSolution.Count);
            if (CurrentStep < PuzzleSolution.Count)
            {
                ISolutionStep step = PuzzleSolution[CurrentStep];
                TextBoxRuleInformation.Text = step.Solution.Description;
                UIGameBoard.HighlightCells(step.Solution);
            }
            else
            {
                TextBoxRuleInformation.Text = string.Empty;
            }
        }

        private void ButtonStepForward_Click(object sender, EventArgs e)
        {
            Step(1);
        }

        private void ButtonStepBack_Click(object sender, EventArgs e)
        {
            Step(-1);
        }


        private void ButtonRedoAll_Click(object sender, EventArgs e)
        {
            Step(PuzzleSolution.Count - CurrentStep);
        }

        private void ButtonUndoAll_Click(object sender, EventArgs e)
        {
            Step(-CurrentStep);
        }
        
        private void Step(int stepCount)
        {
            if (stepCount == 0)
                return;

            UIGameBoard.SuspendLayout();
            UIGameBoard.TurnOffHighlighting();
            int increment = (stepCount > 0) ? -1 : 1;
            while (stepCount != 0)
            {
                stepCount += increment;
                if (increment < 0)  // moving forward
                    PuzzleSolution[CurrentStep++].Apply();
                else
                    PuzzleSolution[--CurrentStep].Undo();
            }

            UpdateRuleButtons();
            UIGameBoard.ResumeLayout();
            UIGameBoard.Invalidate(true/*invalidateChildren*/);
            UIGameBoard.Update();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string demoGamesFile = "DemoGames.txt";
            if (!File.Exists(demoGamesFile))
                return;

            List<string> allLines = File.ReadAllLines(demoGamesFile).ToList();
            if (allLines[allLines.Count - 1].Trim().Length > 0)
                allLines.Add("  ");

            int len = allLines.Count;
            GameReadingStatus status = GameReadingStatus.None;

            var sbGame = new StringBuilder();
            string gameName = null;
            ListBoxDemoGames.SuspendLayout();
            for (int i = 0; i < len; i++)
            {
                string singleLine = allLines[i].Trim();
                if (singleLine.Length == 0)
                {
                    if (status != GameReadingStatus.ReadingGameLines)
                    {
                        if (status == GameReadingStatus.InError)
                            status = GameReadingStatus.None;

                        continue;
                    }

                    DictionaryDemoGames.Add(gameName, sbGame.ToString());
                    status = GameReadingStatus.None;
                    continue;
                }

                if (status == GameReadingStatus.None)
                {
                    gameName = singleLine;
                    if (DictionaryDemoGames.ContainsKey(gameName))
                    {
                        MessageBox.Show(string.Format("Error reading file DemoGames.txt.  Error around line {0}", i));
                        status = GameReadingStatus.InError;
                        continue;
                    }

                    status = GameReadingStatus.ReadingGameLines;
                    sbGame.Clear();
                    continue;
                }

                if (status == GameReadingStatus.ReadingGameLines)
                    sbGame.Append("\r\n").Append(singleLine);
            }

            ListBoxDemoGames.Items.AddRange(DictionaryDemoGames.Keys.ToArray());
            ListBoxDemoGames.ResumeLayout();
        }

        private void ResetButtonStates()
        {
            CurrentStep = 0;
            PuzzleSolution = null;
            ButtonEditGame.Enabled = ButtonEditGame.Visible = true;
            ButtonClearAll.Enabled = ButtonClearAll.Visible = true;
            ButtonEndEdit.Visible = false;
            ButtonSolve.Enabled = ButtonSolve.Visible = true;
            ButtonRedoAll.Enabled = false;
            ButtonUndoAll.Enabled = false;
            ButtonStepBack.Enabled = false;
            ButtonStepForward.Enabled = false;
        }

        private void ListBoxDemoGames_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string gameName = (string)ListBoxDemoGames.SelectedItem;
            UIGameBoard.Board = new GameBoard(9, DictionaryDemoGames[gameName]);
            PuzzleSolution = null;
            ResetButtonStates();
        }
    }
}
