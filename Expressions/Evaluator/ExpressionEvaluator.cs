using System;
using System.Collections.Generic;

using Expressionator.Utils;
using Expressionator.Expressions.Builder;

namespace Expressionator.Expressions.Evaluator
{
	/// <summary>
	/// The expression-evaluator is being used for evaluating mathematical expressions.
	/// In fact, it's the main API you gonna use when you need to handle 
	/// dynamic mathematical expressions in your application.
	/// 
	/// The static methods are for convenience. Use them if you can,
	/// and instanciate ExpressionEvaluator only if you have to.
	/// </summary>
	public class ExpressionEvaluator : INodeVisitor
	{
		#region public class Result
		public class Result
		{
			#region types
			public enum Type
			{
				Boolean,
				Number,
				Date,
				TimeSpan,
				Text,
                TextRange,
				DateRange,
				NumberRange,
				TimeSpanRange
			}
			#endregion

			#region data
			private Type _type;

			private double _number;
			private bool _boolean;
			private DateTime _date;
			private TimeSpan _timeSpan;
			private DateRange _dateRange;
			private string _text;
			private NumberRange<double> _numberRange;
			private KeyValuePair<TimeSpan, TimeSpan> _timeSpanRange;
			#endregion

			#region property accessors
			public Type type
			{
				get { return _type; }
			}

			public bool boolean
			{
				get 
				{
					AssumeType(Type.Boolean);
					return _boolean;
				}
			}

			public double number
			{
				get
				{
					AssumeType(Type.Number);
					return _number;
				}
			}

			public DateTime date
			{
				get
				{
					AssumeType(Type.Date);
					return _date.Date;
				}
			}

			public TimeSpan timeSpan
			{
				get
				{
					AssumeType(Type.TimeSpan);
					return _timeSpan;
				}
			}

			public string text {
				get {
					AssumeType(Type.Text);
					return _text;
				}
			}

			public DateRange dateRange
			{
				get
				{
					AssumeType(Type.DateRange);
					return _dateRange;
				}
			}

			public NumberRange<double> numberRange
			{
				get
				{
					AssumeType(Type.NumberRange);
					return _numberRange;
				}
			}

			public KeyValuePair<TimeSpan, TimeSpan> timeSpanRange
			{
				get
				{
					AssumeType(Type.TimeSpanRange);
					return _timeSpanRange;
				}
			}
			#endregion

			#region constructors
			public Result(bool boolean)
			{
				_type = Type.Boolean;
				_boolean = boolean;
			}

			public Result(double number)
			{
				_type = Type.Number;
				_number = number;
			}

			public Result(DateTime date)
			{
				_type = Type.Date;
				_date = date;
			}

			public Result(TimeSpan timeSpan)
			{
				_type = Type.TimeSpan;
				_timeSpan = timeSpan;
			}

			public Result(DateRange dateRange)
			{
				_type = Type.DateRange;
				_dateRange = dateRange;
			}

			public Result(DateTime begin, DateTime end)
			{
				_type = Type.DateRange;
				_dateRange = new DateRange(begin, end);
			}

			public Result(NumberRange<double> numberRange)
			{
				_type = Type.NumberRange;
				_numberRange = numberRange;
			}

			public Result(double low, double high)
			{
				_type = Type.NumberRange;
				_numberRange = new NumberRange<double>(low, high);
			}

			public Result(TimeSpan low, TimeSpan high)
			{
				_type = Type.TimeSpanRange;
                _timeSpanRange = new KeyValuePair<TimeSpan, TimeSpan>(low, high);
                //_timeSpanRange.Key = low;
                //_timeSpanRange.Value = high;
			}

			public Result(string text) {
				_type = Type.Text;
				_text = text;
			}
			#endregion

			private void AssumeType(Type assumedType)
			{
				if (type != assumedType)
				{
					throw new InvalidCastException(String.Format(
						"Unexpected result type {0}. Expected {1}.",
						type.ToString(), assumedType.ToString()
					));
				}
			}

