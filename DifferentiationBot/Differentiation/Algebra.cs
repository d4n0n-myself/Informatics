using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Differentiation
{
    public class Algebra
    {
        public static Expression<Func<double, double>> Differentiate(
            Expression<Func<double, double>> function)
        {
            return Expression.Lambda<Func<double, double>>(
                CommonDifferentiate(function.Body, function.Parameters), function.Parameters);
        }

        private static Expression CommonDifferentiate(Expression func,
            ReadOnlyCollection<ParameterExpression> parameters)
        {
            var funcType = func.NodeType;
            BinaryExpression newExp;
            Expression leftExp;
            Expression rightExp;

            switch (funcType)
            {
                case ExpressionType.Constant:
                    return Expression.Constant(0.0);
                case ExpressionType.Add:
                    newExp = func as BinaryExpression;
                    leftExp = CommonDifferentiate(
                        Expression.Lambda(newExp.Left, parameters[0]), parameters);
                    rightExp = CommonDifferentiate(
                        Expression.Lambda(newExp.Right, parameters[0]), parameters);
                    return Expression.Add(leftExp, rightExp);
                case ExpressionType.Lambda:
                    var lambda = func as LambdaExpression;
                    return CommonDifferentiate(lambda.Body, lambda.Parameters);
                case ExpressionType.Parameter:
                    return Expression.Constant(1.0);
                case ExpressionType.Call:
                    var callExpression = func as MethodCallExpression;
                    var method = callExpression.Method;
                    switch (method.Name)
                    {
                        case "Cos":
                            {
                                return Expression.Multiply(Expression.Constant(-1.0),
                                    GetMethodDifferential("Sin", callExpression, parameters));
                            }
                        case "Sin":
                            {
                                return GetMethodDifferential("Cos", callExpression, parameters);
                            }
                    }

                    break;
                case ExpressionType.Multiply:
                    newExp = func as BinaryExpression;
                    if (newExp.Left.NodeType == ExpressionType.Constant &&
                        newExp.Right.NodeType == ExpressionType.Parameter)
                        return newExp.Left;
                    if (newExp.Right.NodeType == ExpressionType.Constant &&
                        newExp.Left.NodeType == ExpressionType.Parameter)
                        return newExp.Right;
                    if (newExp.Left.NodeType == ExpressionType.Parameter &&
                        ExpressionType.Parameter == newExp.Right.NodeType)
                        return Expression.Multiply(Expression.Constant(2.0), newExp.Left);
                    leftExp = CommonDifferentiate(
                        Expression.Lambda<Func<double, double>>(newExp.Left, parameters[0]),
                            parameters);
                    rightExp = CommonDifferentiate(
                        Expression.Lambda<Func<double, double>>(newExp.Right, parameters[0]),
                            parameters);
                    return Expression.Add(Expression.Multiply(leftExp,
                            newExp.Right),
                        Expression.Multiply(newExp.Left,
                            rightExp));
            }

            throw new InvalidOperationException();
        }

        private static Expression GetMethodDifferential(string methodName,
            MethodCallExpression callExpression,
            ReadOnlyCollection<ParameterExpression> parameters)
        {
            var sinExpCall =
                Expression.Call(null,
                    typeof(Math).GetMethod(methodName) ?? throw new InvalidOperationException(nameof(methodName)),
                    callExpression.Arguments[0]);
            return Expression.Multiply(
                CommonDifferentiate(callExpression.Arguments[0], parameters), sinExpCall);
        }
    }
}