currentDir = $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))
userId = $(shell id -u)
groupId = $(shell id -g)


build-and-copy: build copy



build:
	dotnet publish -c debug

copy:
	rm PugSharp/bin/Debug/net7.0/publish/CounterStrikeSharp.API.dll
	cp -rf PugSharp/bin/Debug/net7.0/publish/* /home/volza/temp/cs2/cs2-data/game/csgo/addons/counterstrikesharp/plugins/PugSharp

start-ds:
	cd /home/volza/temp/cs2
	docker run --rm -it --net=host -v /home/volza/temp/cs2/cs2-data:/home/steam/cs2-dedicated/ --name=cs2-dedicated -e STEAMUSER=cs2_lan2play -e STEAMPASS=vZ23qYxdyFLKZyI1rnbrFHEmGfET -e CS2_BOT_QUOTA=0 lan2play/cs2

fix-mm:
	sed -i '/^			Game	csgo$/i			Game	csgo/addons/metamod' /home/volza/temp/cs2/cs2-data/game/csgo/gameinfo.gi

# Make Documentation
docs-html:
ifeq ($(OS),Windows_NT)
	echo you currently need docker on linux to build the documentation
else
	docker run --rm -v $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))/Docs:/docs -e USERID=$(shell id -u ${USER}) -e GROUPID=$(shell id -g ${USER}) lan2play/docker-sphinxbuild:latest
endif