			public override string ToString()
			{
				switch (type)
				{
					case Type.Boolean:
						return boolean.ToString();
					case Type.Date:
						return date.ToString();
					case Type.DateRange:
						return String.Format("DATERANGE({0})", dateRange);
					case Type.Number:
						return number.ToString();
					case Type.NumberRange:
						return String.Format("NUMBERRANGE({0})", numberRange);
					case Type.TimeSpan:
						return String.Format("TIMESPAN({0})", timeSpan);
					case Type.TimeSpanRange:
						return String.Format("TIMESPAN({0}) .. TIMESPAN({1})", timeSpanRange.Key, timeSpanRange.Value);
					case Type.Text:
						return _text;
					default:
						throw new Exception("Internal error: Unhandled enum value.");
				}
			}
		}
		#endregion

		/// <summary>
		/// this delegate is used for on-demand variable evaluation.
		/// </summary>
		/// <param name="node">the variable-node to evaluate</param>
		/// <returns>the respective value</returns>
		public delegate Result EvaluateVariableDelegate(VariableNode node);

		/// <summary>
		/// this delegate gets invoked on evaluating variable nodes to return their respective value.
		/// </summary>
		public EvaluateVariableDelegate evaluateVariable;

		/// <summary>
		/// contains the contextual result of all visitor() methods being actually invoked.
		/// </summary>
		private Result result;

		/// <summary>
		/// this variable-map is being used when the user-application passed us a fixed variable-map
		/// to use for resolving variables while evaluating.
		/// </summary>
		private Dictionary<string, Node> variableMap;

		// -------------------------------------------------------------------------------------------

		/// <summary>
		/// Evaluates a given mathematical expression (without taking care of variables!)
		/// </summary>
		/// <param name="expression">the mathematical expression to evaluate</param>
		/// <returns>the evaluated result value</returns>
		public static Result EvaluateExpression(string expression)
		{
			return EvaluateExpression(ExpressionBuilder.ParseExpression(expression), (EvaluateVariableDelegate)null);
		}

		public static T EvaluateExpression<T>(string expression, Dictionary<string, object> variables = null)
        {
			object result;
			var evalResult = (variables != null) 
				? EvaluateExpression(expression, variables)
				: EvaluateExpression(expression);

			if (typeof(T) == typeof(DateRange))
			{
				result = evalResult.dateRange;
            }
            else
            {
				switch (Type.GetTypeCode(typeof(T)))
				{
					case TypeCode.String:
						result = evalResult.ToString();
						break;
					case TypeCode.Int32:
					case TypeCode.Int16:
						result = (T)(object)Convert.ToInt32(evalResult.number);
						break;
					case TypeCode.Double:
						result = evalResult.number;
						break;
					case TypeCode.Decimal:
						result = (decimal)evalResult.number;
						break;
					case TypeCode.Boolean:
						result = evalResult.boolean;
						break;
					case TypeCode.DateTime:
						result = evalResult.date;
						break;
					default:
						throw new NotSupportedException($"{typeof(T)} is not not supported yet");
						
				}
            }

			return (T)result;
		}

		/// <summary>
		/// a little abbreviation method for quickly evaluating a parametrized expression evaluation.
		/// </summary>
		/// <param name="expression">the mathematical expression to evaluate</param>
		/// <param name="variableMap">contains a set of variables (name-value pairs) that are being referenced when they occur within the given expression</param>
		/// <returns>the respective evaluated result value</returns>
		public static Result EvaluateExpression(string expression, Dictionary<string, string> variableMap)
		{
			Dictionary<string, Node> vars = new Dictionary<string, Node>();

			foreach (KeyValuePair<string, string> pair in variableMap)
				vars[pair.Key] = ExpressionBuilder.ParseExpression(pair.Value);

			return EvaluateExpression(expression, vars);
		}

		public static Result EvaluateExpression(string expression, IDictionary<string, object> variables)
		{
			Dictionary<string, Node> vars = new Dictionary<string, Node>();

			foreach (KeyValuePair<string, object> variable in variables)
				vars[variable.Key] = CreateNode(variable.Value);

			return EvaluateExpression(expression, vars);
		}

		private static Node CreateNode(object value) 
		{
			if (value is Node)
				return (Node)value; // TODO: use ExpressionCloner.Clone(value);

			if (value is DateTime)
				return new DateExpr((DateTime)value);

			if (value is string)
				return new TextNode((string)value);

			return ExpressionBuilder.ParseExpression(value.ToString());
		}

