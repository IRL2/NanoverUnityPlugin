// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

/// Structs and functions used for unit testing GPU logic

/// Represents an assertion that two floats are equal.
struct AssertionEqualFloat {
    float expected;
    float actual;
    int linenumber;
};

/// Buffer which allows AssertionEqualFloat structs to be copied to the CPU to be asserted
AppendStructuredBuffer<AssertionEqualFloat> AssertionEqualFloatBuffer;

/// Asserts that two floats are equal
void assert_equal(float expected, float actual, int linenumber) {
    AssertionEqualFloat assertion;
    assertion.expected = expected;
    assertion.actual = actual;
    assertion.linenumber = linenumber;
    AssertionEqualFloatBuffer.Append(assertion);
};

/// Represents an assertion that two vectors are equal.
struct AssertionEqualVector {
    float4 expected;
    float4 actual;
    int linenumber;
};

/// Buffer which allows AssertionEqualVector structs to be copied to the CPU to be asserted
AppendStructuredBuffer<AssertionEqualVector> AssertionEqualVectorBuffer;

/// Asserts that two vectors are equal
void assert_equal(float4 expected, float4 actual, int linenumber) {
    AssertionEqualVector assertion;
    assertion.expected = expected;
    assertion.actual = actual;
    assertion.linenumber = linenumber;
    AssertionEqualVectorBuffer.Append(assertion);
};


/// Represents an assertion that two matrices are equal.
struct AssertionEqualMatrix {
    float4x4 expected;
    float4x4 actual;
    int linenumber;
};

/// Buffer which allows AssertionEqualMatrix structs to be copied to the CPU to be asserted
AppendStructuredBuffer<AssertionEqualMatrix> AssertionEqualMatrixBuffer;

/// Asserts that two vectors are equal
void assert_equal(float4x4 expected, float4x4 actual, int linenumber) {
    AssertionEqualMatrix assertion;
    assertion.expected = expected;
    assertion.actual = actual;
    assertion.linenumber = linenumber;
    AssertionEqualMatrixBuffer.Append(assertion);
};