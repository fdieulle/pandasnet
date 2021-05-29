from setuptools import setup, Command
import distutils

import sys, os

PY_MAJOR = sys.version_info[0]
PY_MINOR = sys.version_info[1]

CONFIGURED_PROPS = "configured.props"

def readme():
   with open('README.md', 'r', encoding='utf-8') as f:
       return f.read()


def _write_configure_props():
    defines = [
        f'PYTHON{PY_MAJOR}{PY_MINOR}'
    ]

    if sys.platform == 'win32':
        defines.append('WINDOWS')

    if hasattr(sys, "abiflags"):
        if "d" in sys.abiflags:
            defines.append("PYTHON_WITH_PYDEBUG")
        if "m" in sys.abiflags:
            defines.append("PYTHON_WITH_PYMALLOC")

    import xml.etree.ElementTree as ET

    proj = ET.Element("Project")
    props = ET.SubElement(proj, "PropertyGroup")
    c = ET.SubElement(props, "ConfiguredConstants")
    c.text = " ".join(defines)

    ET.ElementTree(proj).write(CONFIGURED_PROPS)


def _ensure_reference(project_file):
    import xml.etree.ElementTree as ET
    from distutils.sysconfig import get_python_lib

    root = ET.parse(project_file)

    for node in root.findall('//Reference[@Include="Python.Runtime"]/HintPath'):
        reference = os.path.join(os.path.dirname(project_file), node.text)
        if not os.path.exists(reference):
            node.text = os.path.join(get_python_lib(), 'pythonnet', 'runtime', 'Python.Runtime.dll')
    
    root.write(project_file)


class configure(Command):
    """Configure command"""

    description = "Configure the pythonnet build"
    user_options = []

    def initialize_options(self):
        pass

    def finalize_options(self):
        pass

    def run(self):
        self.announce("Writing configured.props...", level=distutils.log.INFO)
        _write_configure_props()


class DotnetLib:
    def __init__(self, name, path, **kwargs):
        self.name = name
        self.path = path
        self.args = kwargs


class build_dotnet(Command):
    """Build command for dotnet-cli based builds"""

    description = "Build DLLs with dotnet-cli"
    user_options = [
        ("dotnet-config", None, "dotnet build configuration"),
        (
            "inplace",
            "i",
            "ignore build-lib and put compiled extensions into the source "
            + "directory alongside your pure Python modules",
        ),
    ]

    def initialize_options(self):
        self.dotnet_config = None
        self.build_lib = None
        self.inplace = False

    def finalize_options(self):
        if self.dotnet_config is None:
            self.dotnet_config = "release"
        # self.dotnet_config = "debug"

        build = self.distribution.get_command_obj("build")
        build.ensure_finalized()
        if self.inplace:
            self.build_lib = "."
        else:
            self.build_lib = build.build_lib

    def run(self):
        dotnet_modules = self.distribution.dotnet_libs
        self.run_command("configure")

        for lib in dotnet_modules:
            output = os.path.join(
                os.path.abspath(self.build_lib), 
                lib.args.pop("output")
            )
            exclude = lib.args.pop("exclude", None)

            opts = sum([
                ["--" + name.replace("_", "-"), value] for name, value in lib.args.items()],
                [],
            )

            _ensure_reference(lib.path)

            opts.extend(["--configuration", self.dotnet_config])
            opts.extend(["--output", output])

            self.announce("Running dotnet build...", level=distutils.log.INFO)
            self.spawn(["dotnet", "build", lib.path] + opts)

            if exclude is not None:
                files = [os.path.join(output, f) for f in os.listdir(output) if exclude in f]
                [os.remove(f) for f in files if os.path.isfile(f)]


# Add build_dotnet to the build tasks:
from distutils.command.build import build as _build
from setuptools.command.develop import develop as _develop
from setuptools import Distribution
import setuptools

class build(_build):
    sub_commands = _build.sub_commands + [("build_dotnet", None)]


class develop(_develop):
    def install_for_development(self):
        # Build extensions in-place
        self.reinitialize_command("build_dotnet", inplace=1)
        self.run_command("build_dotnet")

        return super().install_for_development()


# Monkey-patch Distribution s.t. it supports the dotnet_libs attribute
Distribution.dotnet_libs = None

cmdclass = {
    "build": build,
    "build_dotnet": build_dotnet,
    "configure": configure,
    "develop": develop,
}

dotnet_libs = [
    DotnetLib(
        "pandas-converters",
        "dotnet/PandasNet/PandasNet.csproj",
        output="pandasnet/libs",
        exclude='Python.Runtime.'
    ),
    DotnetLib(
        "pandas-converters-tests",
        "dotnet/LibForTests/LibForTests.csproj",
        output="tests/libs",
    ),
]

setup(
    cmdclass=cmdclass,
    name='pandasnet',
    author='Fabien Dieulle',
    author_email='fabiendieulle@hotmail.fr',
    description='Extensions of pythonnet package to support pandas DataFrame conversions',
    long_description=readme(),
    long_description_content_type='text/markdown',
    url='https://github.com/fdieulle/pandasnet',
    packages=['pandasnet'],
    package_data={'pandasnet': ['libs/PandasNet.*']},
    install_requires=[
        'pycparser', 
        'pythonnet', 
        'pandas'],
    dotnet_libs=dotnet_libs,
    classifiers=[
       "License :: OSI Approved :: MIT License",
        "Programming Language :: C#",
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.6",
        "Programming Language :: Python :: 3.7",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "Operating System :: Microsoft :: Windows",
        "Operating System :: POSIX :: Linux",
        "Operating System :: MacOS :: MacOS X",
    ],
    use_scm_version=True,
    setup_requires=['setuptools_scm'],
    zip_safe=False
)