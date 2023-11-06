######## You can find a getting started documentation on https://pugsharp.lan2play.de/develop/quickstart.html#run-develop-pugsharp-locally-with-the-make-file
######## and a detailed documentation on https://pugsharp.lan2play.de/develop/makefile.html

## Silent functions
.SILENT: init-env

## Variables
currentDir = $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))
userId = $(shell id -u)
groupId = $(shell id -g)
user = $(userId):$(groupId)
dockeruser = --user $(user)

dotnet_runtime_url = "https://download.visualstudio.microsoft.com/download/pr/dc2c0a53-85a8-4fda-a283-fa28adb5fbe2/8ccade5bc400a5bb40cd9240f003b45c/aspnetcore-runtime-7.0.11-linux-x64.tar.gz"
dotnet_runtime_version = "7.0.11"

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
build-and-copy: build-debug copy-pugsharp
build-and-copy-docker: build-debug-docker copy-pugsharp
init-all: prepare-folders init-env copy-counterstrikesharp install-netruntime install-metamod start-csserver attach-csserver
clean-all: clean-csserver clean-env clean-build
start-attach: start-csserver attach-csserver



## preperation commands
prepare-folders:
	mkdir -p $(currentDir)/cs2 && chmod 777 $(currentDir)/cs2

init-env:
	cp $(currentDir)/.env.example $(currentDir)/.env ; 

copy-counterstrikesharp:
	mkdir -p $(currentDir)/cs2/game/csgo/addons/
	cp -rf $(currentDir)/PugSharp/counterstrikesharp $(currentDir)/cs2/game/csgo/addons/
	cp -rf $(currentDir)/PugSharp/metamod $(currentDir)/cs2/game/csgo/addons/

install-metamod:
	mkdir -p $(currentDir)/cs2/game/csgo/
	export LATESTMM=$(shell wget -qO- https://mms.alliedmods.net/mmsdrop/2.0/mmsource-latest-linux); \
	wget -qO- https://mms.alliedmods.net/mmsdrop/2.0/$$LATESTMM | tar xvzf - -C $(currentDir)/cs2/game/csgo

install-netruntime:
	mkdir -p $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/dotnet
	curl -s -L $(dotnet_runtime_url) | tar xvz -C $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/dotnet
	mv $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/$(dotnet_runtime_version)/* $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/

#TODO!!!
fix-metamod:
	sed -i '/^			Game	csgo$/i			Game	csgo/addons/metamod' /home/volza/temp/cs2/cs2-data/game/csgo/gameinfo.gi



## base commands

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
	$(user) mcr.microsoft.com/dotnet/sdk:7.0 /bin/sh -c " \
	cd /app && dotnet publish -c release; chown -R $(user) /app"

copy-pugsharp:
	rm -rf $(currentDir)/PugSharp/bin/Debug/net7.0/publish/CounterStrikeSharp.API.dll
	mkdir -p $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/plugins/PugSharp
	cp -rf $(currentDir)/PugSharp/bin/Debug/net7.0/publish/* $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/plugins/PugSharp




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
