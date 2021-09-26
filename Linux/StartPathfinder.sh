cd "`dirname "$0"`"

if [ "$(uname -m)" == "x86_64" ]; then
    TERM=xterm LD_PRELOAD="$(pwd)/lib64/libcef.so  $(pwd)/intercept.so /usr/lib/libmono-2.0.so" MONO_DEBUG=explicit-null-checks ./HacknetPathfinder.bin.x86_64
else
    TERM=xterm LD_PRELOAD="$(pwd)/lib/libcef.so  $(pwd)/intercept.so /usr/lib/libmono-2.0.so" MONO_DEBUG=explicit-null-checks ./HacknetPathfinder.bin.x86
fi
