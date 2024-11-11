
Translation / i18n
==================================================

Currently there is no i18n implemented, but this will follow soon.

* PugSharp itself has i18n abillity
* The documentation will be in english only at the moment


PugSharp Translation
----------------------------------------
translation status
^^^^^^^^^^^^^^^^^^^
.. image:: https://translate.lan2play.de/widgets/pugsharp/-/pugsharp/multi-auto.svg
    :alt: Translation status
    :target: https://translate.lan2play.de/engage/pugsharp/

string structure 
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

You will find the following syntax inside your strings, that influence the output text later:

+-----------+----------------------+---------------+---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
|   Type    |    Syntax example    | color in chat |                                                                                                           Description                                                                                                           |
+===========+======================+===============+=================================================================================================================================================================================================================================+
| Variable  | ``{0:variableName}`` | none          | This will be filled by PugSharp dynamically with the content of a Variable. The number infront indicates the count of the variable. Don't change these and use them in your translation as they are in the English Strings      |
+-----------+----------------------+---------------+---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
| Error     | ``!!string!!``       | red           | This should be used for errors or critical messages. You should translate the content inbetween the ``!!`` and ``!!`` to your language if the content is not a variable (like shown above)                                      |
+-----------+----------------------+---------------+---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
| Command   | ```string```         | green         | This should be used to print commands to users. You should translate the content inbetween the ````` and ````` to your language if the content is not a variable (like shown above)                                             |
+-----------+----------------------+---------------+---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
| Highlight | ``**string**``       | blue          | This should be used to print highlighted messages to users. This will be printed in blue, you should translate the content inbetween the ``**`` and ``**`` to your language if the content is not a variable (like shown above) |
+-----------+----------------------+---------------+---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+

translate with weblate
^^^^^^^^^^^^^^^^^^^^^^^

You can find the client translation on our `Weblate project`_

.. _Weblate project: https://translate.lan2play.de/engage/pugsharp/


translate without weblate 
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

* The resource files are located in ``PugSharp.Translation/Properties/`` .
* The ``Resources.resx`` is the english and default resource file and for every other language there either is a a ``Resources.lc.resx`` (lc = two letter language code. for example: de for german) file or you can create one.


