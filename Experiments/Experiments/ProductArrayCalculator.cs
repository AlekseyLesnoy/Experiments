using System.Collections.Generic;
using NUnit.Framework;

namespace Experiments
{
    // # Problem
    //     Given an array of integers, return a new array where each element is the product of all of the other elements in the array
    //
    //     E.G.
    //
    //     Given an array of [ 2, 4, 8, 16 ]
    //
    //     The returned array should be [ 512, 256, 128, 64 ]
    //
    //     array[0] = 512 = 4 * 8 * 16
    //     array[1] = 256 = 2 * 8 * 16
    //     etc.
    //
    //     Note: You cannot use Division in order to solve this problem
    //
    // # Test
    //     Your solution should be able to make the following tests pass (feel free to change to the language of your choice)
    [TestFixture]
    public class ProductArrayCalculatorTest
    {
        public static IEnumerable<TestCaseData> Arrays()
        {
            yield return new TestCaseData(new[] { 2, 4, 8, 16 }, new[] { 512, 256, 128, 64 });
            yield return new TestCaseData(new[] { 6, 10, 2, 144 }, new[] { 2880, 1728, 8640, 120 });
            yield return new TestCaseData(new[] { 10, 4, 7, 2, 2 }, new[] { 112, 280, 160, 560, 560 });
        }
        
        [TestCaseSource(nameof(Arrays))]
        public void CalculateProductArray_Basic_Return_ProductOfAllOtherInts(int[] inputArray, int[] expected)
        {
            ProductArrayCalculator sut = new();

            var actual = sut.CalculateProductArray_Basic(inputArray);

            Assert.That(actual, Is.EqualTo(expected));
        }
        
        [TestCaseSource(nameof(Arrays))]
        public void CalculateProductArray_Improved_Should_Return_ProductOfAllOtherInts(int[] inputArray, int[] expected)
        {
            ProductArrayCalculator sut = new();

            var actual = sut.CalculateProductArray_Improved(inputArray);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    public class ProductArrayCalculator
    {
        public int[] CalculateProductArray_Basic(int[] inputArray)
        {
            var retVal = new int[inputArray.Length];
            for (int i = 0; i < inputArray.Length; i++)
            {
                var value = 0;

                for (int j = 0; j < inputArray.Length; j++)
                {
                    if (j==i)
                        continue;

                    value = value == 0 ? inputArray[j] : value *= inputArray[j];
                }

                retVal[i] = value;
            }
            
            return retVal;
        }

        public int[] CalculateProductArray_Improved(int[] inputArray)
        {
            int cumulativeProduct = 1;
            int[] leftProduct = new int[inputArray.Length];

            for(int i = 0; i < inputArray.Length; i++)
            {
                cumulativeProduct = cumulativeProduct * inputArray[i];
                leftProduct[i] = cumulativeProduct;
            }

            cumulativeProduct = 1;
            int[] rightProduct = new int[inputArray.Length];

            for(int i = inputArray.Length - 1; i >= 0; i--)
            {
                cumulativeProduct = cumulativeProduct * inputArray[i];
                rightProduct[i] = cumulativeProduct;
            }

            int[] outputArray = new int[inputArray.Length];
            for(int i = 0; i < outputArray.Length; i++)
            {
                if(i == 0)
                    outputArray[i] = rightProduct[i + 1];
                else if(i == inputArray.Length - 1)
                    outputArray[i] = leftProduct[inputArray.Length - 2];
                else
                    outputArray[i] = leftProduct[i - 1] * rightProduct[i + 1];
            }

            return outputArray;
        }
    }
}