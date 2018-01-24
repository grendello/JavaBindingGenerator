#!/bin/bash
exec mono --debug tools/BindingGenerator/bin/Debug/generator2.exe -w --fixup=metadata --fixup=enummetadata -l=debug --dump-fixedup --dump-hierarchy api.xml
