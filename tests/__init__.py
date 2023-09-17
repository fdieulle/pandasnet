import os
from pythonnet import load
import sys

load("coreclr")

import clr

lib_folder = os.path.join(os.path.dirname(__file__), 'libs')
if lib_folder not in sys.path:
    sys.path.append(lib_folder)

clr.AddReference('LibForTests')