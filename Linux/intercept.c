#define _GNU_SOURCE

#include <dlfcn.h>
#include <stdlib.h>

void mono_set_dirs(char *assembly_dir,char *config_dir) {
    setenv("MONO_PATH", "/usr/lib/mono/4.5", 1);

    void (*set_dir_ptr)(char*,char*) = dlsym(RTLD_NEXT, "mono_set_dirs");
    (*set_dir_ptr)("/usr/lib/mono/4.5", config_dir);
}
