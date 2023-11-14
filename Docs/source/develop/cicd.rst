CI / CD
==================================================

Our CI /CD is setted up in our gh repository with github actions. There are two workflows that we use.


test_and_build.yml
-------------------------------

`klick here to see the current test_and_build.yml <https://github.com/Lan2Play/PugSharp/blob/main/.github/workflows/test_and_build.yml>`_


**This workflow runs on:**

* every push to the ``main`` branch
* every push to the ``develop`` branch
* every release / tag that begins with ``v``
* every pull request that is authorized to run workflows

**Excluted file changes that will not trigger this workflow in any of the above cases:**

* ``README.md``
* ``!Makefile``

**This workflow does the following things:**

* run the dotnet tests and upload them to sonarcloud
* get set all download links and versions dynamically
* generates an example metadata file that maybe someday serves the purpose of easy updating
* build and package linux packages on ubuntu (the release configuration is only used when a stable release tag, for example ``v1.0.0`` is created, otherwise the debug configuration is used)
* build and package windows packages on windows (the release configuration is only used when a stable release tag, for example ``v1.0.0`` is created, otherwise the debug configuration is used)
* Upload the current build as artifacts
* Upload files to the corresponding release, if the run is triggered with the creation of a release

for more detail check out `the current version <https://github.com/Lan2Play/PugSharp/blob/main/.github/workflows/test_and_build.yml>`_


website_build.yml
-------------------------------

`klick here to see the current website_build.yml <https://github.com/Lan2Play/PugSharp/blob/main/.github/workflows/website_build.yml>`_

**This workflow runs only when one of the following files/paths are changed and one of the conditions below is true:**

**Files/paths**

* ``Docs/**``
* ``packaging/Docs/**``
* ``.github/workflows/website_build.yml``

**conditions**

* every push to the ``main`` branch
* every release / tag that begins with ``v``
* every pull request that is authorized to run workflows


**This workflow does the following things:**

* build the pugsharp documentation using `docker-sphinxbuild <https://github.com/Lan2Play/docker-sphinxbuild>`_ 
* publish the page to `https://github.com/Lan2Play/pugsharp.lan2play.de <https://github.com/Lan2Play/pugsharp.lan2play.de>`_ if the triggering release / tag begins with ``v`` or the commit text contains ``forcepublish``