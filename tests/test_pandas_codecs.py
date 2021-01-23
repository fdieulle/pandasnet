import pandasnet
import os
import pandas as pd
from datetime import datetime
import clr

lib_file = os.path.join(os.path.dirname(__file__), 'libs', 'LibForTests.dll')
clr.AddReference(lib_file)

from LibForTests import PandasNet as pdnet


def test_basic_dataframe():
    x = pd.DataFrame({
        'A': [1, 2, 3],
        'B': [1.23, 1.24, 1.22],
        'C': ['foo', 'bar', 'other'],
        'D': [datetime(2021, 1, 22), datetime(2021, 1, 23), datetime(2021, 1, 24)]
    })
    y = pdnet.BasicDataFrame(x)
    
    __check(x, y)


def __check(x: pd.DataFrame, y: pd.DataFrame):
    assert x is not y
    assert y is not None
    assert all(x.columns == y.columns)
    for c in x.columns:
        assert all(x[c] == y[c])