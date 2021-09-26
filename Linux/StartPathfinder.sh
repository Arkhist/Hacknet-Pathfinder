TERM=xterm LD_PRELOAD="$(pwd)/lib64/libcef.so  $(pwd)/intercept.so /usr/lib/libmono-2.0.so.1" MONO_DEBUG=explicit-null-checks ./HacknetPathfinder.bin.x86_64
