﻿namespace Fixie
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Internal;

    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Execute the given method against the given instance of its class.
        /// </summary>
        /// <returns>
        /// For void methods, returns null.
        /// For synchronous methods, returns the value returned by the test method.
        /// For async Task methods, returns null after awaiting the Task.
        /// For async Task<![CDATA[<T>]]> methods, returns the Result T after awaiting the Task.
        /// </returns>
        public static object Execute(this MethodInfo method, object instance, params object[] parameters)
        {
            bool isDeclaredAsync = method.IsAsync();

            if (isDeclaredAsync && method.IsVoid())
                throw new NotSupportedException(
                    "Async void methods are not supported. Declare async methods with a " +
                    "return type of Task to ensure the task actually runs to completion.");

            if (method.ContainsGenericParameters)
                throw new Exception("Could not resolve type parameters for generic method.");

            object result;

            try
            {
                result = method.Invoke(instance, parameters != null && parameters.Length == 0 ? null : parameters);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
            }

            if (result == null)
                return null;

            if (!ConvertibleToTask(result, out var task))
                return result;

            try
            {
                task.Wait();
            }
            catch (AggregateException exception)
            {
                throw new PreservedException(exception.InnerExceptions.First());
            }

            if (method.ReturnType.IsGenericType)
            {
                var property = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);

                return property.GetValue(task, null);
            }

            return null;
        }

        static bool ConvertibleToTask(object result, out Task task)
        {
            if (result is Task t)
            {
                task = t;
                return true;
            }

            var resultType = result.GetType();

            if (IsFSharpAsync(resultType))
            {
                task = ConvertFSharpAsyncToTask(result, resultType);
                return true;
            }

            task = null;
            return false;
        }

        static bool IsFSharpAsync(Type resultType)
        {
            return resultType.IsGenericType &&
                   resultType.GetGenericTypeDefinition().FullName == "Microsoft.FSharp.Control.FSharpAsync`1";
        }

        static Task ConvertFSharpAsyncToTask(object result, Type resultType)
        {
            var startAsTask =
                resultType
                    .Assembly
                    .GetType("Microsoft.FSharp.Control.FSharpAsync")
                    .GetRuntimeMethods()
                    .FirstOrDefault(mi => mi.Name == "StartAsTask");

            if (startAsTask == null)
                throw new InvalidOperationException("Unable to locate F# Control.Async.StartAsTask method");

            return (Task) startAsTask.MakeGenericMethod(resultType.GetGenericArguments())
                .Invoke(null, new[] {result, null, null});
        }
    }
}
