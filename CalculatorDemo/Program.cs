using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorDemo
{
  class Program
  {
    static List<string> lstValues = new List<string>();

    static void Main(string[] args)
    {

      string[] sums = { "1 + 1", "2 * 2", "1 + 2 + 3", "6 / 2", "11 + 23", "11.1 + 23", "1 + 1 * 3", "( 11.5 + 15.4 ) + 10.1",
                        "23 - ( 29.3 - 12.5 )", "( 1 / 2 ) - 1 + 1", "10 - ( 2 + 3 * ( 7 - 5 ) )",
                        "1 + ( 2 + 3 ) - 1 * ( 1 + ( 6 / 2 + ( 3 - 1 * 3 ) ) + ( 8 - 2 ) ) + 6 + ( 99 * 1 + 9 )" };

      for (int i = 0; i < sums.Length; i++)
      {
        Console.WriteLine(string.Format("{0} = {1}", sums[i], Calculate(sums[i])));
        Console.WriteLine();
      }

      Console.ReadKey();
    }

    public static double Calculate(string sum)
    {
      lstValues = sum.Split(' ').ToList();

      if (lstValues.FindIndex(x => x.Contains("(")) > -1)
      {
        ProcessBrackets();
      }

      if (lstValues.FindIndex(x => x.Contains("*") || x.Contains("/")) > -1)
      {
        lstValues = ProcessMultiplyDivide(lstValues);
      }

      return GetTotalSum(lstValues);
    }

    private static void ProcessBrackets()
    {
      try
      {
        List<ValuePair> lstBracketInfo = new List<ValuePair>();

        List<KeyValuePair<int, int>> lstIndexesOfEachBracket = new List<KeyValuePair<int, int>>();

        // get the indexes of both left and right brackets (assuming their Count is equally the same)
        List<int> lstIndexesOfLeftBracket = Enumerable.Range(0, lstValues.Count).Where(i => lstValues[i] == "(").ToList();
        List<int> lstIndexesOfRightBracket = Enumerable.Range(0, lstValues.Count).Where(i => lstValues[i] == ")").ToList();

        // loop through Left Bracket indexes, but starts from the last index first
        for (int i = lstIndexesOfLeftBracket.Count - 1; i >= 0; i--)
        {
          // store current Left index
          int idxLeft = lstIndexesOfLeftBracket[i];
          // store the last Right Index
          int idxRight = lstIndexesOfRightBracket[lstIndexesOfRightBracket.Count - 1];

          // locate any nested brackets between Left and Right indexes (if any)
          int idx = lstIndexesOfRightBracket.FindIndex(x => idxLeft < x && x < idxRight);
          if (idx > -1)
          {
            // get the index of nearest matched Right Bracket value instead - copyOfRightBracketIndexes[idx]
            lstIndexesOfEachBracket.Add(new KeyValuePair<int, int>(idxLeft, lstIndexesOfRightBracket[idx]));

            // remove the index of the nested Right Bracket
            lstIndexesOfRightBracket.RemoveAt(idx);
          }
          else
          {
            // if there's no more value(s) found between current Left and Right indexes, then it's considered the last bracket (main bracket, not nested)
            lstIndexesOfEachBracket.Add(new KeyValuePair<int, int>(idxLeft, idxRight));

            // get the total sum of the main bracket (not nested)
            double sum = HandleBracketValues(lstIndexesOfEachBracket);

            // store the Start/End Index of main bracket, along with the total sum
            lstBracketInfo.Add(new ValuePair { StartIndex = idxLeft, EndIndex = idxRight, Sum = sum });

            // once done, reset the List for the next main bracket
            lstIndexesOfEachBracket.Clear();
            // clear the "used" Right Bracket info from List
            lstIndexesOfRightBracket.RemoveAt(i);
          }

        }

        for (int i = 0; i < lstBracketInfo.Count; i++)
        {
          // remove the range of Start till End Index
          lstValues.RemoveRange(lstBracketInfo[i].StartIndex, lstBracketInfo[i].EndIndex - lstBracketInfo[i].StartIndex + 1);

          // insert (replace) instead with the Sum of bracket, e.g. "( 1 + 1 )" will be replaced by "2" (value)
          lstValues.Insert(lstBracketInfo[i].StartIndex, lstBracketInfo[i].Sum.ToString());
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("An error has occurred! " + ex.InnerException);
      }
    }

    private static double HandleBracketValues(List<KeyValuePair<int, int>> bracketIndexes)
    {
      try
      {
        // receives list of all brackets (including nested) start and end indexes

        List<string> lstBracketValues = new List<string>();
        List<double> nestedBracketValueSum = new List<double>();

        double sumOfBracket = 0;

        // loop through each list of bracket indexes
        for (int i = 0; i < bracketIndexes.Count; i++)
        {
          // with each iteration, loop through the Start Index till End Index of each bracket
          for (int j = bracketIndexes[i].Key + 1; j < bracketIndexes[i].Value; j++)
          {
            // for handling nested brackets (if any)
            // check for any nested Start Index between current Bracket
            int idx = bracketIndexes.FindIndex(x => x.Key == j);
            if (idx > -1)
            {
              // if found, replace the nested bracket with the nested total sum instead
              lstBracketValues.Add(nestedBracketValueSum[idx].ToString());

              // skip the entire nested bracket
              j = bracketIndexes[idx].Value;
              continue;
            }

            // if there's no nested found, then insert the value from List (repeat till all the bracket values are added)
            lstBracketValues.Add(lstValues[j]);
          }

          // if the values contain Multiply or Divide, then process those values first
          if (lstBracketValues.FindIndex(x => x.Contains("*") || x.Contains("/")) > -1)
          {
            lstBracketValues = ProcessMultiplyDivide(lstBracketValues);
          }

          // get the total sum of the current bracket
          double currentSum = GetTotalSum(lstBracketValues);
          sumOfBracket = currentSum;

          // store each bracket's total sum in a List (be it nested bracket or main bracket)
          nestedBracketValueSum.Add(currentSum);
          lstBracketValues.Clear();
        }

        // return the main bracket sum
        return sumOfBracket;
      }
      catch (Exception ex)
      {
        Console.WriteLine("An error has occurred! " + ex.InnerException);
        return 0;
      }
    }

    private static List<string> ProcessMultiplyDivide(List<string> lstNums)
    {
      try
      {

        List<int> indexOfTimesDivide = Enumerable.Range(0, lstNums.Count).Where(i => lstNums[i] == "*" || lstNums[i] == "/").ToList();
        List<ValuePair> lstMultiplyDivideInfo = new List<ValuePair>();

        for (int i = 0; i < indexOfTimesDivide.Count; i++)
        {
          int currentIndex = indexOfTimesDivide[i];

          // we'll assume that the immediate row before and after Multiple(*) and Divide(/) are numbers
          double firstNumber = double.Parse(lstNums[currentIndex - 1]);
          double secondNumber = double.Parse(lstNums[currentIndex + 1]);
          double? result = 0;

          switch (lstNums[currentIndex])
          {
            case "*":
              result = firstNumber * secondNumber;
              break;
            case "/":
              result = firstNumber / secondNumber;
              break;
          }
          lstNums[currentIndex + 1] = result.ToString();

          // below checking if True means the next sum is also a Multiply(*) or a Divide(/) - this is to handle cases whereby the formula is "2 * 2 / 2"
          // indexOfTimesDivide[i + 1] - currentIndex == 2 >> this is to check if previous index is a Multiply(*) or a Divide(/)
          if (i < indexOfTimesDivide.Count - 1 && indexOfTimesDivide[i + 1] - currentIndex == 2)
          {
            result = null;
          }
          lstMultiplyDivideInfo.Add(new ValuePair { StartIndex = currentIndex - 1, EndIndex = result == null ? currentIndex : currentIndex + 1, Sum = result });

        }

        // loop from the last row first to remove range from back to front, else will hit the index not found error
        for (int i = lstMultiplyDivideInfo.Count - 1; i >= 0; i--)
        {
          lstNums.RemoveRange(lstMultiplyDivideInfo[i].StartIndex, lstMultiplyDivideInfo[i].EndIndex - lstMultiplyDivideInfo[i].StartIndex + 1);
          if (lstMultiplyDivideInfo[i].Sum != null)
          {
            lstNums.Insert(lstMultiplyDivideInfo[i].StartIndex, lstMultiplyDivideInfo[i].Sum.ToString());
          }
        }

        return lstNums;
      }
      catch (Exception ex)
      {
        Console.WriteLine("An error has occurred! " + ex.InnerException);
        return null;
      }
    }

    // function to specifically handles Plus(+) and Minus(-) values
    private static double GetTotalSum(List<string> sum)
    {
      try
      {
        if (sum == null || sum.Count() == 0)
        {
          Console.WriteLine("Unable to calculate the total Sum!");
          return 0;
        }

        double totalSum = double.Parse(sum[0]);
        string arithmetic = string.Empty;
        double value = 0;

        for (int i = 1; i < sum.Count; i++)
        {
          // try to parse as Double
          if (double.TryParse(sum[i], out value))
          {
            if (!string.IsNullOrEmpty(arithmetic))
            {
              switch (arithmetic)
              {
                case "+":
                  totalSum += value;
                  break;
                case "-":
                  totalSum -= value;
                  break;
              }
              // reset the arimetic operator
              arithmetic = string.Empty;
            }
          }
          else
          {
            // if it's not a double value, then it's assumed to be an arithmetic (+ or -)
            arithmetic = sum[i];
          }
        }

        return totalSum;
      }
      catch (Exception ex)
      {
        Console.WriteLine("An error has occurred! " + ex.InnerException);
        return 0;
      }
    }
  }
}

struct ValuePair
{
  public int StartIndex;
  public int EndIndex;

  // added Nullable to cater for Multiply/Divide scenario
  public double? Sum;
}
