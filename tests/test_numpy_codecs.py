import pandasnet
import numpy as np
import pytest

from LibForTests import NumpyNet as npnet


@pytest.mark.skip(reason="pythonnet do not allow override")
def test_int8_codec():
    x = np.array([1, 2, 3], dtype='int8')
    y = npnet.OneDimensionInt8Array(x)
    __check(x, y)

@pytest.mark.skip(reason="pythonnet do not allow override")
def test_int16_codec():
    x = np.array([1, 2, 3], dtype='int16')
    y = npnet.OneDimensionInt16Array(x)
    __check(x, y)

@pytest.mark.skip(reason="pythonnet do not allow override")
def test_int32_codec():
    x = np.array([1, 2, 3])
    y = npnet.OneDimensionInt32Array(x)
    __check(x, y)

@pytest.mark.skip(reason="pythonnet do not allow override")
def test_int64_codec():
    x = np.array([1, 2, 3], dtype='int64')
    y = npnet.OneDimensionInt64Array(x)
    __check(x, y)

@pytest.mark.skip(reason="pythonnet do not allow override")
def test_uint8_codec():
    x = np.array([1, 2, 3], dtype='uint8')
    y = npnet.OneDimensionUInt8Array(x)
    __check(x, y)

@pytest.mark.skip(reason="pythonnet do not allow override")
def test_uint16_codec():
    x = np.array([1, 2, 3], dtype='uint16')
    y = npnet.OneDimensionUInt16Array(x)
    __check(x, y)
@pytest.mark.skip(reason="pythonnet do not allow override")
def test_uint32_codec():
    x = np.array([1, 2, 3], dtype='uint32')
    y = npnet.OneDimensionUInt32Array(x)
    __check(x, y)

@pytest.mark.skip(reason="pythonnet do not allow override")
def test_uint64_codec():
    x = np.array([1, 2, 3], dtype='uint64')
    y = npnet.OneDimensionUInt64Array(x)
    __check(x, y)

@pytest.mark.skip(reason="pythonnet do not allow override")
def test_float32_codec():
    x = np.array([1, 2, 3], dtype='float32')
    y = npnet.OneDimensionFloat32Array(x)
    __check(x, y)

@pytest.mark.skip(reason="pythonnet do not allow override")
def test_float64_codec():
    x = np.array([1, 2, 3], dtype='float64')
    y = npnet.OneDimensionFloat64Array(x)
    __check(x, y)

@pytest.mark.skip(reason="pythonnet do not allow override")
def test_string_codec():
    x = np.array(['foo_1', 'foo_2', 'foo_3'], dtype='str')
    y = npnet.OneDimensionStringArray(x)
    __check(x, y)


def __check(x: np.array, y: np.array):
    assert x is not y
    assert x.dtype == y.dtype
    assert all(x == y)