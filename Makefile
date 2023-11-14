######## You can find a getting started documentation on https://pugsharp.lan2play.de/develop/quickstart.html#run-develop-pugsharp-locally-with-the-make-file
######## and a detailed documentation on https://pugsharp.lan2play.de/develop/makefile.html

## Silent functions
.SILENT: init-env fix-metamod


## Variables
currentDir = $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))
currentDirWin = $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))
userId = $(shell id -u)
groupId = $(shell id -g)
user = $(userId):$(groupId)
dockeruser = --user $(user)

## Docker Compose detection
ifeq ($(OS),Windows_NT)
  DOCKER_COMPOSE=docker compose
else
ifneq ($(shell docker compose version 2>/dev/null),)
  DOCKER_COMPOSE=docker compose
else
  DOCKER_COMPOSE=docker-compose
endif
endif

## group commands
build-and-copy: git-pull build-debug copy-pugsharp
build-and-copy-docker: git-pull build-debug-docker copy-pugsharp
init-all: prepare-folders init-env install-deps copy-pugsharp-sample-configs pull-csserver start-csserver attach-csserver
init-all-docker: prepare-folders init-env install-deps-docker copy-pugsharp-sample-configs pull-csserver start-csserver attach-csserver
install-all-windows: install-windows-steamcmd install-windows
install-deps: install-counterstrikesharp install-metamod
install-deps-docker: install-counterstrikesharp-docker install-metamod
clean-all: clean-csserver clean-env clean-build
start-attach: start-csserver attach-csserver



## preperation commands
prepare-folders:
	mkdir -p $(currentDir)/cs2 && chmod 777 $(currentDir)/cs2

init-env:
	cp $(currentDir)/.env.example $(currentDir)/.env ;

