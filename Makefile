######## You can find a getting started documentation on https://pugsharp.lan2play.de/develop/quickstart.html#run-develop-pugsharp-locally-with-the-make-file
######## and a detailed documentation on https://pugsharp.lan2play.de/develop/makefile.html

## Silent functions
.SILENT: init-env fix-metamod



## Variables
currentDir = $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))
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
init-all: prepare-folders init-env install-deps pull-csserver start-csserver attach-csserver
init-all-docker: prepare-folders init-env install-deps-docker pull-csserver start-csserver attach-csserver
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
	wget -q -O $(currentDir)/counterstrikesharp.zip $(shell curl -s -L -H "Accept: application/vnd.github+json" https://api.github.com/repos/roflmuffin/CounterStrikeSharp/releases/tags/$(shell dotnet list PugSharp/PugSharp.csproj package --format json | jq -r '.projects[].frameworks[].topLevelPackages[] | select(.id == "CounterStrikeSharp.API") | .resolvedVersion' | sed 's|1.0.|v|g') | jq -r '.assets[] | select(.browser_download_url | test("with-runtime")) | .browser_download_url') 
	unzip -o $(currentDir)/counterstrikesharp.zip -d $(currentDir)/cs2/game/csgo
	rm -rf $(currentDir)/counterstrikesharp.zip

install-counterstrikesharp-docker:
	docker run --rm --interactive \
	-v $(currentDir):/app \
	mcr.microsoft.com/dotnet/sdk:7.0 /bin/sh -c " \
	apt-get update && apt-get install jq unzip -y; \
	mkdir -p /app/cs2/game/csgo/addons/; \
	wget -q -O /app/counterstrikesharp.zip $(shell curl -s -L -H "Accept: application/vnd.github+json" https://api.github.com/repos/roflmuffin/CounterStrikeSharp/releases/tags/$(shell dotnet list PugSharp/PugSharp.csproj package --format json | jq -r '.projects[].frameworks[].topLevelPackages[] | select(.id == "CounterStrikeSharp.API") | .resolvedVersion' | sed 's|1.0.|v|g') | jq -r '.assets.[] | select(.browser_download_url | test("with-runtime")) | .browser_download_url'); \
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
	rm -rf $(currentDir)/PugSharp/bin/Debug/net7.0/publish/CounterStrikeSharp.API.dll
	mkdir -p $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/plugins/PugSharp
	cp -rf $(currentDir)/PugSharp/bin/Debug/net7.0/publish/* $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/plugins/PugSharp

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
