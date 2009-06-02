// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Remotion.Development.UnitTesting;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class SelectManyExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod_WithoutPosition ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.SelectMany (i => new[] { 1, 2, 3 }, (i, j) => new { i, j }));
      Assert.That (SelectManyExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void QuerySourceType ()
    {
      SelectManyExpressionNode node = ExpressionNodeObjectMother.CreateSelectMany(SourceStub);

      Assert.That (node.QuerySourceElementType, Is.SameAs (typeof (Student_Detail)));
    }

    [Test]
    public void Resolve_ReplacesParameter_WithProjection ()
    {
      var node = new SelectManyExpressionNode (
          SourceStub,
          ExpressionHelper.CreateLambdaExpression (),
          ExpressionHelper.CreateLambdaExpression<int, int, AnonymousType> ((a, b) => new AnonymousType (a, b)));

      var expression = ExpressionHelper.CreateLambdaExpression<AnonymousType, bool> (i => i.a > 5 && i.b > 6);
      var result = node.Resolve (expression.Parameters[0], expression.Body);
      Console.WriteLine (result);
      
      var selectManySourceReference = new IdentifierReferenceExpression (node);

      // new AnonymousType (SourceReference, selectManySourceReference).a > 5 && new AnonymousType (SourceReference, selectManySourceReference).b > 6

      var newAnonymousTypeExpression = Expression.New (typeof (AnonymousType).GetConstructor(new[] {typeof (int), typeof (int) }), SourceReference, selectManySourceReference);
      var anonymousTypeMemberAExpression = Expression.MakeMemberAccess (newAnonymousTypeExpression, typeof (AnonymousType).GetProperty ("a"));
      var anonymousTypeMemberBExpression = Expression.MakeMemberAccess (newAnonymousTypeExpression, typeof (AnonymousType).GetProperty ("b"));

      var expectedResult = Expression.MakeBinary (ExpressionType.AndAlso,
        Expression.MakeBinary (ExpressionType.GreaterThan, anonymousTypeMemberAExpression, Expression.Constant (5)),
        Expression.MakeBinary (ExpressionType.GreaterThan, anonymousTypeMemberBExpression, Expression.Constant (6)));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedResultSelector ()
    {
      var collectionSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var resultSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SelectManyExpressionNode (SourceStub, collectionSelector, resultSelector);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedResultSelector ();

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedCollectionSelector ()
    {
      var collectionSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var resultSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SelectManyExpressionNode (SourceStub, collectionSelector, resultSelector);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedCollectionSelector ();

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedResultSelector_Cached ()
    {
      var sourceMock = new MockRepository ().StrictMock<IExpressionNode> ();
      var collectionSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var resultSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SelectManyExpressionNode (sourceMock, collectionSelector, resultSelector);
      var expectedResult = ExpressionHelper.CreateLambdaExpression ();

      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything)).Repeat.Once ().Return (expectedResult);

      sourceMock.Replay ();

      node.GetResolvedResultSelector ();
      node.GetResolvedResultSelector ();

      sourceMock.VerifyAllExpectations ();
    }

    [Test]
    public void GetResolvedCollectionSelector_Cached ()
    {
      var sourceMock = new MockRepository ().StrictMock<IExpressionNode> ();
      var collectionSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var resultSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SelectManyExpressionNode (sourceMock, collectionSelector, resultSelector);
      var expectedResult = ExpressionHelper.CreateLambdaExpression ();

      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything)).Repeat.Once ().Return (expectedResult);

      sourceMock.Replay ();

      node.GetResolvedCollectionSelector ();
      node.GetResolvedCollectionSelector ();

      sourceMock.VerifyAllExpectations ();
    }
  }
}