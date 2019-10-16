// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using JetBrains.Annotations;
using NUnit.Framework;

namespace Narupa.Visualisation.Tests
{
    internal class TransformationTests
    {
        private static string ShaderPath =>
            "Assets/Narupa/Visualisation/Tests/TransformationTests.compute";

        [NotNull]
        private static string[] GetKernels =>
            ComputeShaderAssert.GetAllKernels(ShaderPath);

        [Test]
        public void Test([ValueSource(nameof(GetKernels))] string kernelName)
        {
            ComputeShaderAssert.RunTests(this, ShaderPath, kernelName);
        }
    }
}