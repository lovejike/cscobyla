//------------------------------------------------------------------------------
//     Main program of test problems in Report DAMTP 1992/NA5.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cureos.Numerics;
using NUnit.Framework;

using TestCaseType = System.Tuple<string, Cureos.Numerics.CalcfcDelegate, int, int, double[], double>;

namespace Cscobyla.Tests
{
    [TestFixture]
    public class Cobyla2Tests
    {
        #region FIELDS

        private const int iprint = 1;
        private const double rhobeg = 0.5;
        private const double rhoend1 = 1.0e-6;
        private const double rhoend2 = 1.0e-8;

        #endregion

        #region AUTO-IMPLEMENTED PROPERTIES

        private IEnumerable<TestCaseType> TestCases
        {
            get
            {
                yield return
                    Tuple.Create("1 (Simple quadratic)", (CalcfcDelegate)calcfc1, 2, 0, new[] {-1.0, 0.0}, 1.0e-5);
                yield return
                    Tuple.Create("2 (2D unit circle calculation)", (CalcfcDelegate)calcfc2, 2, 1,
                                 new[] {Math.Sqrt(0.5), -Math.Sqrt(0.5)}, 1.0e-5);
                yield return
                    Tuple.Create("3 (3D ellipsoid calculation)", (CalcfcDelegate)calcfc3, 3, 1,
                                 new[] {1.0 / Math.Sqrt(3.0), 1.0 / Math.Sqrt(6.0), -1.0 / 3.0}, 1.0e-5);
                yield return
                    Tuple.Create("4 (Weak Rosenbrock)", (CalcfcDelegate)calcfc4, 2, 0, new[] {-1.0, 1.0}, 2.0e-5);
                yield return
                    Tuple.Create("5 (Intermediate Rosenbrock)", (CalcfcDelegate)calcfc5, 2, 0, new[] {-1.0, 1.0}, 2.0e-4)
                    ;
                yield return
                    Tuple.Create("6 (Equation (9.1.15) in Fletcher's book)", (CalcfcDelegate)calcfc6, 2, 2,
                                 new[] {Math.Sqrt(0.5), Math.Sqrt(0.5)}, 1.0e-6);
                yield return
                    Tuple.Create("7 (Equation (14.4.2) in Fletcher)", (CalcfcDelegate)calcfc7, 3, 3,
                                 new[] {0.0, -3.0, -3.0}, 1.0e-8);
                yield return
                    Tuple.Create("8 (Rosen-Suzuki)", (CalcfcDelegate)calcfc8, 4, 3,
                                 new[] {0.0, 1.0, 2.0, -1.0}, 1.0e-5);
                yield return
                    Tuple.Create("9 (Hock and Schittkowski 100)", (CalcfcDelegate)calcfc9, 7, 4,
                                 new[] { 2.330499, 1.951372, -0.4775414, 4.365726, -0.624487, 1.038131, 1.594227 }, 1.0e-5);
                yield return
                    Tuple.Create("10 (Hexagon area)", (CalcfcDelegate)calcfc10, 9, 14,
                                 new[] { 0.688341, 0.725387, -0.284033, 0.958814, 0.688341, 0.725387, -0.284033, 0.958814, 0.0 }, 
                                 1.0e-5);
            }
        }
 
        #endregion

        #region METHODS

        [TestCaseSource("TestCases")]
        public void TestProblem(TestCaseType testCase)
        {
            var problem = testCase.Item1;
            var calcfc = testCase.Item2;
            var n = testCase.Item3;
            var m = testCase.Item4;
            var xopt = testCase.Item5;
            var accepted = testCase.Item6;

            Console.WriteLine("{0}Output from test problem {1}", Environment.NewLine, problem);

            var error1 = RunTestProblem(calcfc, n, m, rhoend1, xopt);
            Assert.Less(error1, accepted);
            var error2 = RunTestProblem(calcfc, n, m, rhoend2, xopt);
            Assert.Less(error2, error1);
        }

