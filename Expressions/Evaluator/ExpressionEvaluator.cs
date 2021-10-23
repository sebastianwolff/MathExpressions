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
			public enum ResultTypes
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
			private readonly ResultTypes _type;

			private readonly double _number;
			private readonly bool _boolean;
			private readonly DateTime _date;
			private readonly TimeSpan _timeSpan;
			private readonly DateRange _dateRange;
			private readonly string _text;
			private readonly NumberRange<double> _numberRange;
			private readonly KeyValuePair<TimeSpan, TimeSpan> _timeSpanRange;
			#endregion

			#region property accessors
			public ResultTypes Type
			{
				get { return _type; }
			}

			public bool Boolean
			{
				get 
				{
					AssumeType(ResultTypes.Boolean);
					return _boolean;
				}
			}

			public double Number
			{
				get
				{
					AssumeType(ResultTypes.Number);
					return _number;
				}
			}

			public DateTime Date
			{
				get
				{
					AssumeType(ResultTypes.Date);
					return _date.Date;
				}
			}

			public TimeSpan TimeSpan
			{
				get
				{
					AssumeType(ResultTypes.TimeSpan);
					return _timeSpan;
				}
			}

			public string Text {
				get {
					AssumeType(ResultTypes.Text);
					return _text;
				}
			}

			public DateRange DateRange
			{
				get
				{
					AssumeType(ResultTypes.DateRange);
					return _dateRange;
				}
			}

			public NumberRange<double> NumberRange
			{
				get
				{
					AssumeType(ResultTypes.NumberRange);
					return _numberRange;
				}
			}

			public KeyValuePair<TimeSpan, TimeSpan> TimeSpanRange
			{
				get
				{
					AssumeType(ResultTypes.TimeSpanRange);
					return _timeSpanRange;
				}
			}
			#endregion

			#region constructors
			public Result(bool boolean)
			{
				_type = ResultTypes.Boolean;
				_boolean = boolean;
			}

			public Result(double number)
			{
				_type = ResultTypes.Number;
				_number = number;
			}

			public Result(DateTime date)
			{
				_type = ResultTypes.Date;
				_date = date;
			}

			public Result(TimeSpan timeSpan)
			{
				_type = ResultTypes.TimeSpan;
				_timeSpan = timeSpan;
			}

			public Result(DateRange dateRange)
			{
				_type = ResultTypes.DateRange;
				_dateRange = dateRange;
			}

			public Result(DateTime begin, DateTime end)
			{
				_type = ResultTypes.DateRange;
				_dateRange = new DateRange(begin, end);
			}

			public Result(NumberRange<double> numberRange)
			{
				_type = ResultTypes.NumberRange;
				_numberRange = numberRange;
			}

			public Result(double low, double high)
			{
				_type = ResultTypes.NumberRange;
				_numberRange = new NumberRange<double>(low, high);
			}

			public Result(TimeSpan low, TimeSpan high)
			{
				_type = ResultTypes.TimeSpanRange;
                _timeSpanRange = new KeyValuePair<TimeSpan, TimeSpan>(low, high);
                //_timeSpanRange.Key = low;
                //_timeSpanRange.Value = high;
			}

			public Result(string text) {
				_type = ResultTypes.Text;
				_text = text;
			}
			#endregion

			private void AssumeType(ResultTypes assumedType)
			{
				if (Type != assumedType)
				{
					throw new InvalidCastException(String.Format(
						"Unexpected result type {0}. Expected {1}.",
						Type.ToString(), assumedType.ToString()
					));
				}
			}

			public override string ToString()
			{
                switch (Type)
                {
                    case ResultTypes.Boolean:
                        return Boolean.ToString();
                    case ResultTypes.Date:
                        return Date.ToString();
                    case ResultTypes.DateRange:
                        return String.Format("DATERANGE({0})", DateRange);
                    case ResultTypes.Number:
                        return Number.ToString();
                    case ResultTypes.NumberRange:
                        return String.Format("NUMBERRANGE({0})", NumberRange);
                    case ResultTypes.TimeSpan:
                        return String.Format("TIMESPAN({0})", TimeSpan);
                    case ResultTypes.TimeSpanRange:
                        return String.Format("TIMESPAN({0}) .. TIMESPAN({1})", TimeSpanRange.Key, TimeSpanRange.Value);
                    case ResultTypes.Text:
                        return _text;
                    case ResultTypes.TextRange:
						throw new NotImplementedException("TextRange not implemented");
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
				result = evalResult.DateRange;
            }
            else
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.String:
                        result = evalResult.ToString();
                        break;
                    case TypeCode.Int32:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
					case TypeCode.Single:
					case TypeCode.Int16:
                        result = (T)(object)Convert.ToInt32(evalResult.Number);
                        break;
                    case TypeCode.Double:
                        result = evalResult.Number;
                        break;
                    case TypeCode.Decimal:
                        result = (decimal)evalResult.Number;
                        break;
                    case TypeCode.Boolean:
                        result = evalResult.Boolean;
                        break;
                    case TypeCode.DateTime:
                        result = evalResult.Date;
                        break;
                    case TypeCode.Empty:
                    case TypeCode.Object:
                    case TypeCode.DBNull:
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
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
			var vars = new Dictionary<string, Node>();

			foreach (KeyValuePair<string, string> pair in variableMap)
				vars[pair.Key] = ExpressionBuilder.ParseExpression(pair.Value);

			return EvaluateExpression(expression, vars);
		}

		public static Result EvaluateExpression(string expression, IDictionary<string, object> variables)
		{
			var vars = new Dictionary<string, Node>();

			foreach (KeyValuePair<string, object> variable in variables)
				vars[variable.Key] = CreateNode(variable.Value);

			return EvaluateExpression(expression, vars);
		}

		private static Node CreateNode(object value) 
		{
			if (value is Node node)
				return node; // TODO: use ExpressionCloner.Clone(value);

			if (value is DateTime time)
				return new DateExpr(time);

			if (value is string stringVal)
				return new TextNode(stringVal);

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
			var variableNodeMap = new Dictionary<string, Node>(variableMap.Count);

			foreach (KeyValuePair<string, Result> current in variableMap)
			{
                switch (current.Value.Type)
                {
                    case Result.ResultTypes.Boolean:
                        variableNodeMap[current.Key] = new NumberNode(1);
                        break;
                    case Result.ResultTypes.Date:
                        variableNodeMap[current.Key] = new DateExpr(current.Value.Date);
                        break;
                    case Result.ResultTypes.DateRange:
                        variableNodeMap[current.Key] = new RangeNode(
                            new DateExpr(current.Value.DateRange.Begin),
                            new DateExpr(current.Value.DateRange.End)
                        );
                        break;
                    case Result.ResultTypes.Number:
                        variableNodeMap[current.Key] = new NumberNode(current.Value.Number);
                        break;
                    case Result.ResultTypes.NumberRange:
                        variableNodeMap[current.Key] = new RangeNode(
                            new NumberNode(current.Value.NumberRange.Min),
                            new NumberNode(current.Value.NumberRange.Max)
                        );
                        break;
                    case Result.ResultTypes.TimeSpan:
                        variableNodeMap[current.Key] = new NumberNode(current.Value.TimeSpan.Ticks);
                        break;
                    case Result.ResultTypes.Text:
                    case Result.ResultTypes.TextRange:
                    case Result.ResultTypes.TimeSpanRange:
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
            var evaluator = new ExpressionEvaluator
            {
                recursionDepth = 0,
                recursionLimit = recursionLimit,
				variableMap = variableMap
            };

            evaluator.evaluateVariable = new EvaluateVariableDelegate(evaluator.EvaluateVariableFromMap);

			expression.Accept(evaluator);

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
            var evaluator = new ExpressionEvaluator
            {
                evaluateVariable = evalVar
            };

            expression.Accept(evaluator);

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
		private Result EvaluateVariableFromMap(VariableNode variableNode)
		{
			if (variableMap.ContainsKey(variableNode.Name))
			{
				if (++recursionDepth >= recursionLimit)
					throw new Exception(String.Format("Evaluation exceeds recursion limit of {0}", recursionLimit));

				return Evaluate(variableMap[variableNode.Name]);
			}

			throw new Exception("Unresolved symbol '" + variableNode.Name + "'");
		}

		// -------------------------------------------------------------------------------------------

		/// <summary>
		/// evaluates a compiled expression tree and returns its scalar result value
		/// </summary>
		/// <param name="node">the compiled expression tree</param>
		/// <returns>their respective scalar result value</returns>
		public Result Evaluate(Node node)
		{
			node.Accept(this);
			return result;
		}

		#region INodeVisitor implementation
		public void Visit(AddNode node)
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

			Result left = Evaluate(node.Left);
			Result right = Evaluate(node.Right);

			if (left.Type == Result.ResultTypes.Text || right.Type == Result.ResultTypes.Text) {
				result = new Result(left.ToString() + right.ToString());
				return;
			}

			switch (left.Type)
			{
				case Result.ResultTypes.Date:
					// XXX: SPECIAL CASE: the difference of two dates is a timespan!
					if (node.Right.GetType() == typeof(NegNode))
					{
						Result dateRight = Evaluate(((NegNode)node.Right).SubExpression);

						if (dateRight.Type == Result.ResultTypes.Date)
						{
							result = left.Date < dateRight.Date
								? new Result(new DateRange(left.Date, dateRight.Date))
								: new Result(new DateRange(dateRight.Date, left.Date));

							break;
						}
					}

					if (right.Type != Result.ResultTypes.TimeSpan)
						throw new Exception("Operator evaluation error: Invalid operand type combination.");

					result = new Result(left.Date.Add(right.TimeSpan));

					break;
				case Result.ResultTypes.Number:
					if (right.Type != left.Type)
						throw new Exception("Operator evaluation error: Invalid operand type combination.");

					result = new Result(left.Number + right.Number);

					break;
				case Result.ResultTypes.TimeSpan:
                    switch (right.Type)
                    {
                        case Result.ResultTypes.Date:
                            result = new Result(right.Date.Add(left.TimeSpan));
                            break;
                        case Result.ResultTypes.TimeSpan:
                            result = new Result(left.TimeSpan.Add(right.TimeSpan));
                            break;
                        case Result.ResultTypes.Boolean:
                        case Result.ResultTypes.Number:
                        case Result.ResultTypes.Text:
                        case Result.ResultTypes.TextRange:
                        case Result.ResultTypes.DateRange:
                        case Result.ResultTypes.NumberRange:
                        case Result.ResultTypes.TimeSpanRange:
                        default:
                            throw new ExpressionEvaluationException("Incompatible operand types.");
                    }
                    break;
				default:
					throw new ExpressionEvaluationException("Incompatible operand types.");
            }
        }

        public void Visit(NegNode node)
		{
			// SEMANTICS:
			//     - number
			//     - timespan

			Result operand = Evaluate(node.SubExpression);

            switch (operand.Type)
            {
                case Result.ResultTypes.Number:
                    result = new Result(-operand.Number);
                    break;
                case Result.ResultTypes.TimeSpan:
                    result = new Result(-operand.TimeSpan);
                    break;
                case Result.ResultTypes.Date:
                    result = new Result(new TimeSpan(-operand.Date.Ticks));
                    break;
                case Result.ResultTypes.Boolean:
                case Result.ResultTypes.Text:
                case Result.ResultTypes.TextRange:
                case Result.ResultTypes.DateRange:
                case Result.ResultTypes.NumberRange:
                case Result.ResultTypes.TimeSpanRange:
                default:
                    throw new Exception("Operator evaluation error: Invalid operand type.");
            }
        }

        public void Visit(MulNode node)
		{
			// SEMANTICS:
			//     number * number
			//     timespan * number
			//     number * timespan

			Result left = Evaluate(node.Left);
			Result right = Evaluate(node.Right);

            switch (left.Type)
            {
                case Result.ResultTypes.Number:
                    switch (right.Type)
                    {
                        case Result.ResultTypes.Number:
                            result = new Result(left.Number * right.Number);
                            break;
                        case Result.ResultTypes.TimeSpan:
                            result = new Result(new TimeSpan((long)(left.Number * right.TimeSpan.Ticks)));
                            break;
                        case Result.ResultTypes.Boolean:
                        case Result.ResultTypes.Date:
                        case Result.ResultTypes.Text:
                        case Result.ResultTypes.TextRange:
                        case Result.ResultTypes.DateRange:
                        case Result.ResultTypes.NumberRange:
                        case Result.ResultTypes.TimeSpanRange:
                        default:
                            throw new Exception("Operator evaluation error: Incompatible operand types.");
                    }
                    break;
                case Result.ResultTypes.TimeSpan:
                    switch (right.Type)
                    {
                        case Result.ResultTypes.Number:
                            result = new Result(new TimeSpan((long)(left.TimeSpan.Ticks * right.Number)));
                            break;
                        case Result.ResultTypes.Boolean:
                        case Result.ResultTypes.Date:
                        case Result.ResultTypes.TimeSpan:
                        case Result.ResultTypes.Text:
                        case Result.ResultTypes.TextRange:
                        case Result.ResultTypes.DateRange:
                        case Result.ResultTypes.NumberRange:
                        case Result.ResultTypes.TimeSpanRange:
                        default:
                            throw new Exception("Operator evaluation error: Incompatible operand types.");
                    }
                    break;
                case Result.ResultTypes.Boolean:
                case Result.ResultTypes.Date:
                case Result.ResultTypes.Text:
                case Result.ResultTypes.TextRange:
                case Result.ResultTypes.DateRange:
                case Result.ResultTypes.NumberRange:
                case Result.ResultTypes.TimeSpanRange:
                default:
                    throw new Exception("Operator evaluation error: Incompatible operand types.");
            }
        }

        public void Visit(DivNode node)
		{
			// SEMANTICS:
			//     number / number
			//     timespan / number

			Result left = Evaluate(node.Left);
			Result right = Evaluate(node.Right);

			switch (left.Type)
			{
				case Result.ResultTypes.Number:
					if (right.Type != Result.ResultTypes.Number)
						throw new Exception("Operator evaluation error: Incompatible operand types.");

					result = new Result(left.Number / right.Number);
					break;
				case Result.ResultTypes.TimeSpan:
					if (right.Type != Result.ResultTypes.Number)
						throw new Exception("Operator evaluation error: Incompatible operand types.");

					result = new Result(new TimeSpan((long)(left.TimeSpan.Ticks / right.Number)));
					break;
				default:
					throw new Exception("Operator evaluation error: Incompatible operand types.");
			}
		}

		public void Visit(NumberNode node)
		{
			result = new Result(node.Value);
		}

		public void Visit(TextNode node) {
			result = new Result(node.Value);
		}

		public void Visit(PowNode node)
		{
			// SEMANTICS:
			//     number ^ number
			//     timespan ^ number

			Result left = Evaluate(node.PowBase);
			Result right = Evaluate(node.Exp);

			switch (left.Type)
			{
				case Result.ResultTypes.Number:
					if (right.Type != Result.ResultTypes.Number)
						throw new ExpressionEvaluationException("Invalid operand types.");

					result = new Result(System.Math.Pow(left.Number, right.Number));
					break;
				case Result.ResultTypes.TimeSpan:
					if (right.Type != Result.ResultTypes.Number)
						throw new ExpressionEvaluationException("Invalid operand types.");

					result = new Result(new TimeSpan((long)System.Math.Pow(left.TimeSpan.Ticks, right.Number)));
					break;
				default:
					throw new ExpressionEvaluationException("Incompatible operand types.");
			}
		}

		public void Visit(CondExpr node)
		{
			Result condResult = Evaluate(node.Expression);

			switch (condResult.Type)
			{
				case Result.ResultTypes.Boolean:
					result = condResult.Boolean
						? Evaluate(node.ThenExpr)
						: Evaluate(node.ElseExpr);
					break;
				case Result.ResultTypes.Number:
					result = condResult.Number != 0
						? Evaluate(node.ThenExpr)
						: Evaluate(node.ElseExpr);
					break;
				default:
					throw new ExpressionEvaluationException("Incompatible operand types.");
			}
		}

		public void Visit(CompareNode compareNode)
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

			Result left = Evaluate(compareNode.Left);
			Result right = Evaluate(compareNode.Right);

			if (left.Type == Result.ResultTypes.Text || right.Type == Result.ResultTypes.Text)
				switch (compareNode.Compare) {
					case CompareNode.CompareTypes.Equal:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) == 0);
						return;
					case CompareNode.CompareTypes.UnEqual:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) != 0);
						return;
					case CompareNode.CompareTypes.Less:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) < 0);
						return;
					case CompareNode.CompareTypes.LessEqual:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) <= 0);
						return;
					case CompareNode.CompareTypes.Greater:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) > 0);
						return;
					case CompareNode.CompareTypes.GreaterEqual:
						result = new Result(String.Compare(left.ToString(), right.ToString(),
							StringComparison.CurrentCultureIgnoreCase) >= 0);
						return;
					default:
						throw new ExpressionEvaluationException("Invalid operation.");
				}

			if (left.Type != right.Type)
				throw new ExpressionEvaluationException("Incompatible types.");

			switch (left.Type)
			{
				case Result.ResultTypes.Number:
					switch (compareNode.Compare)
					{
						case CompareNode.CompareTypes.Less:
							result = new Result(left.Number < right.Number);
							break;
						case CompareNode.CompareTypes.LessEqual:
							result = new Result(left.Number <= right.Number);
							break;
						case CompareNode.CompareTypes.Equal:
							result = new Result(left.Number == right.Number);
							break;
						case CompareNode.CompareTypes.UnEqual:
							result = new Result(left.Number != right.Number);
							break;
						case CompareNode.CompareTypes.GreaterEqual:
							result = new Result(left.Number >= right.Number);
							break;
						case CompareNode.CompareTypes.Greater:
							result = new Result(left.Number > right.Number);
							break;
						case CompareNode.CompareTypes.And:
							result = new Result(left.Number != 0 && right.Number != 0);
							break;
						case CompareNode.CompareTypes.Or:
							result = new Result(left.Number != 0 || right.Number != 0);
							break;
						case CompareNode.CompareTypes.XOr:
							result = new Result(left.Number != 0 ^ right.Number != 0);
							break;
						default:
							throw new ExpressionEvaluationException("Invalid operation.");
					}
					break;
				case Result.ResultTypes.Date:
					switch (compareNode.Compare)
					{
						case CompareNode.CompareTypes.Less:
							result = new Result(left.Date < right.Date);
							break;
						case CompareNode.CompareTypes.LessEqual:
							result = new Result(left.Date <= right.Date);
							break;
						case CompareNode.CompareTypes.Equal:
							result = new Result(left.Date == right.Date);
							break;
						case CompareNode.CompareTypes.UnEqual:
							result = new Result(left.Date != right.Date);
							break;
						case CompareNode.CompareTypes.GreaterEqual:
							result = new Result(left.Date >= right.Date);
							break;
						case CompareNode.CompareTypes.Greater:
							result = new Result(left.Date > right.Date);
							break;
						default:
							throw new ExpressionEvaluationException("Invalid operation.");
					}
					break;
				case Result.ResultTypes.TimeSpan:
					switch (compareNode.Compare)
					{
						case CompareNode.CompareTypes.Less:
							result = new Result(left.TimeSpan < right.TimeSpan);
							break;
						case CompareNode.CompareTypes.LessEqual:
							result = new Result(left.TimeSpan <= right.TimeSpan);
							break;
						case CompareNode.CompareTypes.Equal:
							result = new Result(left.TimeSpan == right.TimeSpan);
							break;
						case CompareNode.CompareTypes.UnEqual:
							result = new Result(left.TimeSpan != right.TimeSpan);
							break;
						case CompareNode.CompareTypes.GreaterEqual:
							result = new Result(left.TimeSpan >= right.TimeSpan);
							break;
						case CompareNode.CompareTypes.Greater:
							result = new Result(left.TimeSpan > right.TimeSpan);
							break;
						default:
							throw new ExpressionEvaluationException("Invalid operation.");
					}
					break;
				case Result.ResultTypes.Boolean:
					switch (compareNode.Compare)
					{
						case CompareNode.CompareTypes.And:
							result = new Result(left.Boolean && right.Boolean);
							break;
						case CompareNode.CompareTypes.Or:
							result = new Result(left.Boolean || right.Boolean);
							break;
						case CompareNode.CompareTypes.XOr:
							result = new Result(left.Boolean ^ right.Boolean);
							break;
						case CompareNode.CompareTypes.Equal:
							result = new Result(left.Boolean == right.Boolean);
							break;
						case CompareNode.CompareTypes.UnEqual:
							result = new Result(left.Boolean != right.Boolean);
							break;
						default:
							throw new ExpressionEvaluationException("Invalid operation.");
					}
					break;
				default:
					throw new ExpressionEvaluationException("Incompatible types.");
			}
		}

		public void Visit(VariableNode variableNode)
		{
			if (evaluateVariable == null || evaluateVariable.GetInvocationList().Length == 0)
				throw new Exception("Cannot resolve symbol: '" + variableNode.Name + "'");

			result = evaluateVariable(variableNode);
		}

		public void Visit(RangeNode rangeNode)
		{
			Result low = Evaluate(rangeNode.Low);
			Result high = Evaluate(rangeNode.High);

			if (low.Type != high.Type)
				throw new Exception("Evaluation error: Cannot handle range set components of different type.");

			switch (low.Type)
			{
				case Result.ResultTypes.Boolean:
					result = new Result(low.Boolean ? 1 : 0, high.Boolean ? 1 : 0);
					break;
				case Result.ResultTypes.Date:
					result = new Result(new DateRange(low.Date, high.Date));
					break;
				case Result.ResultTypes.DateRange:
					throw new Exception("Illegal evaluation: Cannot evaluate the range of two number range sets into a single range set.");
				case Result.ResultTypes.Number:
					result = new Result(low.Number, high.Number);
					break;
				case Result.ResultTypes.NumberRange:
					throw new Exception("Illegal evaluation: Cannot evaluate the range of two number range sets into a single range set.");
				case Result.ResultTypes.TimeSpan:
					result = new Result(low.TimeSpan, high.TimeSpan);
					break;
				case Result.ResultTypes.TimeSpanRange:
					throw new Exception("Illegal evaluation: Cannot evaluate the range of two number range sets into a single range set.");
				default:
					throw new Exception("Internal error: unhandled enum value.");
			}
		}

		public void Visit(ContainsNode contains)
		{
			Result test = Evaluate(contains.Test);

			Result range = Evaluate(contains.Range);

			switch (test.Type)
			{
				case Result.ResultTypes.Boolean:
					throw new Exception("Unsupported range bound test.");
				case Result.ResultTypes.Date:
					if (range.Type == Result.ResultTypes.DateRange)
						result = new Result(range.DateRange.Contains(test.Date));
					else
						throw new Exception("Expected a date range.");
					break;
				case Result.ResultTypes.DateRange:
					throw new Exception("Unsupported range bound test.");

                case Result.ResultTypes.Text:
                    if (range.Type == Result.ResultTypes.TextRange)
                        result = new Result(range.Text.Contains(test.Text));
                    else
                        throw new Exception("Expected a number range.");
                    break;
		
				case Result.ResultTypes.Number:
					if (range.Type == Result.ResultTypes.NumberRange)
						result = new Result(range.NumberRange.Contains(test.Number));
					else
						throw new Exception("Expected a number range.");
					break;
				case Result.ResultTypes.NumberRange:
					throw new Exception("Unsuppported range bound test.");
				case Result.ResultTypes.TimeSpan:
					if (range.Type == Result.ResultTypes.TimeSpanRange)
						result = new Result(
							range.TimeSpanRange.Key.Ticks <= test.TimeSpan.Ticks && test.TimeSpan.Ticks <= range.TimeSpanRange.Value.Ticks
						);
					else
						throw new Exception("Expected a timespan range.");
					break;
				case Result.ResultTypes.TimeSpanRange:
					throw new Exception("Unsupported range bound test.");
				default:
					throw new Exception("Internal Error: unhandled enum value.");
			}
		}

		public void Visit(DateExpr date)
		{
			result = new Result(date.Date);
		}

		// TODO: rename DateCastExpr to TimeSpanCast maybe?
		public void Visit(DateCastExpr dateCast)
		{
			// SEMANTICS:
			//     DATE_CAST(number): TIMESPAN

			Result number = Evaluate(dateCast.SubExpression);

			if (number.Type != Result.ResultTypes.Number)
				throw new ExpressionEvaluationException("Illegal date cast.");

			switch (dateCast.Unit)
			{
				case DateCastExpr.Units.Year:
					throw new ExpressionEvaluationException("The result would be inaccurate.");
				case DateCastExpr.Units.Month:
					throw new ExpressionEvaluationException("The result would be inaccurate.");
				case DateCastExpr.Units.Day:
					result = new Result(new TimeSpan((long)(number.Number * TimeSpan.FromDays(1).Ticks)));
					break;
				default:
					throw new Exception("Internal conversion error: unhandled type.");
			}
		}

		public void Visit(DateQualExpr dateQual)
		{
			// SEMANTICS:
			//     DATE.QUAL: NUMBER

			Result dateResult = Evaluate(dateQual.Expression);
			if (dateResult.Type != Result.ResultTypes.Date)
				throw new Exception("Date qualifier expected a date expression to qualify. But got something else.");

			switch (dateQual.Qual)
			{
				case DateQualExpr.Quals.Day:
					result = new Result((double)dateResult.Date.Day);
					break;
				case DateQualExpr.Quals.Month:
					result = new Result((double)dateResult.Date.Month);
					break;
				case DateQualExpr.Quals.Year:
					result = new Result((double)dateResult.Date.Year);
					break;
				default:
					throw new ExpressionEvaluationException("Invalid date qualification.");
			}
		}

		public void Visit(TimeSpanCastExpr timeSpan)
		{
			// SEMANTICS:
			//     TIMESPAN(number) ==> results in TIMESPAN
			//     TIMESPAN(timespan) ==> results in NUMBER
			//     TIMESPAN(date .. date) ==> results in NUMBER
			//     TIMESPAN(date - date) ==> results in NUMBER

			result = Evaluate(timeSpan.TimeSpan);

			switch (result.Type)
			{
				case Result.ResultTypes.Number:
					switch (timeSpan.Unit)
					{
						case TimeSpanCastExpr.Units.Day:
							result = new Result(TimeSpan.FromDays(result.Number));
							break;
						case TimeSpanCastExpr.Units.Month:
							result = new Result(TimeSpan.FromDays(result.Number * 30));
							break;
//							throw new ExpressionEvaluationException("The result would be inaccurate.");
						case TimeSpanCastExpr.Units.Year:
							result = new Result(TimeSpan.FromDays(result.Number * 365));
							break;
//							throw new ExpressionEvaluationException("The result would be inaccurate.");
						default:
							throw new Exception("Internal error: Unhandled enum value.");
					}
					break;
				case Result.ResultTypes.TimeSpan:
					switch (timeSpan.Unit)
					{
						case TimeSpanCastExpr.Units.Day:
							result = new Result(result.TimeSpan.TotalDays);
							break;
						case TimeSpanCastExpr.Units.Month:
							result = new Result(result.TimeSpan.TotalDays / 30);
							break;
//							throw new ExpressionEvaluationException("The result would be inaccurate.");
						case TimeSpanCastExpr.Units.Year:
							result = new Result(result.TimeSpan.TotalDays / 365);
							break;
//							throw new ExpressionEvaluationException("The result would be inaccurate.");
						default:
							throw new Exception("Internal error: Unhandled enum value.");
					}
					break;
				case Result.ResultTypes.DateRange:
					switch (timeSpan.Unit)
					{
						case TimeSpanCastExpr.Units.Day:
							result = new Result(result.DateRange.TotalDays);
							break;
						case TimeSpanCastExpr.Units.Month:
							result = new Result(result.DateRange.TotalMonths);
							break;
						case TimeSpanCastExpr.Units.Year:
							result = new Result(result.DateRange.TotalYears);
							break;
						default:
							throw new Exception("Internal error: Unhandled enum value.");
					}
					break;
				default:
					throw new ExpressionEvaluationException("Invalid cast.");
			}
		}

		public void Visit(RoundCastExpr roundCast)
		{
			result = Evaluate(roundCast.Expression);

			if (result.Type != Result.ResultTypes.Number)
				throw new ExpressionEvaluationException("Invalid cast.");

			result = new Result(System.Math.Round(result.Number, roundCast.Decimals));
		}
        #endregion


    }
}
