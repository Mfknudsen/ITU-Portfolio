#region Libraries

using NUnit.Framework;
using Runtime.AI.Navigation;
using Runtime.Core;
using Runtime.Systems.Pooling;

#endregion

namespace Tests
{
    /// <summary>
    /// Base class containing all needed setups and tears downs to ensure no test spill into each other.
    /// </summary>
    public class BaseTestSetup
    {
        [SetUp]
        public void SetUp()
        {
            Pool.InitializeForTests();
            UnitNavigation.InitializeForTest();
            TimerUpdater.InitializeForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Pool.ResetForTests();
            UnitNavigation.ClearForTests();
            TimerUpdater.ClearForTests();
        }
    }
}