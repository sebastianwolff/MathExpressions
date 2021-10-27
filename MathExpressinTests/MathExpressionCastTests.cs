using Expressionator.Expressions.Builder;
using Expressionator.Expressions.Evaluator;
using Expressionator.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MathExpressinTests
{
    [TestClass]
    public class MathExpressionCastTests
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
            var resultDouble = ExpressionEvaluator.EvaluateExpression<double>("1,5 + 1,487", new CultureInfo("de-DE"));
            Assert.AreEqual(resultDouble.GetType(), typeof(double));

        }


        [TestMethod]
        public void DateTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021", new CultureInfo("de-DE"));
            Assert.AreEqual(result, new DateTime(2021, 01, 01));
            Assert.AreEqual(result.GetType(), typeof(DateTime));

        }

        [TestMethod]
        public void DateDETest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021", new CultureInfo("de-DE"));
            Assert.AreEqual(result, new DateTime(2021, 01, 01));
            Assert.AreEqual(result.GetType(), typeof(DateTime));

        }

        [TestMethod]
        public void DateGBTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("24/01/2021", new CultureInfo("en-GB"));
            Assert.AreEqual(result, new DateTime(2021, 01, 24));
            Assert.AreEqual(result.GetType(), typeof(DateTime));

        }

        [TestMethod]
        public void DateUSTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01/24/2021", new CultureInfo("en-EN"));
            Assert.AreEqual(result, new DateTime(2021, 01, 24));
            Assert.AreEqual(result.GetType(), typeof(DateTime));

        }

        [TestMethod]
        public void DateUS2Test()
        {
            var result = ExpressionEvaluator.EvaluateExpression("1/24/2021", new CultureInfo("en-EN"));
            Assert.AreEqual(result.Number, (1d/24d/2021d));
          
        }


        [TestMethod]
        public void DateAddMonthsTest()
        {
            var de = new CultureInfo("de-DE");
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021 + Months(5)", de);
            Assert.AreEqual(result, new DateTime(2021, 05, 31));
            Assert.AreEqual(result.GetType(), typeof(DateTime));

        }

        [TestMethod]
        public void DateBaseTest()
        {
            var de = new CultureInfo("de-DE");
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("23.10.4567", de);
            Assert.AreEqual(result, new DateTime(4567, 10, 23));
            Assert.AreEqual(result.GetType(), typeof(DateTime));

            var en = new CultureInfo("en-EN");
            var resulten = ExpressionEvaluator.EvaluateExpression<DateTime>("10/23/4567", en);
            Assert.AreEqual(resulten, new DateTime(4567, 10, 23));
            Assert.AreEqual(resulten.GetType(), typeof(DateTime));


        }

        [TestMethod]
        public void TimeTest()
        {
            var de = new CultureInfo("de-DE");
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01:23", de);
            Assert.AreEqual(result.Hour, new DateTime(0001, 01, 01, 1,23,0).Hour );
            Assert.AreEqual(result.Minute, new DateTime(0001, 01, 01, 1, 23, 0).Minute);

            Assert.AreEqual(result.GetType(), typeof(DateTime));


        }


        [TestMethod]
        public void TimeSubstractionTest()
        {
            var de = new CultureInfo("de-DE");
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("02:00 - 01:00", de);
            Assert.AreEqual(result.TimeSpan.TotalMinutes, 60);
            
            Assert.AreEqual(result.GetType(), typeof(DateRange));


        }

        [TestMethod]
        public void TimeAdditionTest()
        {
            var de = new CultureInfo("de-DE");
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("02:10:05 + 01:10:05", de);
            Assert.AreEqual(result.TimeSpan.Hours, 3);
            Assert.AreEqual(result.TimeSpan.Minutes, 20);
            Assert.AreEqual(result.TimeSpan.Seconds, 10);

            Assert.AreEqual(result.GetType(), typeof(DateRange));


        }

        [TestMethod]
        public void TimeAdditionOverflowTest()
        {
            var de = new CultureInfo("de-DE");
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("02:59:59 + 00:00:10", de);
            Assert.AreEqual(result.TimeSpan.Hours, 3);
            Assert.AreEqual(result.TimeSpan.Minutes, 0);
            Assert.AreEqual(result.TimeSpan.Seconds, 9);

            Assert.AreEqual(result.GetType(), typeof(DateRange));


        }

        [TestMethod]
        public void TimeDateAdditionTest()
        {
            var de = new CultureInfo("de-DE");
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("01.01.2021 - 01:00", de);
            Assert.AreEqual(result.TimeSpan.TotalMinutes, 60);

            Assert.AreEqual(result.GetType(), typeof(DateRange));
        }

        [TestMethod]
        public void TimeDateAdditionCastTest()
        {
            var de = new CultureInfo("de-DE");
            var result = ExpressionEvaluator.EvaluateExpression<double>("Hours(02:00 - 01:00)", de);
            Assert.AreEqual(result, 1);
            result = ExpressionEvaluator.EvaluateExpression<double>("Minutes(01.01.2021 - 01:00)", de);
            Assert.AreEqual(result, 60);
            result = ExpressionEvaluator.EvaluateExpression<double>("Seconds(01.01.2021 - 01:00)", de);
            Assert.AreEqual(result, 3600);

        }

        [TestMethod]
        public void DateAdditionTest()
        {
            var de = new CultureInfo("de-DE");
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("01.01.2020 - 01.01.2019", de);
            Assert.AreEqual(result.TotalDays, 365);
            Assert.AreEqual(result.TotalDays, 365);

            Assert.AreEqual(result.GetType(), typeof(DateRange));


        }

        [TestMethod]
        public void DateAddDaysTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021 + Days(5)", new CultureInfo("de-DE"));
            Assert.AreEqual(result, new DateTime(2021, 01, 06));
            Assert.AreEqual(result.GetType(), typeof(DateTime));

        }

        [TestMethod]
        public void DateAddYearsTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021 + Years(5)", new CultureInfo("de-DE"));
            Assert.AreEqual(result, new DateTime(2025, 12, 31));
            Assert.AreEqual(result.GetType(), typeof(DateTime));

        }

        [TestMethod]
        public void DateRangeTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("01.01.2021 - 01.01.2020", new CultureInfo("de-DE"));
            Assert.AreEqual(result, new DateRange(new DateTime(2020, 01, 01), new DateTime(2021, 01, 01)));
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