        public double RunTestProblem(CalcfcDelegate calcfc, int n, int m, double rhoend, double[] xopt)
        {
            var x = Enumerable.Repeat(1.0, n).ToArray();
            var maxfun = 3500;

            var timer = new Stopwatch();
            timer.Restart();
            Cobyla2.Minimize(calcfc, n, m, x, rhobeg, rhoend, iprint, ref maxfun);
            timer.Stop();

            var error = xopt.Select((xo, i) => Math.Pow(xo - x[i], 2.0)).Sum();
            Console.WriteLine("{0}Least squares error in variables = {1,16:E6}", Environment.NewLine, error);
            Console.WriteLine("Elapsed time for optimization = {0} ms", timer.ElapsedMilliseconds);

            return error;
        }

        /// <summary>
        /// Minimization of a simple quadratic function of two variables.
        /// </summary>
        public static void calcfc1(int n, int m, double[] x, out double f, double[] con)
        {
            f = 10.0 * Math.Pow(x[0] + 1.0, 2.0) + Math.Pow(x[1], 2.0);
        }

        /// <summary>
        /// Easy two dimensional minimization in unit circle.
        /// </summary>
        public static void calcfc2(int n, int m, double[] x, out double f, double[] con)
        {
            f = x[0] * x[1];
            con[0] = 1.0 - x[0] * x[0] - x[1] * x[1];
        }

        /// <summary>
        /// Easy three dimensional minimization in ellipsoid.
        /// </summary>
        public static void calcfc3(int n, int m, double[] x, out double f, double[] con)
        {
            f = x[0] * x[1] * x[2];
            con[0] = 1.0 - x[0] * x[0] - 2.0 * x[1] * x[1] - 3.0 * x[2] * x[2];
        }

        /// <summary>
        /// Weak version of Rosenbrock's problem.
        /// </summary>
        public static void calcfc4(int n, int m, double[] x, out double f, double[] con)
        {
            f = Math.Pow(x[0] * x[0] - x[1], 2.0) + Math.Pow(1.0 + x[0], 2.0);
        }

        /// <summary>
        /// Intermediate version of Rosenbrock's problem.
        /// </summary>
        public static void calcfc5(int n, int m, double[] x, out double f, double[] con)
        {
            f = 10.0 * Math.Pow(x[0] * x[0] - x[1], 2.0) + Math.Pow(1.0 + x[0], 2.0);
        }

        /// <summary>
        /// This problem is taken from Fletcher's book Practical Methods of
        /// Optimization and has the equation number (9.1.15).
        /// </summary>
        public static void calcfc6(int n, int m, double[] x, out double f, double[] con)
        {
            f = -x[0] - x[1];
            con[0] = x[1] - x[0] * x[0];
            con[1] = 1.0 - x[0] * x[0] - x[1] * x[1];
        }

        /// <summary>
        /// This problem is taken from Fletcher's book Practical Methods of
        /// Optimization and has the equation number (14.4.2).
        /// </summary>
        public static void calcfc7(int n, int m, double[] x, out double f, double[] con)
        {
            f = x[2];
            con[0] = 5.0 * x[0] - x[1] + x[2];
            con[1] = x[2] - x[0] * x[0] - x[1] * x[1] - 4.0 * x[1];
            con[2] = x[2] - 5.0 * x[0] - x[1];
        }

