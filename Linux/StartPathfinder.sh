cd "$(dirname "$0")"

if [ "$(uname -m)" = "x86_64" ]; then
    LD_PRELOAD="$(pwd)/intercept.so /usr/lib/libmono-2.0.so" MONO_DEBUG=explicit-null-checks ./HacknetPathfinder.bin.x86_64 "$@"
else
    LD_PRELOAD="$(pwd)/intercept.so /usr/lib/libmono-2.0.so" MONO_DEBUG=explicit-null-checks ./HacknetPathfinder.bin.x86 "$@"
fi