install-counterstrikesharp:
	mkdir -p $(currentDir)/cs2/game/csgo/addons/
	wget -q -O $(currentDir)/counterstrikesharp.zip $(shell curl -s -L -H "Accept: application/vnd.github+json" https://api.github.com/repos/roflmuffin/CounterStrikeSharp/releases/tags/$(shell dotnet list PugSharp/PugSharp.csproj package --format json | jq -r '.projects[].frameworks[].topLevelPackages[] | select(.id == "CounterStrikeSharp.API") | .resolvedVersion' | sed 's|1.0.|v|g') | jq -r '.assets[] | select(.browser_download_url | test("with-runtime.*linux")) | .browser_download_url')
	unzip -o $(currentDir)/counterstrikesharp.zip -d $(currentDir)/cs2/game/csgo
	rm -rf $(currentDir)/counterstrikesharp.zip

install-counterstrikesharp-docker:
	docker run --rm --interactive \
	-v $(currentDir):/app \
	mcr.microsoft.com/dotnet/sdk:7.0 /bin/sh -c " \
	apt-get update && apt-get install jq unzip -y; \
	mkdir -p /app/cs2/game/csgo/addons/; \
	wget -q -O /app/counterstrikesharp.zip $(shell curl -s -L -H "Accept: application/vnd.github+json" https://api.github.com/repos/roflmuffin/CounterStrikeSharp/releases/tags/$(shell dotnet list PugSharp/PugSharp.csproj package --format json | jq -r '.projects[].frameworks[].topLevelPackages[] | select(.id == "CounterStrikeSharp.API") | .resolvedVersion' | sed 's|1.0.|v|g') | jq -r '.assets.[] | select(.browser_download_url | test("with-runtime.*linux")) | .browser_download_url'); \
	unzip -o /app/counterstrikesharp.zip -d /app/cs2/game/csgo; \
	rm -rf /app/counterstrikesharp.zip; \
	chown -R $(user) /app/cs2/game/csgo/addons;"

install-metamod:
	mkdir -p $(currentDir)/cs2/game/csgo/
	export LATESTMM=$(shell wget -qO- https://mms.alliedmods.net/mmsdrop/2.0/mmsource-latest-linux); \
	wget -qO- https://mms.alliedmods.net/mmsdrop/2.0/$$LATESTMM | tar xvzf - -C $(currentDir)/cs2/game/csgo

fix-metamod:
	./resources/acmrs.sh

install-jq-and-unzip:
	apt-get update && apt-get install jq unzip -y	

install-vsdebug:
	$(DOCKER_COMPOSE) exec -u 0 cs2-server /bin/bash -c "apt-get update ; apt-get install procps -y ; mkdir -p /root/.vs-debugger; curl -sSL https://aka.ms/getvsdbgsh -o '/root/.vs-debugger/GetVsDbg.sh' && chmod +x /root/.vs-debugger/GetVsDbg.sh && /root/.vs-debugger/GetVsDbg.sh -v latest -l /vsdbg"

# install-windows:
# 	powershell Start-Process -NoNewWindow -WorkingDirectory ${CURDIR} -FilePath "$$env:LOCALAPPDATA\Microsoft\WinGet\Links\steamcmd" -ArgumentList '+force_install_dir ${CURDIR}\cs2\ +login Anonymous +app_update 730 validate +exit';

# install-windows-steamcmd:
# 	winget install --id Valve.SteamCMD --exact --accept-source-agreements --disable-interactivity --accept-source-agreements --force

# install-metamod-windows:
# 	mkdir -p ${CURDIR}/cs2/game/csgo/
# 	export LATESTMM=$(shell wget -qO- https://mms.alliedmods.net/mmsdrop/2.0/mmsource-latest-windows); \
# 	powershell Start-Process wget -Argume -qO- https://mms.alliedmods.net/mmsdrop/2.0/$$LATESTMM | tar xvzf - -C ${CURDIR}/cs2/game/csgo


## base commands
pull-csserver:
	docker pull joedwards32/cs2

start-csserver:
	$(DOCKER_COMPOSE) up -d

attach-csserver:
	docker attach pugsharp-cs2-server-1

stop-csserver:
	$(DOCKER_COMPOSE) down

build-debug:
	dotnet publish -c debug

build-release:
	dotnet publish -c release

build-debug-docker:
	docker run --rm --interactive \
	-v $(currentDir):/app \
	mcr.microsoft.com/dotnet/sdk:7.0 /bin/sh -c " \
	cd /app && dotnet publish -c debug; chown -R $(user) /app"

build-release-docker:
	docker run --rm --interactive \
	-v $(currentDir):/app \
	mcr.microsoft.com/dotnet/sdk:7.0 /bin/sh -c " \
	cd /app && dotnet publish -c release; chown -R $(user) /app"

copy-pugsharp:
	mkdir -p $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/plugins/PugSharp
	cp -rf $(currentDir)/PugSharp/bin/Debug/net7.0/publish/* $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/plugins/PugSharp

copy-pugsharp-sample-configs:
	mkdir -p $(currentDir)/cs2/game/csgo/cfg
	cp -rf $(currentDir)/resources/cfg/* $(currentDir)/cs2/game/csgo/cfg/

git-pull:
	git pull || true



## Documentation Commands
docs-html:
ifeq ($(OS),Windows_NT)
	echo you currently need docker on linux to build the documentation
else
	docker run --rm -v $(currentDir)/Docs:/docs -e USERID=$(userId) -e GROUPID=$(groupId) lan2play/docker-sphinxbuild:latest
endif




## cleaning commands

clean-csserver:
	$(DOCKER_COMPOSE) down
	rm -rf $(currentDir)/cs2
	$(DOCKER_COMPOSE) rm

clean-env:
	rm -rf $(currentDir)/.env

clean-build:
	find $(currentDir) -wholename '*PugSharp*/bin' -not -path "*PugSharp/counterstrikesharp/bin" | xargs rm -rf
	find $(currentDir) -wholename '*PugSharp*/obj' | xargs rm -rf
	rm -rf Docs/build

clean-pugsharp:
	rm -rf $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/plugins/PugSharp

clean-counterstrikesharp:
	rm -rf $(currentDir)/cs2/game/csgo/addons/counterstrikesharp
	rm -rf $(currentDir)/cs2/game/csgo/addons/metamod/counterstrikesharp.vdf

clean-metamod:
	rm -rf $(currentDir)/cs2/game/csgo/addons/metamod.vdf
	rm -rf $(currentDir)/cs2/game/csgo/addons/metamod_x64.vdf
	rm -rf $(currentDir)/cs2/game/csgo/addons/metamod
