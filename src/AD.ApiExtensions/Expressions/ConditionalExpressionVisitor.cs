﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace AD.ApiExtensions.Expressions
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an expression visitor which reduces conditional statements dependant upon constant values.
    /// </summary>
    [PublicAPI]
    public sealed class ConditionalExpressionVisitor : ExpressionVisitor
    {
        /// <inheritdoc />
        [Pure]
        [ContractAnnotation("e:notnull => notnull; e:null => null")]
        public override Expression Visit(Expression e) => base.Visit(e);

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!(Visit(node.Left) is Expression left))
                throw new ArgumentNullException(nameof(left));

            if (!(Visit(node.Right) is Expression right))
                throw new ArgumentNullException(nameof(right));

            switch (node.NodeType)
            {
                case ExpressionType.And when left is ConstantExpression c && c.Value is bool b && right is ConstantExpression cc && cc.Value is bool bb:
                {
                    return Expression.Constant(b & bb, typeof(bool));
                }
                case ExpressionType.AndAlso when left is ConstantExpression c && c.Value is false:
                {
                    return Expression.Constant(false, typeof(bool));
                }
                case ExpressionType.AndAlso when right is ConstantExpression c && c.Value is false:
                {
                    return Expression.Constant(false, typeof(bool));
                }
                case ExpressionType.AndAlso when left is ConstantExpression c && c.Value is true && right is ConstantExpression cc && cc.Value is true:
                {
                    return Expression.Constant(true, typeof(bool));
                }
                case ExpressionType.AndAlso when left is ConstantExpression c && c.Value is true:
                {
                    return right;
                }
                case ExpressionType.AndAlso when right is ConstantExpression c && c.Value is true:
                {
                    return left;
                }
                case ExpressionType.Equal when left is ConstantExpression c && right is ConstantExpression cc:
                {
                    return Expression.Constant(c.Value?.Equals(cc.Value) ?? c.Value == cc.Value, typeof(bool));
                }
                case ExpressionType.ExclusiveOr when left is ConstantExpression c && c.Value is bool b && right is ConstantExpression cc && cc.Value is bool bb:
                {
                    return Expression.Constant(b ^ bb, typeof(bool));
                }
                case ExpressionType.GreaterThan when left is ConstantExpression c && right is ConstantExpression cc:
                {
                    return CompileToBool(Expression.GreaterThan(c, cc));
                }
                case ExpressionType.GreaterThanOrEqual when left is ConstantExpression c && right is ConstantExpression cc:
                {
                    return CompileToBool(Expression.GreaterThanOrEqual(c, cc));
                }
                case ExpressionType.LessThan when left is ConstantExpression c && right is ConstantExpression cc:
                {
                    return CompileToBool(Expression.LessThan(c, cc));
                }
                case ExpressionType.LessThanOrEqual when left is ConstantExpression c && right is ConstantExpression cc:
                {
                    return CompileToBool(Expression.LessThanOrEqual(c, cc));
                }
                case ExpressionType.NotEqual when left is ConstantExpression c && right is ConstantExpression cc:
                {
                    return Expression.Constant(c.Value != cc.Value, typeof(bool));
                }
                case ExpressionType.Or when left is ConstantExpression c && c.Value is bool b && right is ConstantExpression cc && cc.Value is bool bb:
                {
                    return Expression.Constant(b | bb, typeof(bool));
                }
                case ExpressionType.OrElse when left is ConstantExpression c && c.Value is true:
                {
                    return Expression.Constant(true, typeof(bool));
                }
                case ExpressionType.OrElse when right is ConstantExpression c && c.Value is true:
                {
                    return Expression.Constant(true, typeof(bool));
                }
                case ExpressionType.OrElse when left is ConstantExpression c && c.Value is false && right is ConstantExpression cc && cc.Value is false:
                {
                    return Expression.Constant(false, typeof(bool));
                }
                case ExpressionType.OrElse when left is ConstantExpression c && c.Value is false:
                {
                    return right;
                }
                case ExpressionType.OrElse when right is ConstantExpression c && c.Value is false:
                {
                    return left;
                }
                case ExpressionType.TypeEqual when left is ConstantExpression c && c.Value is Type t && right is ConstantExpression cc && cc.Value is Type tt:
                {
                    return Expression.Constant(t == tt);
                }
                case ExpressionType.TypeIs when left is ConstantExpression c && right is ConstantExpression cc && cc.Value is Type t:
                {
                    return Expression.Constant(t.IsInstanceOfType(c.Value));
                }
                case ExpressionType.Add:
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.AddChecked:
                case ExpressionType.AndAssign:
                case ExpressionType.ArrayIndex:
                case ExpressionType.ArrayLength:
                case ExpressionType.Assign:
                case ExpressionType.Block:
                case ExpressionType.Call:
                case ExpressionType.Coalesce:
                case ExpressionType.Conditional:
                case ExpressionType.Constant:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.DebugInfo:
                case ExpressionType.Decrement:
                case ExpressionType.Default:
                case ExpressionType.Divide:
                case ExpressionType.DivideAssign:
                case ExpressionType.Dynamic:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.Extension:
                case ExpressionType.Goto:
                case ExpressionType.Increment:
                case ExpressionType.Index:
                case ExpressionType.Invoke:
                case ExpressionType.IsFalse:
                case ExpressionType.IsTrue:
                case ExpressionType.Label:
                case ExpressionType.Lambda:
                case ExpressionType.LeftShift:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ListInit:
                case ExpressionType.Loop:
                case ExpressionType.MemberAccess:
                case ExpressionType.MemberInit:
                case ExpressionType.Modulo:
                case ExpressionType.ModuloAssign:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.New:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                case ExpressionType.Not:
                case ExpressionType.OnesComplement:
                case ExpressionType.OrAssign:
                case ExpressionType.Parameter:
                case ExpressionType.PostDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.Power:
                case ExpressionType.PowerAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.Quote:
                case ExpressionType.RightShift:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.RuntimeVariables:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Switch:
                case ExpressionType.Throw:
                case ExpressionType.Try:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Unbox:
                default:
                {
                    return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method);
                }
            }
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Expression instance = Visit(node.Object);
            Expression[] arguments = node.Arguments.Select(Visit).ToArray();

            if (node.Method.Name == nameof(IExpressive.Express) && instance is ConstantExpression constantInstance0 && constantInstance0.Value is IExpressive expressive)
            {
                return expressive.Reduce(node.Arguments[0]);
            }

            if (node.Method.Name == "ToString" && !arguments.Any() && instance is ConstantExpression constantToString)
            {
                return Expression.Constant(constantToString.Value.ToString(), typeof(string));
            }

            if (node.Method.Name != "Any")
            {
                return base.VisitMethodCall(node);
            }

            if (node.Method.Name == "Any" && arguments.Length == 1)
            {
                switch (arguments[0])
                {
                    // Testing to get directly from model.
                    case UnaryExpression u when u.Operand is MemberExpression m && m.Type.IsInstanceOfType(typeof(ICollection<string>)):
                    {
                        ConstantExpression capture = (ConstantExpression) m.Expression;
                        PropertyInfo propertyInfo = (PropertyInfo) m.Member;
                        ICollection<string> collection = (ICollection<string>) propertyInfo.GetValue(capture.Value);
                        return Expression.Constant(collection.Any(), typeof(bool));
                    }
                    case UnaryExpression u when u.Operand is ConstantExpression c && c.Value is ICollection<string> collection:
                    {
                        return Expression.Constant(collection.Any(), typeof(bool));
                    }
                    case ConstantExpression c when c.Value is ICollection<string> collection:
                    {
                        return Expression.Constant(collection.Any(), typeof(bool));
                    }
                    case UnaryExpression u when u.Operand is ConstantExpression c && c.Value is IGrouping<string, string> collection:
                    {
                        return Expression.Constant(collection.Any(), typeof(bool));
                    }
                    case UnaryExpression u when u.Operand is ConstantExpression c && c.Value is IEnumerable<IGrouping<string, string>> collection:
                    {
                        return Expression.Constant(collection.Any(x => x.Any()), typeof(bool));
                    }
                }
            }

            if (node.Method.Name == "Any" && arguments.Length == 2)
            {
                switch (arguments[0])
                {
                    case UnaryExpression u
                        when u.Operand is ConstantExpression c &&
                             c.Value is ICollection<string> cc &&
                             arguments[1] is UnaryExpression uu &&
                             uu.Operand is MethodCallExpression m:
                    {
                        if (!(m.Object is ConstantExpression obj))
                        {
                            throw new ArgumentNullException(nameof(obj));
                        }

                        MethodInfo methodInfo = (MethodInfo) obj.Value;

                        return
                            cc.Select(x => Expression.Call(m.Arguments[1], methodInfo, Expression.Constant(x)))
                              .Aggregate((Expression) Expression.Constant(false), Expression.Or);
                    }
                    case UnaryExpression u
                        when u.Operand is ConstantExpression c &&
                             c.Value is ICollection<string> cc &&
                             arguments[1] is MethodCallExpression m:
                    {
                        if (!(m.Object is ConstantExpression obj))
                        {
                            throw new ArgumentNullException(nameof(obj));
                        }

                        MethodInfo methodInfo = (MethodInfo) obj.Value;

                        return
                            cc.Select(x => Expression.Call(m.Arguments[1], methodInfo, Expression.Constant(x)))
                              .Aggregate((Expression) Expression.Constant(false), Expression.Or);
                    }
                    case ConstantExpression c
                        when c.Value is ICollection<string> cc &&
                             arguments[1] is UnaryExpression uu &&
                             uu.Operand is MethodCallExpression m:
                    {
                        if (!(m.Object is ConstantExpression obj))
                        {
                            throw new ArgumentNullException(nameof(obj));
                        }

                        MethodInfo methodInfo = (MethodInfo) obj.Value;

                        return
                            cc.Select(x => Expression.Call(m.Arguments[1], methodInfo, Expression.Constant(x)))
                              .Aggregate((Expression) Expression.Constant(false), Expression.Or);
                    }
                    case ConstantExpression c
                        when c.Value is ICollection<string> cc &&
                             arguments[1] is MethodCallExpression m:
                    {
                        if (!(m.Object is ConstantExpression obj))
                        {
                            throw new ArgumentNullException(nameof(obj));
                        }

                        MethodInfo methodInfo = (MethodInfo) obj.Value;

                        return
                            cc.Select(x => Expression.Call(m.Arguments[1], methodInfo, Expression.Constant(x)))
                              .Aggregate((Expression) Expression.Constant(false), Expression.Or);
                    }
                }
            }

            return base.VisitMethodCall(node);
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!(Visit(node.Test) is Expression test))
                throw new ArgumentNullException(nameof(test));

            if (!(Visit(node.IfTrue) is Expression ifTrue))
                throw new ArgumentNullException(nameof(ifTrue));

            if (!(Visit(node.IfFalse) is Expression ifFalse))
                throw new ArgumentNullException(nameof(ifFalse));

            switch (test)
            {
                case UnaryExpression unary when unary.Operand is ConstantExpression constant && constant.Value is bool value:
                    return value ? ifTrue : ifFalse;

                case ConstantExpression constant when constant.Value is bool value:
                    return value ? ifTrue : ifFalse;

                default:
                    return Expression.Condition(test, ifTrue, ifFalse);
            }
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            switch (node.Member)
            {
                case FieldInfo f when f.IsStatic:
                    return Expression.Constant(f.GetValue(f.DeclaringType), node.Type);

                case FieldInfo f when node.Expression is ConstantExpression c:
                    return Expression.Constant(f.GetValue(c.Value), node.Type);

                case PropertyInfo p when node.Expression is MemberExpression m && m.Member is FieldInfo f && m.Expression is ConstantExpression c:
                    return Expression.Constant(p.GetValue(f.GetValue(c.Value)), node.Type);

                default:
                    return base.VisitMember(node);
            }
        }

        /// <inheritdoc />
        [Pure]
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!(Visit(node.Operand) is Expression operand))
                throw new ArgumentNullException(nameof(operand));

            switch (node.NodeType)
            {
                case ExpressionType.ArrayLength:
                    return Expression.Constant(Expression.Lambda<Func<int>>(node).Compile()(), typeof(int));

                case ExpressionType.Not when operand is ConstantExpression constant && constant.Value is bool value:
                    return Expression.Constant(!value, typeof(bool));

                case ExpressionType.Add:
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.AndAssign:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Assign:
                case ExpressionType.Block:
                case ExpressionType.Call:
                case ExpressionType.Coalesce:
                case ExpressionType.Conditional:
                case ExpressionType.Constant:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.DebugInfo:
                case ExpressionType.Decrement:
                case ExpressionType.Default:
                case ExpressionType.Divide:
                case ExpressionType.DivideAssign:
                case ExpressionType.Dynamic:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.Extension:
                case ExpressionType.Goto:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Increment:
                case ExpressionType.Index:
                case ExpressionType.Invoke:
                case ExpressionType.IsFalse:
                case ExpressionType.IsTrue:
                case ExpressionType.Label:
                case ExpressionType.Lambda:
                case ExpressionType.LeftShift:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.ListInit:
                case ExpressionType.Loop:
                case ExpressionType.MemberAccess:
                case ExpressionType.MemberInit:
                case ExpressionType.Modulo:
                case ExpressionType.ModuloAssign:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.New:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                case ExpressionType.NotEqual:
                case ExpressionType.OnesComplement:
                case ExpressionType.Or:
                case ExpressionType.OrAssign:
                case ExpressionType.OrElse:
                case ExpressionType.Parameter:
                case ExpressionType.PostDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.Power:
                case ExpressionType.PowerAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.Quote:
                case ExpressionType.RightShift:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.RuntimeVariables:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Switch:
                case ExpressionType.Throw:
                case ExpressionType.Try:
                case ExpressionType.TypeAs:
                case ExpressionType.TypeEqual:
                case ExpressionType.TypeIs:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Unbox:
                default:
                    return Expression.MakeUnary(node.NodeType, operand, node.Type);
            }
        }

        /// <summary>
        /// Immediately executes a boolean expression and returns the result as a constant expression.
        /// </summary>
        /// <param name="expression">
        /// The expression returning a boolean value
        /// </param>
        /// <returns>
        /// The result of the expression lifted as a constant expression.
        /// </returns>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        static ConstantExpression CompileToBool([NotNull] Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return Expression.Constant(Expression.Lambda<Func<bool>>(expression).Compile()(), typeof(bool));
        }
    }
}