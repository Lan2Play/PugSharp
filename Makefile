######## You can find a detailed documentation on https://pugsharp.lan2play.de/develop/quickstart.html#run-pugsharp-locally

## Silent functions
.SILENT: init-env

## Variables
currentDir = $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))
userId = $(shell id -u)
groupId = $(shell id -g)

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
init-all: prepare-folders init-env copy-counterstrikesharp install-metamod start-csserver
clean-all: clean-csserver clean-env clean-build



## preperation commands
prepare-folders:
	mkdir -p $(currentDir)/cs2 && chmod 777 $(currentDir)/cs2

init-env:
	export STEAMUSER=$(STEAMUSER);\
	export STEAMPASS=$(STEAMPASS);\
	export STEAMGUARD=$(STEAMGUARD);\
	export SKIPSTEAMGUARD=$(SKIPSTEAMGUARD);\

	if [ -z "$$STEAMUSER" ]; then \
		read -r -p "You dont't set the steamuser, you can enter it now (later change it in .env file) " STEAMUSER; \
	fi; \
	echo "using $$STEAMUSER"; \
	if [ -z "$$STEAMPASS" ]; then \
		read -r -p "You dont't set the steampass, you can enter it now (later change it in .env file) " STEAMPASS; \
	fi; \
	echo "using $$STEAMPASS"; \
	if [ -z "$$SKIPSTEAMGUARD" ]; then \
		if [ -z "$$STEAMGUARD" ]; then \
			read -r -p "You dont't set the steamguard, you can enter it now (later change it in .env file) " STEAMGUARD; \
		fi; \
	fi; \
	echo "using $$STEAMGUARD"; \
	cp $(currentDir)/.env.example $(currentDir)/.env ; \
	sed -i "s#STEAMUSER=#STEAMUSER=$$STEAMUSER#g" $(currentDir)/.env ; \
	sed -i "s#STEAMPASS=#STEAMPASS=$$STEAMPASS#g" $(currentDir)/.env ; \
	sed -i "s#STEAMGUARD=#STEAMGUARD=$$STEAMGUARD#g" $(currentDir)/.env; 

copy-counterstrikesharp:
	mkdir -p $(currentDir)/cs2/game/csgo/addons/
	cp -rf $(currentDir)/PugSharp/counterstrikesharp $(currentDir)/cs2/game/csgo/addons/
	cp -rf $(currentDir)/PugSharp/metamod $(currentDir)/cs2/game/csgo/addons/

#TODO!!!
fix-metamod:
	sed -i '/^			Game	csgo$/i			Game	csgo/addons/metamod' /home/volza/temp/cs2/cs2-data/game/csgo/gameinfo.gi

install-metamod:
	mkdir -p $(currentDir)/cs2/game/csgo/
	export LATESTMM=$(shell wget -qO- https://mms.alliedmods.net/mmsdrop/2.0/mmsource-latest-linux); \
	wget -qO- https://mms.alliedmods.net/mmsdrop/2.0/$$LATESTMM | tar xvzf - -C $(currentDir)/cs2/game/csgo



## base commands

start-csserver:
	$(DOCKER_COMPOSE) up 

stop-csserver:
	$(DOCKER_COMPOSE) down

build-debug:
	dotnet publish -c debug

build-release:
	dotnet publish -c release

copy-pugsharp:
	rm -rf $(currentDir)/PugSharp/bin/Debug/net7.0/publish/CounterStrikeSharp.API.dll
	mkdir -p $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/plugins/PugSharp
	cp -rf $(currentDir)/PugSharp/bin/Debug/net7.0/publish/* $(currentDir)/cs2/game/csgo/addons/counterstrikesharp/plugins/PugSharp




## Documentation Commands
docs-html:
ifeq ($(OS),Windows_NT)
	echo you currently need docker on linux to build the documentation
else
	docker run --rm -v $(currentDir)/Docs:/docs -e USERID=$(shell id -u ${USER}) -e GROUPID=$(shell id -g ${USER}) lan2play/docker-sphinxbuild:latest
endif




## cleaning commands

clean-csserver:
	$(DOCKER_COMPOSE) down
	rm -rf $(currentDir)/cs2
	$(DOCKER_COMPOSE) rm

clean-env:
	rm -rf $(currentDir)/.env

clean-build:
	find $(currentDir) -wholename '*PugSharp*/bin' | xargs rm -rf
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
