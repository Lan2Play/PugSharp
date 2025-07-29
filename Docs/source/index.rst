
PugSharp
==================================================

|PugSharp_test_and_build| |PugSharp_website_build| |Lines of Code| |Quality Gate Status|
|Duplicated Lines (%)| |Coverage| |Maintainability Rating| |Reliability
Rating| |Security Rating| |Vulnerabilities| |Code Smells| |Bugs| |PugSharp Translation|

`Pugsharp <https://github.com/Lan2Play/PugSharp>`_ is a PUG System Plugin for CS2 based on the awesome  `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_. 

Its intended purpose is to be used with our fork of `eventula <https://github.com/Lan2Play/eventula-manager>`_, but ofc can be used in a different environment or with different software as well.
If you want to use it with a different software instead of standalone or with eventula, currently these are your options:

- `CS2 PugSharp Manager <https://github.com/DuelistRag3/cs2-pugsharp-manager>`_ by DuelistRag3, which is a web based Tournament System that interfaces with PugSharp
- `G5V <https://github.com/PhlexPlexico/G5V>`_ in combination with `G5API <https://github.com/phlexplexico/G5API>`_ by PhlexPlexico, which should work since we have implemented api compatibility with Get5 

If you implement software to interface with PugSharp let us know please!


.. warning::
   This Plugin is in development and maybe some things are not fully working right now! Please report any issues you find either on Discord or in our issues tab on `Github <https://github.com/Lan2Play/PugSharp/issues>`_ 

.. image:: https://discordapp.com/api/guilds/748086853449810013/widget.png?style=banner3
   :target: https://discord.gg/zF5C9WPWFq


Admins quickstart note
----------------------
- If you want to deploy PugSharp, please take a look into the :doc:`/admin/quickstart`
- If you want to help with the translation of PugSharp, please check out our :doc:`/develop/translation` section

Developer note
----------------------
- Github url: https://github.com/Lan2Play/PugSharp
- If you want to contribute to PugSharp, please take a look into the :doc:`/develop/quickstart`
- If you want to help with the translation of PugSharp, please check out our :doc:`/develop/translation` section


Table of contents
----------------------

.. toctree::
   :caption: General
   :maxdepth: 2

   contribution
   license

   
.. toctree::
   :caption: Admins guide
   :maxdepth: 1

   admin/quickstart
   admin/commands
   admin/configuration

   
.. toctree::
   :caption: Developers guide
   :maxdepth: 1

   develop/quickstart
   develop/makefile
   develop/debugging
   develop/folderstructure
   develop/cicd
   develop/release
   develop/translation

.. |PugSharp_test_and_build| image:: https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml/badge.svg
   :target: https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml
.. |PugSharp_website_build| image:: https://github.com/Lan2Play/PugSharp/actions/workflows/website_build.yml/badge.svg
   :target: https://github.com/Lan2Play/PugSharp/actions/workflows/website_build.yml
.. |Lines of Code| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=ncloc
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |Quality Gate Status| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=alert_status
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |Duplicated Lines (%)| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=duplicated_lines_density
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |Coverage| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=coverage
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |Maintainability Rating| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=sqale_rating
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |Reliability Rating| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=reliability_rating
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |Security Rating| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=security_rating
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |Vulnerabilities| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=vulnerabilities
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |Code Smells| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=code_smells
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |Bugs| image:: https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=bugs
   :target: https://sonarcloud.io/summary/overall?id=Lan2Play_PugSharp
.. |PugSharp Translation| image:: https://translate.lan2play.de/widgets/pugsharp/-/pugsharp/svg-badge.svg
    :alt: PugSharp Translation status
    :target: https://translate.lan2play.de/engage/pugsharp/



Indices and tables
==================

* :ref:`genindex`
* :ref:`modindex`
* :ref:`search`