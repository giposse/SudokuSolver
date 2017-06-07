namespace SudokuSolver.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameParts;

    public static partial class Extensions
    {
        public static int NumberOfBitsSet(this int that)
        {
            if (that < 0)
                throw new ArgumentException("NumberOfBitsSet only works with positive numbers");

            int returnValue;
            for (returnValue = 0; that != 0; returnValue++)
                that &= that - 1;

            return returnValue;
        }

        /// <summary>
        /// Extension which takes an int, and returns the list of bits set.
        /// The LSB is 0.  Example:  The intger 0xba = 1011 1010 binary and would return the list
        /// [1, 3, 4, 5, 7], which are the set bits, starting from the least significant as 
        /// bit number 0.
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static List<int> ListOfBits(this int that)
        {
            var returnValue = new List<int>();
            int pmValue = 0;
            while (that != 0)
            {
                if ((that & 0x01) == 0x01)
                    returnValue.Add(pmValue);

                pmValue++;
                that >>= 1;
            }

            return returnValue;
        }

        /// <summary>
        /// Converts a list of integers to a bit map.  Example: The list contains 1, 4, 5, 6. 
        /// The return value is 0111 0010 or 0x72.  The LSB is 0, so 1 goes in the 2nd LS bit.
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static int ToBitMask(this List<int> that)
        {
            int returnValue = 0;
            that.ForEach(t => returnValue |= (1 << t));
            return returnValue;
        }

        public static List<T> AddUnique<T>(this List<T> that, T item)
        {
            if (!that.Contains(item))
                that.Add(item);

            return that;
        }

        public static void Trim<T>(this List<T> that, int count)
        {
            if (count == 0)
            {
                that.Clear();
                return;
            }

            while (that.Count > count)
                that.RemoveAt(that.Count - 1);
        }

        /// <summary>
        /// Merges a list of masks into a single value, by ORing all the masks
        /// </summary>
        /// <param name="that">object being extended</param>
        /// <returns>An integer with the result of ORing (a|b) all the elements in
        /// the list</returns>
        public static int BitMerge(this List<int> that)
        {
            int returnValue = 0;
            that.ForEach(i => returnValue |= i);
            return returnValue;
        }

        public static IEnumerable<Cell> GetShadowedCellsInList(this Cell that, List<Cell> searchList, int pencilmark)
        {
            int pmMask = 1 << pencilmark;
            IEnumerable<Cell> returnValue = searchList
                .Where(cell => (cell.PMSignature & pmMask) != 0 &&
                    (cell.RowIndex == that.RowIndex || cell.ColIndex == that.ColIndex ||
                    cell.GroupIndex == that.GroupIndex));

            return returnValue;
        }

        public static PMColor ToggleColor(this PMColor that)
        {
            return (that == PMColor.COLOR1) ? PMColor.COLOR2 : PMColor.COLOR1;
        }
    }
}
