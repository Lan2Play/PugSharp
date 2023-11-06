
Developers start guide
==================================================

Introduction
----------------------------------------

We are glad you want to help us on PugSharp!
Some things you should think of before you start implementing new features or fixing bugs:

- Can another feature thats already implemented be expanded? yes? then go for that instead of Building complete new stuff!
- Does the addition / change might affect other usecases than your own? Build your changes with legacy support in mind!
- Try to follow the coding Style which is used within PugSharp, just look around in our code to see which case is handled mostly in which manner
- Why i shouldn't join the PugSharp discord developer channel for discussion?

To start a new code contribution please:

- open an issue to announce that you are working on a feature/change to get thoughts from the other developers and to prevent incompatibillities
- make your fork PugSharp (if you are not already a acknowledged developer who can create branches on our repository)
- make a new branch based on ``main`` with the Name ``feature/examplefeature`` 
- as soon as you have code, please open a draft pull request against the ``main`` branch. 

Before you want to change your draft pull request to a finalized pull request to main you should ask yourself some questions:

- Are my changes update proof?
- Have i changed the admin documentation on the affected parts?
- Have i changed the developer documentation on the affected parts?
- Have i changed the ``README.md`` on the affected parts?
- Have i written all ne needed tests for my code?
- Does my pull request have a good speaking name that represents my changes in the changelog?
- Do i have merged the current main branch into my feature branch?
  
.. - Have i implemented all strings with localised variables? See :doc:`/develop/translation`!

What will happen after you have converted to the finalized pull request:

- Someone of the core developer team assigns a specific label to your pull request, then our sonarcloud code analysis will run on your code. Afterwards please fix the things sonarcloud complains about.
- When your code passes the sonarcloud analysis, someone of the core developer team will review your code and will help you to find missing things or bugs.
- As soon as the review is done, your code will be merged to main and will get merged to available for all users in the next release. Currently we dont have a fixed release cycle.

Prerequisites
----------------------------------------
- Linux / inside WSL2
- `Docker <https://docs.docker.com/engine/install/>`_ installed
- `Make <https://www.gnu.org/software/make>`_ installed (use your os packagemanager)
- Install your IDE of choise (Visual Studio or VSCode works best)
- a Steam User that has started cs2 at least one time (we recommended a seperate account with Steamguard disabled)
- (optional) Download and install `.Net7 SDK <https://dotnet.microsoft.com/download/dotnet/7.0>`_ (highly recommended)

Run / Develop PugSharp locally with the make file
--------------------------------------------------------
To make setting up a dev and testing environment easy, we have added a Makefile to our repository, which gives you easy commands that run a variety of stuff for you.

.. warning::
    For local testing, we use the `joedwards32/cs2 <https://github.com/joedwards32/CS2>`_ docker Image  which gets downloaded automatically in this process. This setup will need around 40GB of free space!


To get started, clone our repository, change your directory into it and run the following command:

.. code-block:: bash
    
    make init-all


It will take some time to install the server, please leave the console open as long as this runs.

As soon as the server has started and loaded ``de_dust2``, and shut down the server with  ``quit``.


To fix the MetaMod installation afterwards, run:

.. code-block:: bash
    
    make fix-metamod


Now you can run your server again with the following command that should be used from now on to start your local server.

.. code-block:: bash
    
    make start-attach


To build and copy the published realease into the Server you can run one of the two following two commands:



.. code-block:: bash
    
    make 



.. code-block:: bash
    
    make build-and-copy-docker


The first one requires you to have the Dotnet SDK installed, the second one just uses Docker to build everything.
You should now have a fully loaded PugSharp plugin inside your running server which can be hotreloaded while the Server is running.

The Makefile offers a lot more commands, that you should check out. You can find the detailed documentation on :doc:`/develop/makefile`. 


Digging deeper
----------------------------------------
If you want to know more about our development thoughts, you might want to take a look into:

- :doc:`/develop/makefile`
- :doc:`/develop/folderstructure`
- :doc:`/develop/cicd`
- :doc:`/develop/release`
- :doc:`/develop/translation`


code analysis
----------------------------------------
we do our code analysis on `sonarcloud`_


.. _sonarcloud: https://sonarcloud.io/project/overview?id=Lan2Play_PugSharp

