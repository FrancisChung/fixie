﻿namespace Fixie.Tests
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Should;
    using Should.Core.Exceptions;
    using static Utility;

    public abstract class MessagingTests
    {
        readonly Convention convention;

        protected MessagingTests()
        {
            convention = new Convention();

            convention
                .Classes
                .Where(testClass => testClass == typeof(SampleTestClass));

            convention
                .ClassExecution
                .SortCases((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));

            convention
                .CaseExecution
                .Skip(
                    x => x.Method.Has<SkipAttribute>(),
                    x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);

            convention.
                HideExceptionDetails.For<EqualException>();

            TestClass = FullName<SampleTestClass>();
        }

        protected string TestClass { get; }

        protected void Run(object listener)
            => Run<SampleTestClass>(listener, convention);

        class SampleTestClass
        {
            public void Pass()
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void FailByAssertion()
            {
                WhereAmI();
                1.ShouldEqual(2);
            }

            [Skip]
            public void SkipWithoutReason()
            {
                throw new ShouldBeUnreachableException();
            }

            [Skip("Skipped with reason.")]
            public void SkipWithReason()
            {
                throw new ShouldBeUnreachableException();
            }

            static void WhereAmI([CallerMemberName] string member = null)
            {
                Console.Out.WriteLine("Console.Out: " + member);
                Console.Error.WriteLine("Console.Error: " + member);
            }
        }

        protected static string At(string method)
            => At<SampleTestClass>(method);

        protected static string TestClassPath()
            => PathToThisFile();

        static string PathToThisFile([CallerFilePath] string path = null)
            => path;
    }
}