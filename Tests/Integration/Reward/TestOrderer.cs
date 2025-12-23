using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace HrManagement.Api.Tests.Integration.Reward;

/// <summary>
/// Custom attribute to specify test priority.
/// Lower numbers run first.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestPriorityAttribute : Attribute
{
    public int Priority { get; }

    public TestPriorityAttribute(int priority)
    {
        Priority = priority;
    }
}

/// <summary>
/// Custom test case orderer that orders tests by priority.
/// Tests with [TestPriority(1)] run before [TestPriority(2)], etc.
/// Tests without priority attribute default to priority 100.
/// </summary>
public class PriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

        foreach (var testCase in testCases)
        {
            var priority = 100; // Default priority

            var priorityAttribute = testCase.TestMethod.Method
                .GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName)
                .FirstOrDefault();

            if (priorityAttribute != null)
            {
                priority = priorityAttribute.GetNamedArgument<int>("Priority");
            }

            if (!sortedMethods.ContainsKey(priority))
            {
                sortedMethods[priority] = new List<TTestCase>();
            }

            sortedMethods[priority].Add(testCase);
        }

        foreach (var group in sortedMethods.Keys)
        {
            foreach (var testCase in sortedMethods[group])
            {
                yield return testCase;
            }
        }
    }
}

/// <summary>
/// Custom test collection orderer that orders collections alphabetically.
/// Use naming convention: "1_QueryTests", "2_CommandTests" to control order.
/// </summary>
public class CollectionPriorityOrderer : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
    {
        return testCollections.OrderBy(c => c.DisplayName);
    }
}