		/// <summary>
		/// Evaluates a given mathematical expression and automatically resolves all 
		/// variables as defined in the given variable-map.
		/// </summary>
		/// <param name="expression">the mathematical expression to evaluate</param>
		/// <param name="variableMap">contains a set of variables (name-value pairs) that are being referenced when they occur within the given expression</param>
		/// <returns>the respective evaluated result value</returns>
		public static Result EvaluateExpression(string expression, Dictionary<string, Node> variableMap)
		{
			return EvaluateExpression(ExpressionBuilder.ParseExpression(expression), variableMap);
		}

		/// <summary>
		/// Evaluates a given mathematical expression resolving all variables by invoking given delegate 
		/// to take care of resolving them.
		/// </summary>
		/// <param name="expression">the mathematical expression to evaluate</param>
		/// <param name="evalVar">the delegate to invoke when a variable needs to be resolved into its value</param>
		/// <returns></returns>
		public static Result EvaluateExpression(string expression, EvaluateVariableDelegate evalVar)
		{
			return EvaluateExpression(ExpressionBuilder.ParseExpression(expression), evalVar);
		}

		// -------------------------------------------------------------------------------------------

		/// <summary>
		/// Evaluates a given mathematical expression (without taking care of variables!)
		/// </summary>
		/// <param name="expression">the mathematical expression to evaluate</param>
		/// <returns>the evaluated result value</returns>
		public static Result EvaluateExpression(Node expression)
		{
			return EvaluateExpression(expression, (EvaluateVariableDelegate)null);
		}

		/// <summary>
		/// Evaluates a given mathematical expression and automatically resolves all 
		/// variables as defined in the given variable-map.
		/// </summary>
		/// <param name="expression">the mathematical expression to evaluate</param>
		/// <param name="variableMap">contains a set of variables (name-value pairs) that are being referenced when they occur within the given expression</param>
		/// <returns>the respective evaluated result value</returns>
		public static Result EvaluateExpression(Node expression, Dictionary<string, Result> variableMap)
		{
			Dictionary<string, Node> variableNodeMap = new Dictionary<string, Node>(variableMap.Count);

			foreach (KeyValuePair<string, Result> current in variableMap)
			{
				switch (current.Value.type)
				{
					case Result.Type.Boolean:
						variableNodeMap[current.Key] = new NumberNode(1);
						break;
					case Result.Type.Date:
						variableNodeMap[current.Key] = new DateExpr(current.Value.date);
						break;
					case Result.Type.DateRange:
						variableNodeMap[current.Key] = new RangeNode(
							new DateExpr(current.Value.dateRange.Begin),
							new DateExpr(current.Value.dateRange.End)
						);
						break;
					case Result.Type.Number:
						variableNodeMap[current.Key] = new NumberNode(current.Value.number);
						break;
					case Result.Type.NumberRange:
						variableNodeMap[current.Key] = new RangeNode(
							new NumberNode(current.Value.numberRange.Min),
							new NumberNode(current.Value.numberRange.Max)
						);
						break;
					case Result.Type.TimeSpan:
						variableNodeMap[current.Key] = new NumberNode(current.Value.timeSpan.Ticks);
						break;
					default:
						throw new Exception("Internal Error: Unhandled enum type.");
				}
			}

			return EvaluateExpression(expression, variableNodeMap, 1);
		}

		/// <summary>
		/// Evaluates a given mathematical expression and automatically resolves all 
		/// variables as defined in the given variable-map.
		/// </summary>
		/// <param name="expression">the mathematical expression to evaluate</param>
		/// <param name="variableMap">contains a set of variables (name-value pairs) that are being referenced when they occur within the given expression</param>
		/// <returns>the respective evaluated result value</returns>
		public static Result EvaluateExpression(Node expression, Dictionary<string, Node> variableMap)
		{
			return EvaluateExpression(expression, variableMap, 64);
		}

