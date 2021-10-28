using Expressionator;
using Expressionator.Expressions.Builder;
using Expressionator.Expressions.Evaluator;
using Expressionator.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace MathExpressinTests
{
    [TestClass]
    public class ExpressoinBuillderTests
    {

        [TestMethod]
        public void IsIntDateStopTest()
        {
            var en = new CultureInfo("en-US");
            var de = new CultureInfo("de-DE");
            var ebEN = new ExpressionBuilder(en);
            var ebDE = new ExpressionBuilder(de);

            Assert.IsFalse(ebEN.IsIntDateStopToken('.', false)); // Expected as Decimal Sep.
            Assert.IsFalse(ebDE.IsIntDateStopToken('.', false)); // Expected by Dates
            Assert.IsFalse(ebEN.IsIntDateStopToken('/', true)); // Expected by Dates
           
            Assert.IsTrue(ebEN.IsIntDateStopToken('/', false)); // Expected by Dates
            Assert.IsTrue(ebDE.IsIntDateStopToken('/', true)); // Not Expected 

        }

    }
}
