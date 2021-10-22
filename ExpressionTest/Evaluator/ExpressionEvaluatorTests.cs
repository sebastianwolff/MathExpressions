using Microsoft.VisualStudio.TestTools.UnitTesting;
using Expressionator.Expressions.Evaluator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressionator.Expressions.Builder;

namespace Expressionator.Expressions.Evaluator.Tests
{
    [TestClass()]
    public class ExpressionEvaluatorTests
    {
        [TestMethod()]
        public void AdditionNumberTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression("1+1");
            
            Assert.AreEqual(result.number, 2);
        }

        [TestMethod()]
        public void TextEqualTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression("'hans' = 'hans'");

            Assert.IsTrue(result.boolean);
        }


        [TestMethod()]
        public void CombineTextTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression("'hans'+'hans'");

            Assert.AreEqual(result.text, "hanshans");
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
            Assert.AreEqual(result.number, 0);

            result = ExpressionEvaluator.EvaluateExpression($"round({d2})");
            Assert.AreEqual(result.number, 1);

            result = ExpressionEvaluator.EvaluateExpression($"round({d3})");
            Assert.AreEqual(result.number, 0);

            result = ExpressionEvaluator.EvaluateExpression($"round1({d2})");
            Assert.AreEqual(result.number, 0.5);

            result = ExpressionEvaluator.EvaluateExpression($"round2({d2})");
            Assert.AreEqual(result.number, 0.51);


            result = ExpressionEvaluator.EvaluateExpression($"round3({d2})");
            Assert.AreEqual(result.number, 0.513);



        }

        [TestMethod()]
        public void SubstractDateTest()
        {
            var d1 = new DateTime(2020, 01, 01);
            var d2 = new DateTime(2019, 01, 01);
            var result = ExpressionEvaluator.EvaluateExpression($"{d1} - {d2}");

            Assert.AreEqual(result.dateRange.TotalDays,365);
        }

        [TestMethod()]
        public void DateYearTest()
        {
            var d1 = new DateTime(2021, 01, 01);
            var d2 = new DateTime(2020, 01, 01);
            var result = ExpressionEvaluator.EvaluateExpression($"Year({d1})");

            Assert.AreEqual(result.ToString(), "2021");
        }

        [TestMethod()]
        public void CombinedSubstractDateTest()
        {
            var d1 = new DateTime(2021, 01, 01);
            var d2 = new DateTime(2020, 01, 01);
            var result = ExpressionEvaluator.EvaluateExpression($"Years({d1} - {d2}) >= 1");

            Assert.IsTrue(result.boolean);
        }

        [TestMethod()]
        public void CombinedSubstractDateStringTest()
        {
            var d1 = "01.01.2021";
            var d2 = "01.01.2020";
            var result = ExpressionEvaluator.EvaluateExpression($"Years({d1} - {d2}) >= 1");

            Assert.IsTrue(result.boolean);
        }


        [TestMethod()]
        public void WennDannTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression($"WENN 1 > 0 DANN 'Yes' SONST 'No'");

            Assert.AreEqual(result.text , "Yes");
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