# MathExpressions
A simple Math Expression Parser / Interpreter

This library interprets mathematical expressions from text input. 

Basic functions:

- Math Operations [+] [-] [\\] [*] [^]
- String, Date, Number Comparsion [<] [<=] [>] [>=] [=] 
- Date calculations (Range)
- Extract Date parts (month, day, year)
- Conditional statement  IF THEN ELSE

Example:

## Simple Math
````C#

var result = ExpressionEvaluator.EvaluateExpression("1+1");

// result.number -> 2;
// result.ToString() -> "2"
            
````

## More Math
````C#

var result = ExpressionEvaluator.EvaluateExpression("((1 + 1) * 10 / (7 / 3.5)) ^ 2 / 10000");

// result.number -> 1;
// result.ToString() -> "1"
            
````

## If Then Else

```` C#
 var result = ExpressionEvaluator.EvaluateExpression($"IF 1 > 0 THEN 'Yes, it´s true!' ELSE 'No! Your wrong ..'");
// result.ToString() -> "Yes, it´s true!");
````

## String Comparsion
````C#

var result = ExpressionEvaluator.EvaluateExpression("'MyString' = 'MyString'");
// result.boolean ==  True;

````

## Date Calculation

```` C#
var d1 = new DateTime(2020, 01, 01);
var d2 = new DateTime(2019, 01, 01);
var result = ExpressionEvaluator.EvaluateExpression($"{d1} - {d2}");
// result.dateRange.TotalDays -> 365
````

## Date Parts
````C#
var d1 = new DateTime(2021, 01, 01);
var result = ExpressionEvaluator.EvaluateExpression($"Year({d1})");

// result.ToString() -> "2021";
            
````

## Using Variables 

````C#
var values = new Dictionary<string, object>
            {
                { "FirstVar", 1.75 },
                { "SecondVar", 2 },
                { "ResultText", "Your Right!" }
            };

var result = ExpressionEvaluator.EvaluateExpression($"IF FirstVar < SecondVar THEN ResultText", values);
// result.ToString() -> "Your Right!"

````


