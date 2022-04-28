#!/bin/bash

output=`echo "$1" | sed 's/\(.*\)\..../\1.png/'`
transparent=${2:-#000000}
c1=${3:-#FF0000}
c2=${4:-#00FF00}
c3=${5:-#0000FF}
xxd -b -g0 -c3 "$1" | cut -d' ' -f2 | sed 's/\(..\)/\1  /g' | sed 's/00 /aa/g'| sed 's/01 /bb/g'| sed 's/10 /cc/g'| sed 's/11 /dd/g' |sed 's/ //g' | sed 's/\(.*\)/"\1",/g' | printf "/* XPM */\nstatic char * XFACE[] = {\n\"24 21 4 1\",\n\"a c $transparent\",\n\"b c $c1\",\n\"c c $c2\",\n\"d c $c3\",\n%s\n}" "$(cat -)"| convert xpm:- -transparent $transparent "$output"
