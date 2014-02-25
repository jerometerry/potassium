namespace Sodium.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class PriorityQueueTests
    {
        [TestCase(50000)]
        public void TestPriorityQueue(int numOperations)
        {
            var rand = new Random(0);
            var pq = new PriorityQueue<Employee>();
            for (var op = 0; op < numOperations; ++op)
            {
                var type = rand.Next(0, 2);

                if (type == 0) 
                {
                    // enqueue
                    var lastName = op + "man";
                    var priority = ((100.0 - 1.0) * rand.NextDouble()) + 1.0;
                    pq.Add(new Employee(lastName, priority));
                    if (pq.IsConsistent() == false)
                    {
                        Assert.Fail("Test fails after enqueue operation # " + op);
                    }
                }
                else 
                {
                    // dequeue
                    if (pq.Count() > 0)
                    {
                        pq.Remove();
                        if (pq.IsConsistent() == false)
                        {
                            Assert.Fail("Test fails after dequeue operation # " + op);
                        }
                    }
                }
            }
        }

        private class Employee : IComparable<Employee>
        {
            private readonly string lastName;
            private readonly double priority; // smaller values are higher priority

            public Employee(string lastName, double priority)
            {
                this.lastName = lastName;
                this.priority = priority;
            }

            public override string ToString()
            {
                return "(" + this.lastName + ", " + this.priority.ToString("F1") + ")";
            }

            public int CompareTo(Employee other)
            {
                return this.priority.CompareTo(other.priority);
            }
        }
    } 
}
