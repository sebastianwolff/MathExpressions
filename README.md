# MathExpression.net
A simple .NET 5 Math Expression Parser / Interpreter.

This library interprets mathematical expressions from text input. 

## Important Note on Localization

**Before Version 1.1.xx**
> The Lib expects German localized input Strings (e.g. for Numbers:  Use 2,5 instead 2.5 and Dates in Format dd.mm.yyy )
> Will be corrected/extended in one of the next versions

**Since Version 1.1.xx**
> Localization added. See Chapter **Localization**


### Basic functions

- Math Operations [+] [-] [\\] [*] [^]
- String, Date, Number Comparsion [<] [<=] [>] [>=] [=] 
- Date calculations (Range)
- Extract Date parts (month, day, year)
- Conditional statement  IF THEN ELSE
- Date and Floating Value Localization

## Installation

Get the current Version from NuGet or build it from this GitHub Source

[MathExpression.net on nuget.org](https://www.nuget.org/packages/MathExpression.net/)

## Localization

Localization is supported since version 1.1.xx. The default localization is invariant. To set your preferred localization for floating point numbers and date strings, add a CultureInfo as the last parameter to the EvaluateExpression call.

### Example
````C#

var deCulture = new CultureInfo("de-DE");
var result = ExpressionEvaluator.EvaluateExpression<DateTime>("01.01.2021", deCulture);
            
````

## Basic Math

All Basic Math Operators are supported. 

 \+ = Addition

 \- = Subtraction
 
 \\ = Dividing
 
 \* = Multiplication  
 
 \^ = Exponentiation 

### Examples

#### Simple Math
````C#

var result = ExpressionEvaluator.EvaluateExpression("1+1");
____________________________________
// result.number -> 2;
// result.ToString() -> "2"
            
````

#### More Math
````C#

var result = ExpressionEvaluator.EvaluateExpression("((1 + 1) * 10 / (7 / 3.5)) ^ 2 / 10000");
____________________________________
// result.number -> 1;
// result.ToString() -> "1"
            
````

## Conditions

You can define a conditional Expressions (IF->THEN->ELSE) for special purposes  

## Examples
#### If Then Else

```` C#
 var result = ExpressionEvaluator.EvaluateExpression($"IF 1 > 0 THEN 'Yes, it´s true!' ELSE 'No! Your wrong ..'");
____________________________________
// result.ToString() -> "Yes, it´s true!");
````

#### If Then Else (AND/OR)

```` C#
 var result = ExpressionEvaluator.EvaluateExpression($"IF (1 > 0 AND 'Yes' != 'No') OR 100/10=10 THEN 'Yes, it´s true!' ELSE 'No! Your wrong ..'");
____________________________________
// result.ToString() -> "Yes, it´s true!");
````

## Strings / Text

Text can be compared or merged. 

**Possible Opperands:**

\+ : Concat Text

\= : Text Equals 

### Examples
#### String Compare
````C#

var result = ExpressionEvaluator.EvaluateExpression("'MyString' = 'MyString'");
____________________________________
// result.boolean ==  True;

````

#### String Concat
````C#

var result = ExpressionEvaluator.EvaluateExpression("'MyString' + 'MyString'");
____________________________________
// result.text ==  "MyStringMyString";

````

## Date

There are a number of date functions.
- Date Substraction
- Date Comparsion (=, >, >=, <, <=)

Also some Conversion- and Datepartextraction functions for results from date operations are available.

**Partial Result**
- days(date1 - date2) -> Gets the number of Days between to Dates
- months(date1 - date2) -> Gets the number of month between to Dates
- years(date1 - date2) -> Gets the number of Years between to Dates

**Dateparts:**
- day(date1) -> Gets the Day of the given Date
- month(date1) -> Gets the Month of the given Date
- year(date1) -> Gets the Year of the given Date


### Examples
#### Date Calculation

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

#### Date from string Calculation

```` C#
var d1 = "01.01.2020";
var d2 = "01.01.2019";
var result = ExpressionEvaluator.EvaluateExpression($"{d1} - {d2}");
____________________________________
// result.dateRange.TotalDays -> 365
````

#### Date Parts
````C#
var d1 = new DateTime(2021, 01, 01);
var result = ExpressionEvaluator.EvaluateExpression($"Year({d1})");
____________________________________
// result.ToString() -> "2021";
            
````

### Between 

You can check for dates and numbers if they are in the range of an given pair

**Syntax:** 

[MyValue] BETWEEN [RangeStart_Value] AND [RangeEnd_Value]

#### Between Date
````C#
var d1 = "01.01.2020";
var d2 = "31.12.2020";
var d3 = "15.06.2020";
var result = ExpressionEvaluator.EvaluateExpression($"{d3} BETWEEN {d1} AND {d2}");
____________________________________
// result.boolean -> True;
````


#### Between Numbers
````C#
var d1 = 1;
var d2 = 10;
var d3 = 5;
var result = ExpressionEvaluator.EvaluateExpression($"{d3} BETWEEN {d1} AND {d2}");
____________________________________
// result.boolean -> True;
````

### Variables

You can pass variables to the parser to work with them inside the expression. 

#### Using Variables 

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

### Generic Type Result

### Typesave Results

Instead of getting a Result object as the result of EvaluateExpression, you can use the generic type T and get the concrete result of the expression.

Supported Types:

- int
- string
- boolean
- double
- DateTime
- DateRange
- decimal


#### Get Typed Result

````C#


int result = ExpressionEvaluator.EvaluateExpression<int>($"1 + 1 ");

____________________________________
// int result -> 2

````

````C#


int result = ExpressionEvaluator.EvaluateExpression<string>($"1 + 1 ");

____________________________________
// string result -> "2"

````


## Rounding

To round results, you can use the Keeyword "round" followed by the number of decimals

**Examples:**

round0(1.2336) -> 1

round1(1.2336) -> 1.2

round2(1.2336) -> 1.23

round3(1.2336) -> 1.234 


````C#


int result = ExpressionEvaluator.EvaluateExpression<double>($"round2(10 / 3)");

____________________________________
// double result -> 3.33

````

## Special Remarks

Special Shout-Out to Christian Parpart as initial Author - a long Time ago!


