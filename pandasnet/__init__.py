import clr
import os

lib_file = os.path.join(os.path.dirname(__file__), 'libs', 'PandasNet.dll')
print(lib_file)
clr.AddReference(lib_file)

from PandasNet import Codecs
Codecs.Initialize(False)