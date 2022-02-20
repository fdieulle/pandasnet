import os
import clr
import sys

lib_folder = os.path.join(os.path.dirname(__file__), 'libs')
if lib_folder not in sys.path:
    sys.path.append(lib_folder)

clr.AddReference('LibForTests')