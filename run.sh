#!/bin/bash
rm -f run.log
exec mono --debug tools/BindingGenerator/bin/Debug/generator2.exe -w --fixup=metadata --fixup=enummetadata -l=verbose --dump-fixedup --dump-hierarchy api.xml | tee -a run.log
