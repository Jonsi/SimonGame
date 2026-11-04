using System.Linq;
using NUnit.Framework;
using SimonDuel.Rules;

namespace SimonDuel.Tests
{
    public class PadsTests
    {
        [Test]
        public void ThereAreFourDifferentPads()
        {
            Assert.AreEqual(4, Pads.All.Distinct().Count());
        }
    }
}
