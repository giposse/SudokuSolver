namespace SudokuSolver.Interfaces
{
    using System.Collections.Generic;
    using System.Linq;
    using GameParts;
    using RuleData;

    public  interface ISolutionStep
    {
        bool FindPattern(GameBoard board);
        void Apply();
        void Undo();
        SolveInfo Solution { get; }
    }
}
