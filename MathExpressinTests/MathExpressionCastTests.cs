using Expressionator.Expressions.Builder;
using Expressionator.Expressions.Evaluator;
using Expressionator.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MathExpressinTests
{
    [TestClass]
    public class MathExpressionCastTests
    {
        [TestClass()]
        public class ExpressionEvaluatorTests
        {

            [TestMethod()]
            public void IntTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression<int>("1+1");
                Assert.AreEqual(result, 2);
                Assert.AreEqual(result.GetType(), typeof(int));

            }

            [TestMethod]
            public void DoubleTest()
            {
                var resultDouble = ExpressionEvaluator.EvaluateExpression<double>("1,5 + 1,487");
                Assert.AreEqual(resultDouble, 2.987);
                Assert.AreEqual(resultDouble.GetType(), typeof(double));

            }


            [TestMethod]
            public void DateTest()
            {
                var result= ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021");
                Assert.AreEqual(result, new DateTime(2021,01,01));
                Assert.AreEqual(result.GetType(), typeof(DateTime));

            }

            [TestMethod]
            public void DateAddMonthsTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021 + Months(5)");
                Assert.AreEqual(result, new DateTime(2021, 05, 31));
                Assert.AreEqual(result.GetType(), typeof(DateTime));

            }

            [TestMethod]
            public void DateAddDaysTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021 + Days(5)");
                Assert.AreEqual(result, new DateTime(2021, 01, 06));
                Assert.AreEqual(result.GetType(), typeof(DateTime));

            }

            [TestMethod]
            public void DateAddYearsTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021 + Years(5)");
                Assert.AreEqual(result, new DateTime(2025, 12, 31));
                Assert.AreEqual(result.GetType(), typeof(DateTime));

            }

            [TestMethod]
            public void DateRangeTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression<DateRange>("01.01.2021 - 01.01.2020");
                Assert.AreEqual(result, new DateRange(new DateTime(2020,01,01), new DateTime(2021,01,01)));
                Assert.AreEqual(result.GetType(), typeof(DateRange));

            }

            [TestMethod]
            public void BooleanTest()
            {
                var result = ExpressionEvaluator.EvaluateExpression<Boolean>("1 > 0");
                Assert.AreEqual(result, true);
                Assert.AreEqual(result.GetType(), typeof(Boolean));

            }

        }
    }
}
