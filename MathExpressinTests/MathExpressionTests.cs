using Expressionator.Expressions.Builder;
using Expressionator.Expressions.Evaluator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MathExpressinTests
{
    [TestClass]
    public class MathExpressionTests
    {
        [TestClass()]
        public class ExpressionEvaluatorTests
        {


            [TestMethod()]
            public void AdditionNumberTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression("1+1");
                Assert.AreEqual(result.Number, 2);
            }

            [TestMethod()]
            public void DivisionByZeroTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression("5/0");
                Assert.AreEqual(result.Number, double.PositiveInfinity);
            }

            [TestMethod()]
            public void TextEqualTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression("'hans' = 'hans'");

                Assert.IsTrue(result.Boolean);
            }


            [TestMethod()]
            public void CombineTextTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression("'hans'+'hans'");

                Assert.AreEqual(result.Text, "hanshans");
            }


            [TestMethod()]
            [ExpectedException(typeof(UnknownPrecisionException))]
            public void UnknownRoundPrecisionTest()
            {
                var d1 = 0.2;
                ExpressionEvaluator.EvaluateExpression($"roundFALSE1({d1})");
            }

            [TestMethod()]
            public void RoundTest()
            {
                var d1 = 0.2;
                var d2 = 0.5126;
                var d3 = 0.5;

                var result = ExpressionEvaluator.EvaluateExpression($"round({d1})");
                Assert.AreEqual(result.Number, 0);

                result = ExpressionEvaluator.EvaluateExpression($"round({d2})");
                Assert.AreEqual(result.Number, 1);

                result = ExpressionEvaluator.EvaluateExpression($"round({d3})");
                Assert.AreEqual(result.Number, 0);

                result = ExpressionEvaluator.EvaluateExpression($"round1({d2})");
                Assert.AreEqual(result.Number, 0.5);

                result = ExpressionEvaluator.EvaluateExpression($"round2({d2})");
                Assert.AreEqual(result.Number, 0.51);


                result = ExpressionEvaluator.EvaluateExpression($"round3({d2})");
                Assert.AreEqual(result.Number, 0.513);



            }

            [TestMethod()]
            public void SubstractDateTest()
            {
                var d1 = new DateTime(2020, 01, 01);
                var d2 = new DateTime(2019, 01, 01);
                var result = ExpressionEvaluator.EvaluateExpression($"{d1} - {d2}");

                Assert.AreEqual(result.DateRange.TotalDays, 365);
            }

            [TestMethod()]
            public void SubstractDateDayTest()
            {
                var d1 = new DateTime(2020, 01, 01);
                var d2 = new DateTime(2019, 01, 01);
                var result = ExpressionEvaluator.EvaluateExpression($"days({d1} - {d2})");

                Assert.AreEqual(result.Number, 365);
            }

            [TestMethod()]
            public void DateYearTest()
            {
                var d1 = new DateTime(2021, 01, 01);
                var result = ExpressionEvaluator.EvaluateExpression($"Year({d1})");

                Assert.AreEqual(result.ToString(), "2021");
            }



            [TestMethod()]
            public void BetweenNumberTest()
            {
                var d1 = 1;
                var d2 = 10;
                var d3 = 5;
                var result = ExpressionEvaluator.EvaluateExpression($"{d3} INNERHALB {d1} UND {d2}");

                Assert.IsTrue(result.Boolean);
            }

            [TestMethod()]
            public void BetweenDateTest()
            {
                var d1 = "01.01.2020";
                var d2 = "31.12.2020";
                var d3 = "15.06.2020";
                var result = ExpressionEvaluator.EvaluateExpression($"{d3} BETWEEN {d1} AND {d2}");

                Assert.IsTrue(result.Boolean);
            }



            [TestMethod()]
            public void IfThenAndOrTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression($"IF (1 > 0 AND 'Yes' != 'No') OR 100/10=10 THEN 'Yes, it´s true!' ELSE 'No! Your wrong ..'");
                Assert.IsTrue(result.Text == "Yes, it´s true!");

                result = ExpressionEvaluator.EvaluateExpression($"IF (1 > 0 AND 'Yes' = 'No') OR 100/10=50 THEN 'Yes, it´s true!' ELSE 'No! Your wrong ..'");
                Assert.IsTrue(result.Text == "No! Your wrong ..");

            }

            [TestMethod()]
            public void CombinedSubstractDateTest()
            {
                var d1 = new DateTime(2021, 01, 01);
                var d2 = new DateTime(2020, 01, 01);
                var result = ExpressionEvaluator.EvaluateExpression($"Years({d1} - {d2}) >= 1");

                Assert.IsTrue(result.Boolean);
            }

            [TestMethod()]
            public void CombinedSubstractDateStringTest()
            {
                var d1 = "01.01.2021";
                var d2 = "01.01.2020";
                var result = ExpressionEvaluator.EvaluateExpression($"Years({d1} - {d2}) >= 1");

                Assert.IsTrue(result.Boolean);
            }


            [TestMethod()]
            public void WennDannTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression($"WENN 1 > 0 DANN 'Yes' SONST 'No'");

                Assert.AreEqual(result.Text, "Yes");
            }

            [TestMethod()]
            public void MoreMathTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression($" ((1 + 1) * 10 / (7 / 3.5)) ^ 2 / 10000");

                Assert.AreEqual(result.Number, 1);
            }


            [TestMethod()]
            public void VarUsageTest()
            {
                var values = new Dictionary<string, object>
            {
                { "FirstVar", 1.75 },
                { "SecondVar", 2 },
                { "ResultText", "Your Right!" }
            };

                var result = ExpressionEvaluator.EvaluateExpression($"IF FirstVar < SecondVar THEN ResultText", values);

                Assert.AreEqual(result.Text, values["ResultText"]);
            }

            [TestMethod()]
            public void IfThenDifferentCasesTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression($"IF 1 < 0 then 'Yes' Else 'No'");

                Assert.AreEqual(result.ToString(), "No");
            }

            [TestMethod()]
            public void ComplexTest()
            {
                var d1 = new DateTime(2021, 01, 01);
                var d2 = new DateTime(2020, 01, 01);
                var d3 = new DateTime(2020, 06, 01);

                var expression = @"IF " +
                    @" 1 > 0 " +
                    @" THEN " +
                    $@" (ROUND(YEARS({d1} - {d2})) + 5) / MONAT({d3}) + 1 " +
                    @"ELSE 1";


                var result = ExpressionEvaluator.EvaluateExpression(expression);

                Assert.AreEqual(result.ToString(), "2");
            }
        }
    }
}
