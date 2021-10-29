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
        private static CultureInfo de ;
        private static CultureInfo en ;
        private static CultureInfo gb ;

        public MathExpressionCastTests()
        {
            de = new("de-DE");
            en = new("en-EN");
            gb = new("en-GB");
            Console.WriteLine("Huhu");
        }

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
            var resultDouble = ExpressionEvaluator.EvaluateExpression<double>("1,5 + 1,487", de);
            Assert.AreEqual(resultDouble.GetType(), typeof(double));

        }


        [TestMethod]
        public void DateTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021", de);
            Assert.AreEqual(result, new DateTime(2021, 01, 01));
    
        }

        [TestMethod]
        public void DateDETest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021", de);
            Assert.AreEqual(result, new DateTime(2021, 01, 01));
     
        }

        [TestMethod]
        public void DateGBTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("24/1/2021", gb);
            Assert.AreEqual(result, new DateTime(2021, 01, 24));

        }


        [TestMethod]
        public void DateUSTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01/24/2021", en);
            Assert.AreEqual(result, new DateTime(2021, 01, 24));
  
        }

        [TestMethod]
        public void DateUS2Test()
        {
            var result = ExpressionEvaluator.EvaluateExpression("1/24/2021", en);
            Assert.AreEqual(result.Date, new DateTime(2021,1,24));
          
        }

        [TestMethod]
        public void NoDateTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression("100 /50/2", en);
            Assert.AreEqual(result.Number, 1);

        }

        [TestMethod]
        public void NoDate2Test()
        {
            var result = ExpressionEvaluator.EvaluateExpression("24/1/2001", de);
            Assert.AreEqual(result.Number, (24d/1d/2001d));

        }

        [TestMethod]
        public void DateUDeShortTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression("1.2.2021", de);
            Assert.AreEqual(result.Date, new DateTime(2021, 2, 1));

        }

        [TestMethod]
        public void DateAddMonthsTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021 + Months(5)", de);
            Assert.AreEqual(result, new DateTime(2021, 05, 31));
       
        }

        [TestMethod]
        public void DateAddMonths2Test()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("1.1.2021 + Months(5)", de);
            Assert.AreEqual(result, new DateTime(2021, 05, 31));
        }

        [TestMethod]
        public void DateBaseTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("23.10.4567", de);
            Assert.AreEqual(result, new DateTime(4567, 10, 23));
 
            var resulten = ExpressionEvaluator.EvaluateExpression<DateTime>("10/23/4567", en);
            Assert.AreEqual(resulten, new DateTime(4567, 10, 23));
            

        }

        [TestMethod]
        public void TimeTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01:23", de);
            Assert.AreEqual(result.Hour, new DateTime(0001, 01, 01, 1,23,0).Hour );
            Assert.AreEqual(result.Minute, new DateTime(0001, 01, 01, 1, 23, 0).Minute);

        }


        [TestMethod]
        public void TimeSubstractionTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("02:00 - 01:00", de);
            Assert.AreEqual(result.TimeSpan.TotalMinutes, 60);

        }

        [TestMethod]
        public void TimeAdditionTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("02:10:05 + 01:10:05", de);
            Assert.AreEqual(result.TimeSpan.Hours, 3);
            Assert.AreEqual(result.TimeSpan.Minutes, 20);
            Assert.AreEqual(result.TimeSpan.Seconds, 10);
        }

        [TestMethod]
        public void TimeAdditionOverflowTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("02:59:59 + 00:00:10", de);
            Assert.AreEqual(result.TimeSpan.Hours, 3);
            Assert.AreEqual(result.TimeSpan.Minutes, 0);
            Assert.AreEqual(result.TimeSpan.Seconds, 9);
        }

        [TestMethod]
        public void TimeDateAdditionTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("01.01.2021 - 01:00", de);
            Assert.AreEqual(result.TimeSpan.TotalMinutes, 60);
        }

        [TestMethod]
        public void TimeDateAdditionCastTest()
        {
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
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("01.01.2020 - 01.01.2019", de);
            Assert.AreEqual(result.TotalDays, 365);
            Assert.AreEqual(result.TotalDays, 365);
        }

        [TestMethod]
        public void DateAddDaysTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021 + Days(5) + Tage(2)", de);
            Assert.AreEqual(result, new DateTime(2021, 01, 08));
        }

        [TestMethod]
        public void DateAddYearsTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021 + Years(5)", de);
            Assert.AreEqual(result, new DateTime(2025, 12, 31));
        }

        [TestMethod]
        public void DateRangeTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<DateRange>("01.01.2021 - 01.01.2020", de);
            Assert.AreEqual(result, new DateRange(new DateTime(2020, 01, 01), new DateTime(2021, 01, 01)));
        }

        [TestMethod]
        public void BooleanTest()
        {
            var result = ExpressionEvaluator.EvaluateExpression<Boolean>("1 > 0");
            Assert.AreEqual(result, true);
        }

    }
}
