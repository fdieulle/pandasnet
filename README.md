# pandasnet

[![build](https://github.com/fdieulle/pandasnet/actions/workflows/build.yml/badge.svg)](https://github.com/fdieulle/pandasnet/actions/workflows/build.yml)
[![release](https://github.com/fdieulle/pandasnet/actions/workflows/release.yml/badge.svg)](https://github.com/fdieulle/pandasnet/actions/workflows/release.yml)

[![license](https://img.shields.io/badge/license-MIT-blue.svg?maxAge=3600)](./LICENSE) 
[![pypi](https://img.shields.io/pypi/v/pandasnet.svg)](https://pypi.org/project/pandasnet/)
[![python supported](https://img.shields.io/pypi/pyversions/pandasnet.svg)](https://pypi.org/project/pandasnet/)

`pandasnet` is a python package build on top of [`pythonnet`](https://pythonnet.github.io/). 
It provides additional data conversions for `pandas`, `numpy` and `datetime`

## Prerequisites

* python 3.6 or higher.
* [dotnet](https://dotnet.microsoft.com/download). 
 
dotnet also provides [scripts](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script) to proceed the installation by command line.

## Installation

```
pip install pandasnet
```

## Features

To load the converter you need to import the package once in your python environment.
If the dotnet clr isn't started yet through the pytonnet package the import will.

```python
import pandasnet
```

We construct a simple C# function to test conversion

```csharp
using System;
using System.Collections.Generic;

namespace LibForTests
{
    public class PandasNet
    {
        public static Dictionary<string, Array> BasicDataFrame(Dictionary<string, Array> df)
            => df;
    }
}
```
We build this function into a library named `LibForTests.dll`.
We load this library into our python environment then use it.

```python
import clr
import pandasnet # Load the converters
import pandas as pd
from datetime import datetime

# Load your dll
clr.AddReference('LibForTests.dll')
from LibForTests import PandasNet as pdnet

x = pd.DataFrame({
    'A': [1, 2, 3],
    'B': [1.23, 1.24, 1.22],
    'C': ['foo', 'bar', 'other'],
    'D': [datetime(2021, 1, 22), datetime(2021, 1, 23), datetime(2021, 1, 24)]
})
y = pdnet.BasicDataFrame(x)

print(y)
```


Below an exhausitve list of supported data convertions.

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

## Contributing

Issue tracker: [https://github.com/fdieulle/pandasnet/issues](https://github.com/fdieulle/pandasnet/issues)

If you want to checkout the project and propose your own contribution, you will need to setup it following few steps:

### Create a virtual environment:

```
python -m venv venv
```

### Activate your virtual environment:

```
venv/Scripts/activate
```

### Install package dependencies

```
pip install -r requirements.txt
```

## License

This project is open source under the [MIT license](./LICENSE).