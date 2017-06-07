namespace SudokuSolver.GameParts
{
    using System.Collections.Generic;

    public static class Mask
    {
        private static Dictionary<int, int> cache = new Dictionary<int,int>();

        public static int ForSettingPencilMarks(int dimension)
        {
            int returnValue = 0;
            if (cache.ContainsKey(dimension))
                returnValue = cache[dimension];
            else
            {
                int mask = 0;
                int i = 2;
                int count = 0;
                do
                {
                    mask |= i;
                    i <<= 1;
                } while (++count < dimension);

                cache.Add(dimension, mask);
                returnValue = mask;
            }

            return returnValue;
        }
    }
}
