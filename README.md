# MathExpressions
A simple Math Expression Parser / Interpreter

This library interprets mathematical expressions from text input. 

Basic functions:

- Math operations [+] [-] [\\] [*] [^]
- Comparsion: [<] [<=] [>] [>=] [=] 
- Text comparisons
- Date calculations (Range)
- Extract date components (month, day, year)

Example:

## Simple Math
````C#

var result = ExpressionEvaluator.EvaluateExpression("1+1");

// result.number -> 2;
// result.ToString() -> "2"
            
````

## String Comparsion
````C#

var result = ExpressionEvaluator.EvaluateExpression("'hans' = 'hans'");
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
