namespace SudokuSolverApp
{
    using System.Collections.Generic;
    using System.Drawing;
    using SudokuSolver.RuleData;

    internal static class BoardTraits
    {
		private static Dictionary<PMRole, Brush[]> dictPMMappings = new Dictionary<PMRole, Brush[]>()
		{
			{ PMRole.Pattern, new[] { Brushes.White, Brushes.Black } },
			{ PMRole.Remove, new[] { Brushes.Transparent, Brushes.Black } },
			{ PMRole.ChainColor1, new[] { Brushes.Purple, Brushes.Yellow } },
			{ PMRole.ChainColor2, new[] { Brushes.ForestGreen, Brushes.White } },
			{ PMRole.ChainColorClash, new[] { Brushes.OrangeRed, Brushes.White} },
			{ PMRole.Solution, new [] { Brushes.White, Brushes.Black } },
			{ PMRole.ChainEnd, new[] { Brushes.White, Brushes.Indigo } },
			{ PMRole.EditBold, new[] { Brushes.MediumSeaGreen, Brushes.Yellow } }
		};

		internal static Dictionary<CellRole, Color> dictCellColorMappings = new Dictionary<CellRole, Color>()
        {
            { CellRole.None, Color.White },
            { CellRole.Pattern, Color.Yellow },
            { CellRole.Pattern2, Color.YellowGreen },
            { CellRole.Affected, Color.DarkGray },
            { CellRole.InvolvedLine, Color.Gainsboro }
        };

		internal static Dictionary<PMRole, Brush[]> DictPMMappings { get => dictPMMappings; set => dictPMMappings = value; }
	}
}
