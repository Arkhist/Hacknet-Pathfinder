ifeq ($(OS),Windows_NT)
   NFVERSION=v4.0.30319## Net Framework version
   NFDIR = $(WINDIR)\Microsoft.NET\Framework\
   NFBUILD = $(NFDIR)\msBuild.exe
   NFOPT = /p:Configuration=Release
   PROGRAMFILES := $(if ${ProgramFiles(x86)},${ProgramFiles(x86)},${ProgramFiles})
   HACKNETDIR=$(PROGRAMFILES)\Steam\steamapps\common\Hacknet## Hacknet installation dir
else
   NFBUILD = xbuild
   NFOPT = /p:Configuration=Release /p:TargetFrameworkVersion="v4.5"
   HACKNETDIR=/home/$(USER)/.steam/steam/steamapps/common/Hacknet
endif

.ONESHELL:
.SILENT:
.PHONY: help clean

build: lib/HacknetPathfinder.exe ## make all targets

clean: ## clean all targets
	rm lib/HacknetPathfinder.exe
	rm lib/PathfinderPatcher.exe
	rm bin/Release/Pathfinder.dll

help: ## this help information
	@echo
	echo $(MAKE) [targets ...] [parameters=value ...]
	echo
	echo targets:
	grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST)\
	| awk 'BEGIN {FS = ":.*?## "}; {printf "  %-16s %s\n", $$1, $$2}'
	echo
	echo parameters:
	grep -E '^ *[a-zA-Z_-]+\=.*?## .*$$' $(MAKEFILE_LIST)\
	| awk 'BEGIN {FS = "=.*?## "}; {gsub(/^[ \t]+/, "", $$1);gsub(/^[ \t]+/, "", $$2); printf "  %-16s %s\n", $$1, $$2}'
	echo

lib/HacknetPathfinder.exe: lib/PathfinderPatcher.exe bin/Release/Pathfinder.dll $(HACKNETDIR)/Hacknet.exe
	cd lib
	PathfinderPatcher.exe -pathfinderDir "../bin/Release" -exeDir "$(HACKNETDIR)"
	cd ..

bin/Release/Pathfinder.dll: lib/PathfinderPatcher.exe $(HACKNETDIR)/Hacknet.exe
	cd lib
	PathfinderPatcher.exe -exeDir "$(HACKNETDIR)" -spit
	$(NFBUILD) ../Pathfinder.csproj $(NFOPT)
	cd ..

lib/PathfinderPatcher.exe:
	cd lib
	$(NFBUILD) ../PathfinderPatcher/PathfinderPatcher.csproj $(NFOPT)
	cd ..

$(HACKNETDIR)/Hacknet.exe: #~
	$(MAKE) help
	echo 'hint:'
	echo '  add HACKNETDIR="<Direcory where Hacknet is installed>" in the $(MAKE) command.'
	echo
	echo 'FATAL:  $(HACKNETDIR)/Hacknet.exe not found'
	echo
	exit 127
