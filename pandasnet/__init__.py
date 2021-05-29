import os, sys

lib_folder = os.path.join(os.path.dirname(__file__), 'libs')
sys.path.append(lib_folder)

import clr
clr.AddReference('PandasNet')

from PandasNet import Codecs
Codecs.Initialize(False)