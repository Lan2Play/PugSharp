
Contribution Guide
==================================================

We are looking forward to every conbtirbution you possibly can bring in. If you want to contribute please read this document carefully.

If you encounter serious errors while using PugSharp and you are not able to fix them, feel free to open issues on https://github.com/Lan2Play/PugSharp/issues .

If you plan to add a feature to PugSharp, please open an issue as well and draft a pull request as soon as you have done something, so no one has to do work that someone already did.

You can also join our discord to get in contact with us and the other contributors:

.. image:: https://discordapp.com/api/guilds/748086853449810013/widget.png?style=banner3
   :target: https://discord.gg/zF5C9WPWFq


Documentation
--------------
This documentation is written in restructured text and its build using sphinx and the read the docs theme. The source can be found in our main repository in the ``Docs/`` subfolder (https://github.com/Lan2Play/PugSharp/tree/main/Docs).
Feel free to pull request corrections or expansions of the documentation at any time! 

To build the documentation locally to the ``Docs/build`` subfolder you have two options:

- Building with docker and the make file (recommended)
- Building manually with the sphinx make file

Building with docker and the make file
.......................................

Docker Windows
'''''''''''''''''''''
Prerequisites: 

- Docker for Windows with wsl2 backend (https://docs.docker.com/docker-for-windows/wsl/ Follow the prerequisites, the download and the install part!)


 .. warning::

        If you are using git, consider cloning the repository within your wsl distro instead of with git for windows to get around line ending problems!

To build the documentation just enter yor wsl2 distribution and follow the linux part below!


Docker Linux
'''''''''''''''''''''
Prerequisites: 

- Docker (https://docs.docker.com/engine/install)
- Make (should be available for nearly every linux distro in the corresponding packagemanager)

In order to build the documentation you have to change to the root folder of the repository and run

.. code-block:: bash

   make docs-html

Building manually with the sphinx make file
............................................

Manual Windows
'''''''''''''''''''''
Prerequisites: 

- python 3 (https://docs.python.org/3/using/index.html) with pip
- sphinx (https://www.sphinx-doc.org/en/master/usage/installation.html) 
- the read the docs theme (https://github.com/readthedocs/sphinx_rtd_theme#installation)

open a cmd or powershell and change your folder to the ``Docs/`` subfolder and run

CMD

.. code-block:: batch

   make.bat html

Psh

.. code-block:: powershell

   ./make.bat html


Manual Linux
'''''''''''''''''''''
Prerequisites: 

- python 3 (https://docs.python.org/3/using/index.html) with pip
- sphinx (https://www.sphinx-doc.org/en/master/usage/installation.html) 
- the read the docs theme (https://github.com/readthedocs/sphinx_rtd_theme#installation)
- Make (should be available for nearly every linux distro in the corresponding packagemanager)

open your favorite shell and change your folder to the ``Docs/`` subfolder and run

.. code-block:: bash

   make html


Translation / i18n
-------------------
If you want to help with the translation of PugSharp, please check out our :doc:`/develop/translation` section


Code
-----
If you want to get into coding for PugSharp, check out the :doc:`/develop/quickstart`, there you can find an introduction into how to setup your development environment, how the contribution process looks like and some specific parts of PugSharp where we would love to see adaption for more usecases.
