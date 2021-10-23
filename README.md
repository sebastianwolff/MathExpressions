# MathExpressions
A simple .NET 5 Math Expression Parser / Interpreter.

This library interprets mathematical expressions from text input. 

Basic functions:

- Math Operations [+] [-] [\\] [*] [^]
- String, Date, Number Comparsion [<] [<=] [>] [>=] [=] 
- Date calculations (Range)
- Extract Date parts (month, day, year)
- Conditional statement  IF THEN ELSE

## Examples

### Simple Math
````C#

var result = ExpressionEvaluator.EvaluateExpression("1+1");
____________________________________
// result.number -> 2;
// result.ToString() -> "2"
            
````

### More Math
````C#

var result = ExpressionEvaluator.EvaluateExpression("((1 + 1) * 10 / (7 / 3.5)) ^ 2 / 10000");
____________________________________
// result.number -> 1;
// result.ToString() -> "1"
            
````

### If Then Else

```` C#
 var result = ExpressionEvaluator.EvaluateExpression($"IF 1 > 0 THEN 'Yes, it´s true!' ELSE 'No! Your wrong ..'");
____________________________________
// result.ToString() -> "Yes, it´s true!");
````

### If Then Else (AND/OR)

```` C#
 var result = ExpressionEvaluator.EvaluateExpression($"IF (1 > 0 AND 'Yes' != 'No') OR 100/10=10 THEN 'Yes, it´s true!' ELSE 'No! Your wrong ..'");
____________________________________
// result.ToString() -> "Yes, it´s true!");
````


### String Comparsion
````C#

var result = ExpressionEvaluator.EvaluateExpression("'MyString' = 'MyString'");
____________________________________
// result.boolean ==  True;

````

### Date Calculation

```` C#
var d1 = new DateTime(2020, 01, 01);
var d2 = new DateTime(2019, 01, 01);
var result = ExpressionEvaluator.EvaluateExpression($"{d1} - {d2}");
____________________________________
// result.dateRange.TotalDays -> 365
````

````C#
var d1 = new DateTime(2020, 01, 01);
var d2 = new DateTime(2019, 01, 01);
var result = ExpressionEvaluator.EvaluateExpression($"days({d1} - {d2})");
____________________________________
// result.number -> 365;
````

### Date with strings Calculation

```` C#
var d1 = "01.01.2020";
var d2 = "01.01.2019";
var result = ExpressionEvaluator.EvaluateExpression($"{d1} - {d2}");
____________________________________
// result.dateRange.TotalDays -> 365
````

### Date Parts
````C#
var d1 = new DateTime(2021, 01, 01);
var result = ExpressionEvaluator.EvaluateExpression($"Year({d1})");
____________________________________
// result.ToString() -> "2021";
            
````

### Between Date
````C#
var d1 = "01.01.2020";
var d2 = "31.12.2020";
var d3 = "15.06.2020";
var result = ExpressionEvaluator.EvaluateExpression($"{d3} BETWEEN {d1} AND {d2}");
____________________________________
// result.boolean -> True;
````


### Between Numbers
````C#
var d1 = 1;
var d2 = 10;
var d3 = 5;
var result = ExpressionEvaluator.EvaluateExpression($"{d3} BETWEEN {d1} AND {d2}");
____________________________________
// result.boolean -> True;
````

### Using Variables 

````C#
var values = new Dictionary<string, object>
            {
                { "FirstVar", 1.75 },
                { "SecondVar", 2 },
                { "ResultText", "Your Right!" }
            };

var result = ExpressionEvaluator.EvaluateExpression($"IF FirstVar < SecondVar THEN ResultText", values);
____________________________________
// result.ToString() -> "Your Right!"

````


## Special Remarks

Special Shout-Out to Christian Parpart as initial Author - a long Time ago!


