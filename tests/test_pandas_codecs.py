import pandasnet
import pandas as pd
import numpy as np
from datetime import datetime
import pytest

from LibForTests import PandasNet as pdnet


def test_basic_dataframe():
    x = pd.DataFrame({
        'A': [1, 2, 3],
        'B': [1.23, 1.24, 1.22],
        'C': ['foo', 'bar', 'other'],
        'D': [datetime(2021, 1, 22), datetime(2021, 1, 23), datetime(2021, 1, 24)],
        'E': [True, False, True]
    })
    y = pdnet.BasicDataFrame(x)
    
    __check(x, y)


def test_datetime64_into_dataframe():
    x = pd.DataFrame({
        'Utc': [
            np.datetime64('2010-03-14T15:00:00.00'),
            np.datetime64('2010-03-15T15:00:00.00'),
            np.datetime64('2010-03-16T15:00:00.00'),
        ],
        'Utc-6': [
            np.datetime64('2010-03-14T15:00:00.00-0600'),
            np.datetime64('2010-03-15T15:00:00.00-0600'),
            np.datetime64('2010-03-16T15:00:00.00-0600'),
        ],
        'Utc+4': [
            np.datetime64('2010-03-14T15:00:00.00+0400'),
            np.datetime64('2010-03-15T15:00:00.00+0400'),
            np.datetime64('2010-03-16T15:00:00.00+0400'),
        ]
    })
    y = pdnet.DateTimeDataFrame(x)
    
    __check(x, y)


def __check(x: pd.DataFrame, y: pd.DataFrame):
    assert x is not y
    assert y is not None
    assert all(x.columns == y.columns)
    for c in x.columns:
        assert all(x[c] == y[c])