// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Narupa.Visualisation.Utility;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Narupa.Visualisation.Tests
{
    internal class ComputeBufferCollectionTests
    {
        private T[] GetData<T>(ComputeBuffer buffer)
        {
            var bufferData = new T[buffer.count];
            buffer.GetData(bufferData);
            return bufferData;
        }

        [Test]
        public void SetIntBuffer()
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var data = new[] { 0, 1, 2, 3, 4 };
                collection.SetBuffer("int_array", data);

                Assert.IsTrue(collection.TryGetValue("int_array", out var buffer));
                Assert.IsNotNull(buffer);
                var bufferData = GetData<int>(buffer);
                Assert.AreEqual(data, bufferData);
            }
        }

        [Test]
        public void SetFloatBuffer()
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var data = new[] { 0f, 1f, -2f, 0.5f };
                collection.SetBuffer("float_array", data);

                Assert.IsTrue(collection.TryGetValue("float_array", out var buffer));
                Assert.IsNotNull(buffer);
                var bufferData = GetData<float>(buffer);
                Assert.AreEqual(data, bufferData);
            }
        }

        [Test]
        public void SetStructBuffer()
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var data = new[]
                {
                    new TestStruct { Field1 = 0, Field2 = 0.5f },
                    new TestStruct { Field1 = 2, Field2 = 1f }
                };
                collection.SetBuffer("struct_array", data);

                Assert.IsTrue(collection.TryGetValue("struct_array", out var buffer));
                Assert.IsNotNull(buffer);
                var bufferData = GetData<TestStruct>(buffer);
                Assert.AreEqual(data, bufferData);
            }
        }

        public static IEnumerable<(int[] array1, int[] array2)> GetUpdateBufferParameters()
        {
            yield return (new[] { 0, 1, 2, 3, 4 }, new[] { 5, 6, 7, 8, 9 });
            yield return (new[] { 0, 1, 2, 3, 4 }, new[] { 5, 6, 7 });
            yield return (new[] { 0, 1, 2 }, new[] { 3, 4, 5, 6, 7 });
        }

        [Test]
        public void UpdateBufferWithNewValues([ValueSource(nameof(GetUpdateBufferParameters))]
                                              (int[] array1, int[] array2) parameter)
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var (data, newdata) = parameter;
                collection.SetBuffer("int_array", data);

                Assert.IsTrue(collection.TryGetValue("int_array", out var buffer));
                Assert.IsNotNull(buffer);
                var bufferData = GetData<int>(buffer);
                Assert.AreEqual(data, bufferData);

                collection.SetBuffer("int_array", newdata);

                Assert.IsTrue(collection.TryGetValue("int_array", out var buffer2));
                Assert.IsNotNull(buffer2);
                bufferData = GetData<int>(buffer2);
                Assert.AreEqual(newdata, bufferData);
            }
        }

        [Test]
        public void CheckNewBufferIsDirty()
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var data = new int[5];
                collection.SetBuffer("int_array", data);

                Assert.AreEqual(1, collection.GetDirtyBuffers().Count());
            }
        }

        [Test]
        public void ClearDirtyBuffers()
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var data = new int[5];
                collection.SetBuffer("int_array", data);

                Assert.AreEqual(1, collection.GetDirtyBuffers().Count());

                collection.ClearDirtyBuffers();

                Assert.AreEqual(0, collection.GetDirtyBuffers().Count());
            }
        }

        [Test]
        public void ApplyDirtyBuffersToShader()
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var data = new[] { 0, 1, 2, 3, 4 };
                collection.SetBuffer("int_array", data);

                var shader = Substitute.For<IGpuProgram>();

                collection.ApplyDirtyBuffersToShader(shader);

                shader.Received(1).SetBuffer(Arg.Is("int_array"), Arg.Any<ComputeBuffer>());
            }
        }

        [Test]
        public void ApplyAllBuffersToShader()
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var data = new[] { 0, 1, 2, 3, 4 };
                collection.SetBuffer("int_array_1", data);
                collection.ClearDirtyBuffers();

                collection.SetBuffer("int_array_2", data);

                var shader = Substitute.For<IGpuProgram>();

                collection.ApplyAllBuffersToShader(shader);

                shader.Received(1).SetBuffer(Arg.Is("int_array_1"), Arg.Any<ComputeBuffer>());
                shader.Received(1).SetBuffer(Arg.Is("int_array_2"), Arg.Any<ComputeBuffer>());
            }
        }

        [Test]
        public void SetBuffer_CorrectDirtyState()
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var data = new[] { 0, 1, 2, 3, 4 };
                collection.SetBuffer("int_array", data);

                Assert.AreEqual(1, collection.GetDirtyBuffers().Count());

                collection.ClearDirtyBuffers();

                Assert.AreEqual(0, collection.GetDirtyBuffers().Count());

                var newdata = new[] { 5, 6, 7, 8, 9 };
                collection.SetBuffer("int_array", newdata);

                Assert.AreEqual(1, collection.GetDirtyBuffers().Count());
            }
        }

        private static ComputeShaderProgram GetComputeShader(string kernelName)
        {
            var shader =
                AssetDatabase.LoadAssetAtPath<ComputeShader>(
                    "Assets/Narupa/Visualisation/Tests/TestComputeBufferCollection.compute");
            var kernel = shader.FindKernel(kernelName);
            return new ComputeShaderProgram(shader, kernel);
        }

        [Test]
        public void UploadIntArrayToGpu()
        {
            ComputeShaderAssert.CanUseComputeShaders();

            using (var collection = new ComputeBufferCollection())
            {
                var data = new[] { 0, 1, 2, 3, 4 };
                collection.SetBuffer("int_array", data);

                var shader = GetComputeShader("UploadIntArray");

                collection.ApplyDirtyBuffersToShader(shader);

                ComputeShaderAssert.RunTests(shader);
            }
        }

        private struct TestStruct
        {
            public int Field1;
            public float Field2;
        }
    }
}