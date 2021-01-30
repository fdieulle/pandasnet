# pandasnet

[![Build Status](https://travis-ci.com/fdieulle/pandasnet.svg?branch=main)](https://travis-ci.com/github/fdieulle/pandasnet)
[![codecov](https://codecov.io/gh/fdieulle/pandasnet/branch/main/graph/badge.svg?token=A2N3JMEPN6)](https://codecov.io/gh/fdieulle/pandasnet)

[![license](https://img.shields.io/badge/license-MIT-blue.svg?maxAge=3600)](./LICENSE) 
[![pypi](https://img.shields.io/pypi/v/pandasnet.svg)](https://pypi.org/project/pandasnet/)
[![python supported](https://img.shields.io/pypi/pyversions/pandasnet.svg)](https://pypi.org/project/pandasnet/)

`pandasnet` si a python package build on top of [`pythonnet`](https://pythonnet.github.io/). 
It provides additional data conversions for `pandas`, `numpy` and `datetime`

## Prerequisites

* python 3.6 or higher
* dotnet

## Installation

```
pip install pandasnet
```

## Features

To load the converter you need to import the pacakge once in your python environment.
If the dotnet clr isn't started yet through the pytonnet package the import will.

```{python}
import pandasnet
```

Below you can found an exhausitve list of supported data convertions.

### Python -> .Net

|Python                                  |.Net                     |
|----------------------------------------|-------------------------|
|datetime.datetime                       |DateTime                 |
|datetime.date                           |DateTime                 |
|datetime.timedelta                      |TimeSpan                 |
|datetime.time                           |TimeSpan                 |
|numpy.ndarray(dtype=bool_)              |bool[]                   |
|numpy.ndarray(dtype=int8)               |sbyte[]                  |
|numpy.ndarray(dtype=int16)              |short[]                  |
|numpy.ndarray(dtype=int32)              |int[]                    |
|numpy.ndarray(dtype=int64)              |long[]                   |
|numpy.ndarray(dtype=uint8)              |byte[]                   |
|numpy.ndarray(dtype=uint16)             |ushort[]                 |
|numpy.ndarray(dtype=uint32)             |uint[]                   |
|numpy.ndarray(dtype=uint64)             |ulong[]                  |
|numpy.ndarray(dtype=float32)            |float[]                  |
|numpy.ndarray(dtype=float64)            |double[]                 |
|numpy.ndarray(dtype=datetime64)         |DateTime[]               |
|numpy.ndarray(dtype=timedelta64)        |TimeSpan[]               |
|numpy.ndarray(dtype=str)                |string[]                 |
|pandas._libs.tslibs.timestamps.Timestamp|DateTime                 |
|pandas._libs.tslibs.timedeltas.TimeDelta|TimeSpan                 |
|pandas.core.series.Series               |Array                    |
|pandas.core.frame.DataFrame             |Dictionary[string, Array]|

### .Net -> Python

|.Net                     |Python                          |
|-------------------------|--------------------------------|
|DateTime                 |datetime.datetime               |
|TimeSpan                 |datetime.timedelta              |
|bool[]                   |numpy.ndarray(dtype=bool_)      |
|sbyte[]                  |numpy.ndarray(dtype=int8)       |
|short[]                  |numpy.ndarray(dtype=int16)      |
|int[]                    |numpy.ndarray(dtype=int32)      |
|long[]                   |numpy.ndarray(dtype=int64)      |
|byte[]                   |numpy.ndarray(dtype=uint8)      |
|ushort[]                 |numpy.ndarray(dtype=uint16)     |
|uint[]                   |numpy.ndarray(dtype=uint32)     |
|ulong[]                  |numpy.ndarray(dtype=uint64)     |
|float[]                  |numpy.ndarray(dtype=float32)    |
|double[]                 |numpy.ndarray(dtype=float64)    |
|DateTime[]               |numpy.ndarray(dtype=datetime64) |
|TimeSpan[]               |numpy.ndarray(dtype=timedelta64)|
|Dictionary[string, Array]|pandas.core.frame.DataFrame     |
