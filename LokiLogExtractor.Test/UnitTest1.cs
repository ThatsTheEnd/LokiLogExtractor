using NUnit.Framework;
using FirstTestWithRider;
using NUnit.Framework.Legacy;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void PassingTest()
        {
            Assert.That(sandbox.Calculator.Add(2,2), Is.EqualTo(4));
        }

        [Test]
        public void FailingTest()
        {
            Assert.That(sandbox.Calculator.Add(2,2), Is.EqualTo(5));
        }
    }
}