        /// <summary>
        /// This problem is taken from page 66 of Hock and Schittkowski's book Test
        /// Examples for Nonlinear Programming Codes. It is their test problem Number
        /// 43, and has the name Rosen-Suzuki.
        /// </summary>
        public static void calcfc8(int n, int m, double[] x, out double f, double[] con)
        {
            f = x[0] * x[0] + x[1] * x[1] + 2.0 * x[2] * x[2] + x[3] * x[3] - 5.0 * x[0] - 5.0 * x[1] - 21.0 * x[2] +
                7.0 * x[3];
            con[0] = 8.0 - x[0] * x[0] - x[1] * x[1] - x[2] * x[2] - x[3] * x[3] - x[0] + x[1] - x[2] + x[3];
            con[1] = 10.0 - x[0] * x[0] - 2.0 * x[1] * x[1] - x[2] * x[2] - 2.0 * x[3] * x[3] + x[0] + x[3];
            con[2] = 5.0 - 2.0 * x[0] * x[0] - x[1] * x[1] - x[2] * x[2] - 2.0 * x[0] + x[1] + x[3];
        }

        /// <summary>
        /// This problem is taken from page 111 of Hock and Schittkowski's
        /// book Test Examples for Nonlinear Programming Codes. It is their
        /// test problem Number 100.
        /// </summary>
        public static void calcfc9(int n, int m, double[] x, out double f, double[] con)
        {
            f = Math.Pow(x[0] - 10.0, 2.0) + 5.0 * Math.Pow(x[1] - 12.0, 2.0) + Math.Pow(x[2], 4.0) +
                3.0 * Math.Pow(x[3] - 11.0, 2.0) + 10.0 * Math.Pow(x[4], 6.0) + 7.0 * x[5] * x[5] + Math.Pow(x[6], 4.0) -
                4.0 * x[5] * x[6] - 10.0 * x[5] - 8.0 * x[6];
            con[0] = 127.0 - 2.0 * x[0] * x[0] - 3.0 * Math.Pow(x[1], 4.0) - x[2] - 4.0 * x[3] * x[3] - 5.0 * x[4];
            con[1] = 282.0 - 7.0 * x[0] - 3.0 * x[1] - 10.0 * x[2] * x[2] - x[3] + x[4];
            con[2] = 196.0 - 23.0 * x[0] - x[1] * x[1] - 6.0 * x[5] * x[5] + 8.0 * x[6];
            con[3] = -4.0 * x[0] * x[0] - x[1] * x[1] + 3.0 * x[0] * x[1] - 2.0 * x[2] * x[2] - 5.0 * x[5] + 11.0 * x[6];
        }

        /// <summary>
        /// This problem is taken from page 415 of Luenberger's book Applied
        /// Nonlinear Programming. It is to maximize the area of a hexagon of
        /// unit diameter.
        /// </summary>
        public static void calcfc10(int n, int m, double[] x, out double f, double[] con)
        {
            f = -0.5 * (x[0] * x[3] - x[1] * x[2] + x[2] * x[8] - x[4] * x[8] + x[4] * x[7] - x[5] * x[6]);
            con[0] = 1.0 - x[2] * x[2] - x[3] * x[3];
            con[1] = 1.0 - x[8] * x[8];
            con[2] = 1.0 - x[4] * x[4] - x[5] * x[5];
            con[3] = 1.0 - x[0] * x[0] - Math.Pow(x[1] - x[8], 2.0);
            con[4] = 1.0 - Math.Pow(x[0] - x[4], 2.0) - Math.Pow(x[1] - x[5], 2.0);
            con[5] = 1.0 - Math.Pow(x[0] - x[6], 2.0) - Math.Pow(x[1] - x[7], 2.0);
            con[6] = 1.0 - Math.Pow(x[2] - x[4], 2.0) - Math.Pow(x[3] - x[5], 2.0);
            con[7] = 1.0 - Math.Pow(x[2] - x[6], 2.0) - Math.Pow(x[3] - x[7], 2.0);
            con[8] = 1.0 - x[6] * x[6] - Math.Pow(x[7] - x[8], 2.0);
            con[9] = x[0] * x[3] - x[1] * x[2];
            con[10] = x[2] * x[8];
            con[11] = -x[4] * x[8];
            con[12] = x[4] * x[7] - x[5] * x[6];
            con[13] = x[8];
        }

        #endregion
    }
}
