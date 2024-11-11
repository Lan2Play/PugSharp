using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace PugSharp.Api.G5Api.Tests.Fixtures;
[CollectionDefinition(nameof(G5ApiFixtureCollection))]
public class G5ApiFixtureCollection : ICollectionFixture<G5ApiFixture>
{
}