// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Narupa.Visualisation.Utility;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Narupa.Visualisation.Tests
{
    /// <summary>
    /// Utility method for running unit tests using <see cref="ComputeBuffer" />'s and
    /// <see cref="ComputeShader" />'s.
    /// </summary>
    public static class ComputeShaderAssert
    {
        /// <summary>
        /// Maximum number of assertions of a certain type that can occur in a single test.
        /// Limit is needed as the
        /// buffer to store results must be initialised with a certain size
        /// </summary>
        private const int MaxAssertionCount = 20;

        private static IEnumerable<object> GetAssertions<TAssertion>(ComputeBuffer buffer,
                                                                     int count)
            where TAssertion : new()
        {
            Assert.LessOrEqual(count, MaxAssertionCount, "Too many shader assertions!");

            var array = new TAssertion[count];
            buffer.GetData(array);
            return array.AsEnumerable().Cast<object>();
        }

        /// <summary>
        /// List of kinds of assertions. Allows code reuse for getting assertions and
        /// checking they are true
        /// </summary>
        private static readonly AssertionType[] AssertionTypes =
        {
            new AssertionType
            {
                BufferName = "AssertionEqualFloatBuffer",
                Type = typeof(AssertionEqualFloat),
                Assert = (o, shaderPath) =>
                {
                    var assertion = (AssertionEqualFloat) o;
                    Assert.That<object>(assertion.Actual,
                                        Is.EqualTo(assertion.Expected).Within(1e-5f),
                                        $"Assertion failed on line {assertion.LineNumber} of {shaderPath}",
                                        null);
                },
                GetAssertions = GetAssertions<AssertionEqualFloat>
            },
            new AssertionType
            {
                BufferName = "AssertionEqualVectorBuffer",
                Type = typeof(AssertionEqualVector),
                Assert = (o, shaderPath) =>
                {
                    var assertion = (AssertionEqualVector) o;
                    Assert.That<object>((assertion.Expected - assertion.Actual).magnitude,
                                        Is.EqualTo(0f).Within(1e-5f),
                                        $"Assertion failed on line {assertion.LineNumber} of {shaderPath}",
                                        null);
                },
                GetAssertions = GetAssertions<AssertionEqualVector>
            },
            new AssertionType
            {
                BufferName = "AssertionEqualMatrixBuffer",
                Type = typeof(AssertionEqualMatrix),
                Assert = (o, shaderPath) =>
                {
                    var assertion = (AssertionEqualMatrix) o;
                    for (var x = 0; x < 4; x++)
                    for (var y = 0; y < 4; y++)
                        Assert.That<object>(assertion.Actual[x, y],
                                            Is.EqualTo(assertion.Expected[x, y]).Within(1e-5f),
                                            $"Assertion failed on line {assertion.LineNumber} of {shaderPath}. Item[{x}{y}] not equal.",
                                            null);
                },
                GetAssertions = GetAssertions<AssertionEqualMatrix>
            }
        };

        /// <summary>
        /// Get all the kernel names that are present in the compute shader
        /// </ summary>
        /// <remarks>
        /// Reflection has to be used to access internal Unity methods that allow all
        /// kernels (programs) in a
        /// compute shader to be iterate
        /// </remarks>
        [NotNull]
        public static string[] GetAllKernels(string shaderPath)
        {
            var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(shaderPath);

            Func<ComputeShader, int> GetComputeShaderPlatformCount = cs =>
                (int) typeof(ShaderUtil)
                      .GetMethod("GetComputeShaderPlatformCount",
                                 BindingFlags.Static
                               | BindingFlags.NonPublic)
                      .Invoke(null, new object[] { cs });

            Func<ComputeShader, int, int> GetComputeShaderPlatformKernelCount =
                (cs, platformIndex) =>
                    (int) typeof(ShaderUtil)
                          .GetMethod("GetComputeShaderPlatformKernelCount",
                                     BindingFlags.Static
                                   | BindingFlags.NonPublic)
                          .Invoke(null, new object[] { cs, platformIndex });

            Func<ComputeShader, int, int, string> GetComputeShaderPlatformKernelName =
                (cs, platformIndex, kernelIndex) =>
                    (string) typeof(ShaderUtil)
                             .GetMethod("GetComputeShaderPlatformKernelName",
                                        BindingFlags.Static
                                       |
                                        BindingFlags.NonPublic)
                             .Invoke(
                                 null,
                                 new object[] { cs, platformIndex, kernelIndex });

            var list = new List<string>();

            var platformCount = GetComputeShaderPlatformCount(shader);
            for (var platformIndex = 0; platformIndex < platformCount; platformIndex++)
            {
                var kernelCount = GetComputeShaderPlatformKernelCount(shader, platformIndex);
                for (var kernelIndex = 0; kernelIndex < kernelCount; kernelIndex++)
                {
                    var kernelName = GetComputeShaderPlatformKernelName(shader,
                                                                        platformIndex,
                                                                        kernelIndex);
                    list.Add(kernelName);
                }
            }


            return list.ToArray();
        }

        /// <summary>
        /// Execute all the tests for the given kernel for the given shader
        /// </summary>
        public static void RunTests(object src, string shaderPath, string kernelName)
        {
            CanUseComputeShaders();

            var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(shaderPath);

            var kernel = shader.FindKernel(kernelName);

            var program = new ComputeShaderProgram(shader, kernel);

            var method = src.GetType().GetMethod(kernelName);
            if (method != null)
                method?.Invoke(
                    src, new object[] { program, (Action<ComputeShaderProgram>) RunTests });
            else
                RunTests(program);
        }

        public static void RunTests(ComputeShaderProgram shader)
        {
            CanUseComputeShaders();

            if (!SystemInfo.supportsComputeShaders)
                return;

            Assert.IsNotNull(shader);


            var buffers = new Dictionary<AssertionType, ComputeBuffer>();

            foreach (var assertionType in AssertionTypes)
            {
                buffers[assertionType] = new ComputeBuffer(MaxAssertionCount,
                                                           Marshal.SizeOf(assertionType.Type),
                                                           ComputeBufferType.Append);
                buffers[assertionType].SetCounterValue(0);
            }


            try
            {
                foreach (var assertionType in AssertionTypes)
                    shader.SetBuffer(assertionType.BufferName, buffers[assertionType]);

                shader.Dispatch(1, 1, 1);

                foreach (var assertionType in AssertionTypes)
                    using (var countBuffer =
                        new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments))
                    {
                        // Get the number of items in the corresponding compute buffer
                        var counter = new[] { 0 };
                        ComputeBuffer.CopyCount(buffers[assertionType], countBuffer, 0);
                        countBuffer.GetData(counter);
                        var count = counter[0];

                        var assertions = assertionType.GetAssertions(buffers[assertionType], count);

                        foreach (var assertion in assertions)
                            assertionType.Assert(assertion, shader.ShaderPath);
                    }
            }
            finally
            {
                foreach (var buffer in buffers.Values)
                    buffer.Dispose();
            }
        }

        /// <summary>
        /// Assert that compute shaders can be used on this platform.
        /// </summary>
        public static void CanUseComputeShaders()
        {
            if (!SystemInfo.supportsComputeShaders)
                Assert.Ignore("Compute Shaders not Supported");
        }

        /// <summary>
        /// Represents an assertion that Expected and Actual are equal
        /// </summary>
        public struct AssertionEqualFloat
        {
            public float Expected;
            public float Actual;
            public int LineNumber;
        }

        /// <summary>
        /// Represents an assertion that Expected and Actual are equal
        /// </summary>
        public struct AssertionEqualVector
        {
            public Vector4 Expected;
            public Vector4 Actual;
            public int LineNumber;
        }

        /// <summary>
        /// Represents an assertion that Expected and Actual are equal
        /// </summary>
        public struct AssertionEqualMatrix
        {
            public Matrix4x4 Expected;
            public Matrix4x4 Actual;
            public int LineNumber;
        }

        /// <summary>
        /// Defines a kind of assertion (float equality, vector equality etc.)
        /// </summary>
        public struct AssertionType
        {
            /// <summary>
            /// The name of the buffer. This is defined in Test.cginc
            /// </summary>
            public string BufferName;

            /// <summary>
            /// The type of the assertion. This should be a struct, so it can be copied to and
            /// from the GPU
            /// </summary>
            public Type Type;

            /// <summary>
            /// Take an object representing an assertion and assert that it is true
            /// </summary>
            public Action<object, string> Assert;

            /// <summary>
            /// Get the list of assertions from the GPU
            /// </summary>
            public Func<ComputeBuffer, int, IEnumerable<object>> GetAssertions;
        }
    }
}