#!/bin/bash
exec mono --debug tools/BindingGenerator/bin/Debug/generator2.exe -w --fixup=metadata -l=debug --dump-hierarchy api.xml
