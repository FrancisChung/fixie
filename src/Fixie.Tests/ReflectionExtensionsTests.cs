﻿namespace Fixie.Tests
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Assertions;

    public class ReflectionExtensionsTests
    {
        public void CanDetermineTheTypeNameOfAnyObject()
        {
            5.TypeName().ShouldBe("System.Int32");
            "".TypeName().ShouldBe("System.String");
            ((string) null).TypeName().ShouldBe(null);
        }

        public void CanDetectVoidReturnType()
        {
            Method("ReturnsVoid").IsVoid().ShouldBe(true);
            Method("ReturnsInt").IsVoid().ShouldBe(false);
        }

        public void CanDetectStaticTypes()
        {
            typeof(StaticClass).IsStatic().ShouldBe(true);
            typeof(AbstractClass).IsStatic().ShouldBe(false);
            typeof(ConcreteClass).IsStatic().ShouldBe(false);
            typeof(SealedConcreteClass).IsStatic().ShouldBe(false);
        }

        public void CanDetectClassAttributes()
        {
            typeof(AttributeSample).Has<InheritedAttribute>().ShouldBe(true);
            typeof(AttributeSample).Has<NonInheritedAttribute>().ShouldBe(true);
            typeof(AttributeSample).Has<AttributeUsageAttribute>().ShouldBe(false);
        }

        public void CanDetectMethodAttributes()
        {
            Method<AttributeSample>("AttributeOnBaseDeclaration").Has<SampleMethodAttribute>().ShouldBe(true);
            Method<AttributeSample>("AttributeOnOverrideDeclaration").Has<SampleMethodAttribute>().ShouldBe(true);
            Method<AttributeSample>("NoAttrribute").Has<SampleMethodAttribute>().ShouldBe(false);
        }

        public void CanDetectAsyncDeclarations()
        {
            Method("ReturnsVoid").IsAsync().ShouldBe(false);
            Method("ReturnsInt").IsAsync().ShouldBe(false);
            Method("Async").IsAsync().ShouldBe(true);
        }

        public void CanDetectWhetherTypeIsWithinNamespace()
        {
            var opCode = typeof(System.Reflection.Emit.OpCode);

            opCode.IsInNamespace(null).ShouldBe(false);
            opCode.IsInNamespace("").ShouldBe(false);
            opCode.IsInNamespace("System").ShouldBe(true);
            opCode.IsInNamespace("Sys").ShouldBe(false);
            opCode.IsInNamespace("System.").ShouldBe(false);

            opCode.IsInNamespace("System.Reflection").ShouldBe(true);
            opCode.IsInNamespace("System.Reflection.Emit").ShouldBe(true);
            opCode.IsInNamespace("System.Reflection.Emit.OpCode").ShouldBe(false);
            opCode.IsInNamespace("System.Reflection.Typo").ShouldBe(false);
        }

        public void CanDisposeDisposables()
        {
            var disposeable = new Disposable();
            var disposeButNotDisposable = new DisposeButNotDisposable();
            var notDisposable = new NotDisposable();
            object nullObject = null;

            disposeable.Invoked.ShouldBe(false);
            disposeButNotDisposable.Invoked.ShouldBe(false);
            notDisposable.Invoked.ShouldBe(false);

            ((object)disposeable).Dispose();
            ((object)disposeButNotDisposable).Dispose();
            notDisposable.Dispose();
            nullObject.Dispose();

            disposeable.Invoked.ShouldBe(true);
            disposeButNotDisposable.Invoked.ShouldBe(false);
            notDisposable.Invoked.ShouldBe(false);
        }

        void ReturnsVoid() { }
        int ReturnsInt() { return 0; }
        async Task Async() { await Task.Run(() => { }); }

        class SampleMethodAttribute : Attribute { }
        class InheritedAttribute : Attribute { }
        class NonInheritedAttribute : Attribute { }

        static class StaticClass { }
        abstract class AbstractClass { }
        class ConcreteClass { }
        sealed class SealedConcreteClass { }

        [Inherited]
        abstract class AttributeSampleBase
        {
            [SampleMethod]
            public virtual void AttributeOnBaseDeclaration() { }
            public virtual void AttributeOnOverrideDeclaration() { }
            public virtual void NoAttrribute() { }
        }

        [NonInherited]
        class AttributeSample : AttributeSampleBase
        {
            public override void AttributeOnBaseDeclaration() { }
            [SampleMethod]
            public override void AttributeOnOverrideDeclaration() { }
            public override void NoAttrribute() { }
        }

        static MethodInfo Method(string name)
            => Method<ReflectionExtensionsTests>(name);

        static MethodInfo Method<T>(string name)
            => typeof(T).GetInstanceMethod(name);

        class Disposable : IDisposable
        {
            public bool Invoked { get; private set; }
            public void Dispose() => Invoked = true;
        }

        class DisposeButNotDisposable
        {
            public bool Invoked { get; private set; }
            public void Dispose() => Invoked = true;
        }

        class NotDisposable
        {
            public bool Invoked { get; private set; }
        }
    }
}