		/// <summary>
		/// Evaluates a given mathematical expression and automatically resolves all 
		/// variables as defined in the given variable-map.
		/// </summary>
		/// <param name="expression">the mathematical expression to evaluate</param>
		/// <param name="variableMap">contains a set of variables (name-value pairs) that are being referenced when they occur within the given expression</param>
		/// <param name="recursionLimit">sets the variable evaluation recursion limit. In case of exceeding it, an exception gets thrown</param>
		/// <returns>the respective evaluated result value</returns>
		public static Result EvaluateExpression(Node expression, Dictionary<string, Node> variableMap, int recursionLimit)
		{
			ExpressionEvaluator evaluator = new ExpressionEvaluator();

			evaluator.recursionDepth = 0;
			evaluator.recursionLimit = recursionLimit;

			evaluator.variableMap = variableMap;
			evaluator.evaluateVariable = new EvaluateVariableDelegate(evaluator.evaluateVariableFromMap);

			expression.accept(evaluator);

			return evaluator.result;
		}

		/// <summary>
		/// Evaluates a given mathematical expression resolving all variables by invoking given delegate 
		/// to take care of resolving them.
		/// </summary>
		/// <param name="expression">the mathematical expression to evaluate</param>
		/// <param name="evalVar">the delegate to invoke when a variable needs to be resolved into its value</param>
		/// <returns></returns>
		public static Result EvaluateExpression(Node expression, EvaluateVariableDelegate evalVar)
		{
			ExpressionEvaluator evaluator = new ExpressionEvaluator();
			evaluator.evaluateVariable = evalVar;

			expression.accept(evaluator);

			return evaluator.result;
		}

		// -------------------------------------------------------------------------------------------

		private int recursionDepth;
		private int recursionLimit;

		/// <summary>
		/// a little convenience helper method for resolving variable/const symbols to their respective values
		/// </summary>
		/// <param name="variableNode">the VariableNode instance to evaluate</param>
		/// <returns>their respective number representation</returns>
		private Result evaluateVariableFromMap(VariableNode variableNode)
		{
			if (variableMap.ContainsKey(variableNode.name))
			{
				if (++recursionDepth >= recursionLimit)
					throw new Exception(String.Format("Evaluation exceeds recursion limit of {0}", recursionLimit));

				return Evaluate(variableMap[variableNode.name]);
			}

			throw new Exception("Unresolved symbol '" + variableNode.name + "'");
		}

		// -------------------------------------------------------------------------------------------

		/// <summary>
		/// evaluates a compiled expression tree and returns its scalar result value
		/// </summary>
		/// <param name="node">the compiled expression tree</param>
		/// <returns>their respective scalar result value</returns>
		public Result Evaluate(Node node)
		{
			node.accept(this);
			return result;
		}

		#region INodeVisitor implementation
		public void visit(AddNode node)
		{
			// SEMANTICS:
			//     number + number
			//     date + timespan
			//     timespan + date
			//     timespan + timespan
			//     text + text
			//     text + number
			//     text + date
			//     text + timespan
			//     number + text
			//     date + text
			//     timespan + text

			//     (date + -timespan) := DATE
			//     (date + -date) := DATERANGE

			Result left = Evaluate(node.left);
			Result right = Evaluate(node.right);

			if (left.type == Result.Type.Text || right.type == Result.Type.Text) {
				result = new Result(left.ToString() + right.ToString());
				return;
			}

			switch (left.type)
			{
				case Result.Type.Date:
					// XXX: SPECIAL CASE: the difference of two dates is a timespan!
					if (node.right.GetType() == typeof(NegNode))
					{
						Result dateRight = Evaluate(((NegNode)node.right).subExpression);

						if (dateRight.type == Result.Type.Date)
						{
							result = left.date < dateRight.date
								? new Result(new DateRange(left.date, dateRight.date))
								: new Result(new DateRange(dateRight.date, left.date));

							break;
						}
					}

					if (right.type != Result.Type.TimeSpan)
						throw new Exception("Operator evaluation error: Invalid operand type combination.");

					result = new Result(left.date.Add(right.timeSpan));

					break;
				case Result.Type.Number:
					if (right.type != left.type)
						throw new Exception("Operator evaluation error: Invalid operand type combination.");

					result = new Result(left.number + right.number);

					break;
				case Result.Type.TimeSpan:
					switch (right.type)
					{
						case Result.Type.Date:
							result = new Result(right.date.Add(left.timeSpan));
							break;
						case Result.Type.TimeSpan:
							result = new Result(left.timeSpan.Add(right.timeSpan));
							break;
						default:
							throw new ExpressionEvaluationException("Incompatible operand types.");
					}
					break;
				default:
					throw new ExpressionEvaluationException("Incompatible operand types.");
			}
		}

