#!/bin/bash
exec mono --debug BindingGenerator/bin/Debug/generator2.exe -w --fixup=metadata -l=debug --dump-hierarchy api.xml
