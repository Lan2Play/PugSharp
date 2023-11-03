Makefile
==================================================

Introduction
----------------------------------------

`GNU Make <https://www.gnu.org/software/make>`_ is a tool that automates several (build)steps for you.
You can find the ``Makefile`` in the root of the Repository where all available commands are defined.
On this page every single make command will be documented with its purpose, so you can use them as intended.

A few of the Commands have optional parameters that can be set via either a prompt after calling the commands or with writing them after the make command.
So for example when you have the available parameter ``STEAMUSER`` on the make command ``init-all``, that you want to set to ``username`` your finalized Make command would look like:

.. code-block:: bash
    
    make init-all STEAMUSER="username"

There are 2 types of commands, the group commands and the other commands. Group commands are essentially calling multiple other make Commands here.

Commands
----------------------------------------

build-and-copy
........................
Publishes a debug build of Pugsharp with the locally installed .Net SDK and copies it in to the local cs2 server

**Group Command**

- :ref:`develop/makefile:build-debug` 
- :ref:`develop/makefile:copy-pugsharp` 

**Parameters:**

No Parameters

build-and-copy-docker
........................
Publishes a debug build of Pugsharp with .Net SDK inside of docker and copies it in to the local cs2 server

**Group Command**

- :ref:`develop/makefile:build-debug-docker` 
- :ref:`develop/makefile:copy-pugsharp` 

**Parameters:**

No Parameters

init-all
........................
Prepares the gameserver folder, initializes the .env file for the cs2 server, copies CounterStrikeSharp into the server, installs metamod and starts the server.

**Group Command**

- :ref:`develop/makefile:prepare-folders` 
- :ref:`develop/makefile:init-env` 
- :ref:`develop/makefile:copy-counterstrikesharp` 
- :ref:`develop/makefile:install-metamod` 
- :ref:`develop/makefile:start-csserver` 

**Parameters:**

- ``STEAMUSER`` - Steam username for CS2 server download
- ``STEAMPASS`` - Steam password for CS2 server download
- ``STEAMGUARD`` - Steamguard key for the above user if required
- ``SKIPSTEAMGUARD`` - set to anything to disable the prompt for ``STEAMGUARD`` if it should be empty

clean-all
........................
Cleans the CS2 server including all plugins, the build folders and the local .env file

**Group Command**

- :ref:`develop/makefile:clean-csserver` 
- :ref:`develop/makefile:clean-env` 
- :ref:`develop/makefile:clean-build` 

**Parameters:**

No Parameters


prepare-folders
........................
creates the cs2 folder inside of the repo and makes shure it is writable by everyone

**Parameters:**

No Parameters

init-env
........................
copies the .env.example to .env and replaces the parameters in that file.

**Parameters:**

- ``STEAMUSER`` - Steam username for CS2 server download
- ``STEAMPASS`` - Steam password for CS2 server download
- ``STEAMGUARD`` - Steamguard key for the above user if required
- ``SKIPSTEAMGUARD`` - set to anything to disable the prompt for ``STEAMGUARD`` if it should be empty

copy-counterstrikesharp
........................
copies the CounterStrikeSharp version that is included in our repo to the cs2 server

**Parameters:**

No Parameters

install-metamod
........................
downloads and installs the latest metamod 2.0 dev release into the cs2 server

**Parameters:**

No Parameters


fix-metamod
........................
inserts the ``Game	csgo/addons/metamod`` command into ./cs2/game/csgo/gameinfo.gi

**Parameters:**

No Parameters

start-csserver
........................
starts the local cs2 server

**Parameters:**

No Parameters

stop-csserver
........................
stops the local cs2 server

**Parameters:**

No Parameters

build-debug
........................
Publishes a debug build of Pugsharp with the locally installed .Net SDK 

**Parameters:**

No Parameters

build-release
........................
Publishes a release build of Pugsharp with the locally installed .Net SDK 

**Parameters:**

No Parameters

build-debug-docker
........................
Publishes a debug build of Pugsharp with .Net SDK inside of docker

**Parameters:**

No Parameters

build-release-docker
........................
Publishes a debug build of Pugsharp with .Net SDK inside of docker

**Parameters:**

No Parameters

copy-pugsharp
........................
copies pugsharp debug build in to the local cs2 server

**Parameters:**

No Parameters


docs-html
........................
builds the docs ( see :ref:`contribution:documentation` )

**Parameters:**

No Parameters

clean-csserver
........................
stops and removes the local cs2 server with its data

**Parameters:**

No Parameters

clean-env
........................
removes the .env file

**Parameters:**

No Parameters

clean-build
........................
removes the ``bin`` and ``obj`` folders, as well as the ``Docs/build`` folder.

**Parameters:**

No Parameters

clean-pugsharp
........................
removes pugsharp from the local cs2 server

**Parameters:**

No Parameters

clean-counterstrikesharp
........................
removes counterstrikesharp from the local cs2 server

**Parameters:**

No Parameters

clean-metamod
........................
removes metamod from the local cs2 server

**Parameters:**

No Parameters