		public void visit(NegNode node)
		{
			// SEMANTICS:
			//     - number
			//     - timespan

			Result operand = Evaluate(node.subExpression);

			switch (operand.type)
			{
				case Result.Type.Number:
					result = new Result(-operand.number);
					break;
				case Result.Type.TimeSpan:
					result = new Result(-operand.timeSpan);
					break;
				case Result.Type.Date:
					result = new Result(new TimeSpan(-operand.date.Ticks));
					break;
				default:
					throw new Exception("Operator evaluation error: Invalid operand type.");
			}
		}

		public void visit(MulNode node)
		{
			// SEMANTICS:
			//     number * number
			//     timespan * number
			//     number * timespan

			Result left = Evaluate(node.left);
			Result right = Evaluate(node.right);

			switch (left.type)
			{
				case Result.Type.Number:
					switch (right.type)
					{
						case Result.Type.Number:
							result = new Result(left.number * right.number);
							break;
						case Result.Type.TimeSpan:
							result = new Result(new TimeSpan((long)(left.number * right.timeSpan.Ticks)));
							break;
						default:
							throw new Exception("Operator evaluation error: Incompatible operand types.");
					}
					break;
				case Result.Type.TimeSpan:
					switch (right.type)
					{
						case Result.Type.Number:
							result = new Result(new TimeSpan((long)(left.timeSpan.Ticks * right.number)));
							break;
						default:
							throw new Exception("Operator evaluation error: Incompatible operand types.");
					}
					break;
				default:
					throw new Exception("Operator evaluation error: Incompatible operand types.");
			}
		}

		public void visit(DivNode node)
		{
			// SEMANTICS:
			//     number / number
			//     timespan / number

			Result left = Evaluate(node.left);
			Result right = Evaluate(node.right);

			switch (left.type)
			{
				case Result.Type.Number:
					if (right.type != Result.Type.Number)
						throw new Exception("Operator evaluation error: Incompatible operand types.");

					result = new Result(left.number / right.number);
					break;
				case Result.Type.TimeSpan:
					if (right.type != Result.Type.Number)
						throw new Exception("Operator evaluation error: Incompatible operand types.");

					result = new Result(new TimeSpan((long)(left.timeSpan.Ticks / right.number)));
					break;
				default:
					throw new Exception("Operator evaluation error: Incompatible operand types.");
			}
		}

		public void visit(NumberNode node)
		{
			result = new Result(node.value);
		}

		public void visit(TextNode node) {
			result = new Result(node.Value);
		}

		public void visit(PowNode node)
		{
			// SEMANTICS:
			//     number ^ number
			//     timespan ^ number

			Result left = Evaluate(node.powBase);
			Result right = Evaluate(node.exp);

			switch (left.type)
			{
				case Result.Type.Number:
					if (right.type != Result.Type.Number)
						throw new ExpressionEvaluationException("Invalid operand types.");

					result = new Result(System.Math.Pow(left.number, right.number));
					break;
				case Result.Type.TimeSpan:
					if (right.type != Result.Type.Number)
						throw new ExpressionEvaluationException("Invalid operand types.");

					result = new Result(new TimeSpan((long)System.Math.Pow(left.timeSpan.Ticks, right.number)));
					break;
				default:
					throw new ExpressionEvaluationException("Incompatible operand types.");
			}
		}

		public void visit(CondExpr node)
		{
			Result condResult = Evaluate(node.condExpr);

			switch (condResult.type)
			{
				case Result.Type.Boolean:
					result = condResult.boolean
						? Evaluate(node.thenExpr)
						: Evaluate(node.elseExpr);
					break;
				case Result.Type.Number:
					result = condResult.number != 0
						? Evaluate(node.thenExpr)
						: Evaluate(node.elseExpr);
					break;
				default:
					throw new ExpressionEvaluationException("Incompatible operand types.");
			}
		}

