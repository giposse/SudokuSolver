namespace SudokuSolver.RuleData
{
    using System;

    public class PMFinding
    {
        public int Value { get; set; }
        public PMRole Role { get; set; }

        public PMFinding(int value, PMRole role)
        {
            if (value < 1 || value > 9)
                throw new InvalidOperationException("The value must be between 1 and 9.");

            Value = value;
            Role = role;
        }
    }
}
