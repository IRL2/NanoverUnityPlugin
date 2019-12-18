// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace Narupa.Testing.Async
{
    /// <summary>
    /// Delegate used by tests that execute code asynchronously
    /// </summary>
    public delegate Task AsyncTestCallback();

    public static class AsyncAssert
    {
        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="callback">A AsyncTestCallback delegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        /// <remarks>
        /// Copied from Assert.Throws in NUnit, with changes to await async execution
        /// </remarks>
        public static async Task<Exception> ThrowsAsync(
            IResolveConstraint expression,
            AsyncTestCallback callback,
            string message,
            params object[] args)
        {
            Exception actual = null;
            try
            {
                await callback();
            }
            catch (Exception ex)
            {
                actual = ex;
            }

            Assert.That(actual, expression, message, args);
            return actual;
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="callback">A AsyncTestCallback delegate</param>
        /// <remarks>
        /// Copied from Assert.Throws in NUnit, with changes to await async execution
        /// </remarks>
        public static async Task<Exception> ThrowsAsync(IResolveConstraint expression,
                                                        AsyncTestCallback callback)
        {
            return await ThrowsAsync(expression, callback, string.Empty, null);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="callback">A AsyncTestCallback delegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        /// <remarks>
        /// Copied from Assert.Throws in NUnit, with changes to await async execution
        /// </remarks>
        public static async Task<Exception> ThrowsAsync(
            Type expectedExceptionType,
            AsyncTestCallback callback,
            string message,
            params object[] args)
        {
            return await ThrowsAsync(
                       new ExceptionTypeConstraint(expectedExceptionType),
                       callback, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="callback">A TestDelegate</param>
        /// <remarks>
        /// Copied from Assert.Throws in NUnit, with changes to await async execution
        /// </remarks>
        public static async Task<Exception> ThrowsAsync(Type expectedExceptionType,
                                                        AsyncTestCallback callback)
        {
            return await ThrowsAsync(
                       new ExceptionTypeConstraint(expectedExceptionType),
                       callback, string.Empty,
                       null);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="callback">A AsyncTestCallback delegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        /// <remarks>
        /// Copied from Assert.Throws in NUnit, with changes to await async execution
        /// </remarks>
        public static async Task<TActual> ThrowsAsync<TActual>(AsyncTestCallback callback,
                                                               string message,
                                                               params object[] args)
            where TActual : Exception
        {
            return (TActual) await ThrowsAsync(typeof(TActual), callback, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="callback">A AsyncTestDelegate delegate</param>
        /// <remarks>
        /// Copied from Assert.Throws in NUnit, with changes to await async execution
        /// </remarks>
        public static async Task<TActual> ThrowsAsync<TActual>(AsyncTestCallback callback)
            where TActual : Exception
        {
            return await ThrowsAsync<TActual>(callback, string.Empty, null);
        }

        /// <summary>
        /// Run the provided test at 100 millisecond intervals, ignoring assertion
        /// exceptions. This allows the test to return early if it passes, and only fail
        /// after a certain timespan has passed.
        /// </summary>
        public static async Task WaitForAssertion(Action test, int delay = 500, int interval = 100)
        {
            var step = Mathf.Min(delay, interval);
            var time = 0;
            while (time < delay)
            {
                await Task.Delay(step);
                try
                {
                    test();
                    return;
                }
                catch (AssertionException exception)
                {
                    // Ignore assertions
                }

                time += step;
            }

            test();
        }
    }
}