		public void visit(CompareNode compareNode)
		{
			// SEMANTICS:
			//     number OP number
			//     date RelOp date
			//     text RelOp any
			//     any RelOp text
			//     timespan RelOp timespan
			//     boolean BoolOp boolean

			//     OP ::= RelOp | BoolOp
			//     RelOp ::= on of: < <= = <> >= >
			//     BoolOp ::= on of: AND OR XOR

			Result left = Evaluate(compareNode.left);
			Result right = Evaluate(compareNode.right);

			if (left.type == Result.Type.Text || right.type == Result.Type.Text)
				switch (compareNode.compare) {
					case CompareNode.Compare.Equal:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) == 0);
						return;
					case CompareNode.Compare.UnEqual:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) != 0);
						return;
					case CompareNode.Compare.Less:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) < 0);
						return;
					case CompareNode.Compare.LessEqual:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) <= 0);
						return;
					case CompareNode.Compare.Greater:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) > 0);
						return;
					case CompareNode.Compare.GreaterEqual:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) >= 0);
						return;
					default:
						throw new ExpressionEvaluationException("Invalid operation.");
				}

			if (left.type != right.type)
				throw new ExpressionEvaluationException("Incompatible types.");

			switch (left.type)
			{
				case Result.Type.Number:
					switch (compareNode.compare)
					{
						case CompareNode.Compare.Less:
							result = new Result(left.number < right.number);
							break;
						case CompareNode.Compare.LessEqual:
							result = new Result(left.number <= right.number);
							break;
						case CompareNode.Compare.Equal:
							result = new Result(left.number == right.number);
							break;
						case CompareNode.Compare.UnEqual:
							result = new Result(left.number != right.number);
							break;
						case CompareNode.Compare.GreaterEqual:
							result = new Result(left.number >= right.number);
							break;
						case CompareNode.Compare.Greater:
							result = new Result(left.number > right.number);
							break;
						case CompareNode.Compare.And:
							result = new Result(left.number != 0 && right.number != 0);
							break;
						case CompareNode.Compare.Or:
							result = new Result(left.number != 0 || right.number != 0);
							break;
						case CompareNode.Compare.XOr:
							result = new Result(left.number != 0 ^ right.number != 0);
							break;
						default:
							throw new ExpressionEvaluationException("Invalid operation.");
					}
					break;
				case Result.Type.Date:
					switch (compareNode.compare)
					{
						case CompareNode.Compare.Less:
							result = new Result(left.date < right.date);
							break;
						case CompareNode.Compare.LessEqual:
							result = new Result(left.date <= right.date);
							break;
						case CompareNode.Compare.Equal:
							result = new Result(left.date == right.date);
							break;
						case CompareNode.Compare.UnEqual:
							result = new Result(left.date != right.date);
							break;
						case CompareNode.Compare.GreaterEqual:
							result = new Result(left.date >= right.date);
							break;
						case CompareNode.Compare.Greater:
							result = new Result(left.date > right.date);
							break;
						default:
							throw new ExpressionEvaluationException("Invalid operation.");
					}
					break;
				case Result.Type.TimeSpan:
					switch (compareNode.compare)
					{
						case CompareNode.Compare.Less:
							result = new Result(left.timeSpan < right.timeSpan);
							break;
						case CompareNode.Compare.LessEqual:
							result = new Result(left.timeSpan <= right.timeSpan);
							break;
						case CompareNode.Compare.Equal:
							result = new Result(left.timeSpan == right.timeSpan);
							break;
						case CompareNode.Compare.UnEqual:
							result = new Result(left.timeSpan != right.timeSpan);
							break;
						case CompareNode.Compare.GreaterEqual:
							result = new Result(left.timeSpan >= right.timeSpan);
							break;
						case CompareNode.Compare.Greater:
							result = new Result(left.timeSpan > right.timeSpan);
							break;
						default:
							throw new ExpressionEvaluationException("Invalid operation.");
					}
					break;
				case Result.Type.Boolean:
					switch (compareNode.compare)
					{
						case CompareNode.Compare.And:
							result = new Result(left.boolean && right.boolean);
							break;
						case CompareNode.Compare.Or:
							result = new Result(left.boolean || right.boolean);
							break;
						case CompareNode.Compare.XOr:
							result = new Result(left.boolean ^ right.boolean);
							break;
						case CompareNode.Compare.Equal:
							result = new Result(left.boolean == right.boolean);
							break;
						case CompareNode.Compare.UnEqual:
							result = new Result(left.boolean != right.boolean);
							break;
						default:
							throw new ExpressionEvaluationException("Invalid operation.");
					}
					break;
				default:
					throw new ExpressionEvaluationException("Incompatible types.");
			}
		}

		public void visit(VariableNode variableNode)
		{
			if (evaluateVariable == null || evaluateVariable.GetInvocationList().Length == 0)
				throw new Exception("Cannot resolve symbol: '" + variableNode.name + "'");

			result = evaluateVariable(variableNode);
		}

		public void visit(RangeNode rangeNode)
		{
			Result low = Evaluate(rangeNode.low);
			Result high = Evaluate(rangeNode.high);

			if (low.type != high.type)
				throw new Exception("Evaluation error: Cannot handle range set components of different type.");

			switch (low.type)
			{
				case Result.Type.Boolean:
					result = new Result(low.boolean ? 1 : 0, high.boolean ? 1 : 0);
					break;
				case Result.Type.Date:
					result = new Result(new DateRange(low.date, high.date));
					break;
				case Result.Type.DateRange:
					throw new Exception("Illegal evaluation: Cannot evaluate the range of two number range sets into a single range set.");
				case Result.Type.Number:
					result = new Result(low.number, high.number);
					break;
				case Result.Type.NumberRange:
					throw new Exception("Illegal evaluation: Cannot evaluate the range of two number range sets into a single range set.");
				case Result.Type.TimeSpan:
					result = new Result(low.timeSpan, high.timeSpan);
					break;
				case Result.Type.TimeSpanRange:
					throw new Exception("Illegal evaluation: Cannot evaluate the range of two number range sets into a single range set.");
				default:
					throw new Exception("Internal error: unhandled enum value.");
			}
		}

		public void visit(ContainsNode contains)
		{
			Result test = Evaluate(contains.test);

			Result range = Evaluate(contains.range);

			switch (test.type)
			{
				case Result.Type.Boolean:
					throw new Exception("Unsupported range bound test.");
				case Result.Type.Date:
					if (range.type == Result.Type.DateRange)
						result = new Result(range.dateRange.Contains(test.date));
					else
						throw new Exception("Expected a date range.");
					break;
				case Result.Type.DateRange:
					throw new Exception("Unsupported range bound test.");

                case Result.Type.Text:
                    if (range.type == Result.Type.TextRange)
                        result = new Result(range.text.Contains(test.text));
                    else
                        throw new Exception("Expected a number range.");
                    break;
		
				case Result.Type.Number:
					if (range.type == Result.Type.NumberRange)
						result = new Result(range.numberRange.Contains(test.number));
					else
						throw new Exception("Expected a number range.");
					break;
				case Result.Type.NumberRange:
					throw new Exception("Unsuppported range bound test.");
				case Result.Type.TimeSpan:
					if (range.type == Result.Type.TimeSpanRange)
						result = new Result(
							range.timeSpanRange.Key.Ticks <= test.timeSpan.Ticks && test.timeSpan.Ticks <= range.timeSpanRange.Value.Ticks
						);
					else
						throw new Exception("Expected a timespan range.");
					break;
				case Result.Type.TimeSpanRange:
					throw new Exception("Unsupported range bound test.");
				default:
					throw new Exception("Internal Error: unhandled enum value.");
			}
		}

		public void visit(DateExpr date)
		{
			result = new Result(date.date);
		}

		// TODO: rename DateCastExpr to TimeSpanCast maybe?
		public void visit(DateCastExpr dateCast)
		{
			// SEMANTICS:
			//     DATE_CAST(number): TIMESPAN

			Result number = Evaluate(dateCast.subExpression);

			if (number.type != Result.Type.Number)
				throw new ExpressionEvaluationException("Illegal date cast.");

			switch (dateCast.unit)
			{
				case DateCastExpr.Unit.Year:
					throw new ExpressionEvaluationException("The result would be inaccurate.");
				case DateCastExpr.Unit.Month:
					throw new ExpressionEvaluationException("The result would be inaccurate.");
				case DateCastExpr.Unit.Day:
					result = new Result(new TimeSpan((long)(number.number * TimeSpan.FromDays(1).Ticks)));
					break;
				default:
					throw new Exception("Internal conversion error: unhandled type.");
			}
		}

		public void visit(DateQualExpr dateQual)
		{
			// SEMANTICS:
			//     DATE.QUAL: NUMBER

			Result dateResult = Evaluate(dateQual.expression);
			if (dateResult.type != Result.Type.Date)
				throw new Exception("Date qualifier expected a date expression to qualify. But got something else.");

			switch (dateQual.qual)
			{
				case DateQualExpr.Qual.Day:
					result = new Result((double)dateResult.date.Day);
					break;
				case DateQualExpr.Qual.Month:
					result = new Result((double)dateResult.date.Month);
					break;
				case DateQualExpr.Qual.Year:
					result = new Result((double)dateResult.date.Year);
					break;
				default:
					throw new ExpressionEvaluationException("Invalid date qualification.");
			}
		}

		public void visit(TimeSpanCastExpr timeSpan)
		{
			// SEMANTICS:
			//     TIMESPAN(number) ==> results in TIMESPAN
			//     TIMESPAN(timespan) ==> results in NUMBER
			//     TIMESPAN(date .. date) ==> results in NUMBER
			//     TIMESPAN(date - date) ==> results in NUMBER

			result = Evaluate(timeSpan.timeSpan);

			switch (result.type)
			{
				case Result.Type.Number:
					switch (timeSpan.unit)
					{
						case TimeSpanCastExpr.Unit.Day:
							result = new Result(TimeSpan.FromDays(result.number));
							break;
						case TimeSpanCastExpr.Unit.Month:
							result = new Result(TimeSpan.FromDays(result.number * 30));
							break;
//							throw new ExpressionEvaluationException("The result would be inaccurate.");
						case TimeSpanCastExpr.Unit.Year:
							result = new Result(TimeSpan.FromDays(result.number * 365));
							break;
//							throw new ExpressionEvaluationException("The result would be inaccurate.");
						default:
							throw new Exception("Internal error: Unhandled enum value.");
					}
					break;
				case Result.Type.TimeSpan:
					switch (timeSpan.unit)
					{
						case TimeSpanCastExpr.Unit.Day:
							result = new Result(result.timeSpan.TotalDays);
							break;
						case TimeSpanCastExpr.Unit.Month:
							result = new Result(result.timeSpan.TotalDays / 30);
							break;
//							throw new ExpressionEvaluationException("The result would be inaccurate.");
						case TimeSpanCastExpr.Unit.Year:
							result = new Result(result.timeSpan.TotalDays / 365);
							break;
//							throw new ExpressionEvaluationException("The result would be inaccurate.");
						default:
							throw new Exception("Internal error: Unhandled enum value.");
					}
					break;
				case Result.Type.DateRange:
					switch (timeSpan.unit)
					{
						case TimeSpanCastExpr.Unit.Day:
							result = new Result(result.dateRange.TotalDays);
							break;
						case TimeSpanCastExpr.Unit.Month:
							result = new Result(result.dateRange.TotalMonths);
							break;
						case TimeSpanCastExpr.Unit.Year:
							result = new Result(result.dateRange.TotalYears);
							break;
						default:
							throw new Exception("Internal error: Unhandled enum value.");
					}
					break;
				default:
					throw new ExpressionEvaluationException("Invalid cast.");
			}
		}

		public void visit(RoundCastExpr roundCast)
		{
			result = Evaluate(roundCast.expression);

			if (result.type != Result.Type.Number)
				throw new ExpressionEvaluationException("Invalid cast.");

			result = new Result(System.Math.Round(result.number, roundCast.decimals));
		}
		#endregion

		#region helper methods
		private bool toBool(double v)
		{
			return v != 0.0;
		}

		private double toResult(bool v)
		{
			return v ? 1.0 : 0.0;
		}
		#endregion
	}
}
