// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using System.Reflection;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.EagerFetching
{
  [TestFixture]
  public class FetchOneRequestTest
  {
    private MemberInfo _substitutionMember;
    private FetchOneRequest _substitutionFetchRequest;

    [SetUp]
    public void SetUp ()
    {
      _substitutionMember = typeof (Cook).GetProperty ("Substitution");
      _substitutionFetchRequest = new FetchOneRequest (_substitutionMember);
    }

    [Test]
    public void ModifyFetchQueryModel ()
    {
      var inputFetchQuery = from fetch0 in
                              (from sd in ExpressionHelper.CreateKitchenQueryable () select sd.Cook).Take (1)
                            select fetch0;
      var fetchQueryModel = ExpressionHelper.ParseQuery (inputFetchQuery);

      // expected: from fetch0 in (from sd in ExpressionHelper.CreateKitchenQueryable () select sd.Cook)
      //           select fetch0.Substitution;

      PrivateInvoke.InvokeNonPublicMethod (_substitutionFetchRequest, "ModifyFetchQueryModel", fetchQueryModel);

      var selectClause = fetchQueryModel.SelectClause;
      var expectedExpression = ExpressionHelper.Resolve<Cook, Cook> (fetchQueryModel.MainFromClause, s => s.Substitution);
      ExpressionTreeComparer.CheckAreEqualTrees (selectClause.Selector, expectedExpression);
    }

    [Test]
    public void Clone ()
    {
      var clone = _substitutionFetchRequest.Clone (new CloneContext (new QuerySourceMapping ()));

      Assert.That (clone, Is.Not.SameAs (_substitutionFetchRequest));
      Assert.That (clone, Is.InstanceOfType (typeof (FetchOneRequest)));
      Assert.That (((FetchOneRequest) clone).RelationMember, Is.SameAs (_substitutionFetchRequest.RelationMember));
      Assert.That (((FetchOneRequest) clone).InnerFetchRequests.ToArray(), Is.Empty);
    }

    [Test]
    public void Clone_WithInnerFetchRequests ()
    {
      var innerRequest = new FetchOneRequest (_substitutionMember);
      _substitutionFetchRequest.GetOrAddInnerFetchRequest (innerRequest);

      var clone = _substitutionFetchRequest.Clone (new CloneContext (new QuerySourceMapping ()));
      var innerClones = ((FetchOneRequest) clone).InnerFetchRequests.ToArray ();
      Assert.That (innerClones.Length, Is.EqualTo (1));
      Assert.That (innerClones[0], Is.InstanceOfType (typeof (FetchOneRequest)));
      Assert.That (innerClones[0], Is.Not.SameAs (innerRequest));
      Assert.That (innerClones[0].RelationMember, Is.SameAs (innerRequest.RelationMember));
    }
  }
}