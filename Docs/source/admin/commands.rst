Commands
==================================================

Introduction
----------------------------------------
The commands listed below can be executed by either typing it into the ingame chat.

If you want to use them from your console, remove the ``!`` in front of them.


Player Commands
----------------------------------------

+--------------------------+-----------------------------------------------------------------------------------+
|         Command          |                                    Description                                    |
+==========================+===================================================================================+
| ``!ready``               | Mark the player as ready                                                          |
+--------------------------+-----------------------------------------------------------------------------------+
| ``!pause``               | Pause the match in the next freezetime                                            |
+--------------------------+-----------------------------------------------------------------------------------+
| ``!unpause``             | Unpause the match. To continue the match, both teams have to ``!unpause``.        |
+--------------------------+-----------------------------------------------------------------------------------+
| ``!kill``or ``!suicide`` | Kill the current player if allowed by the :ref:`admin/configuration:matchconfig`. |
+--------------------------+-----------------------------------------------------------------------------------+


Admin/Rcon Commands
-------------------

These commands are available through rcon or to users with the required permissions. See :ref:`admin/configuration:CounterstrikeSharp Configs` 

``<requiredParameter>`` marks parameters that are required for commands

``[optionalParameter]`` marks parameters that can be optionally be added to commands

+----------------------------------------+---------------------------------------------------------------------------------------------------------------------------------------------------+
|                Command                 |                                                                    Description                                                                    |
+========================================+===================================================================================================================================================+
| ``!ps_loadconfig <url> [authToken]``   | Load a :ref:`admin/configuration:matchconfig` to initialize a match                                                                               |
+----------------------------------------+---------------------------------------------------------------------------------------------------------------------------------------------------+
| ``!ps_loadconfigfile <filename>``      | Load a :ref:`admin/configuration:matchconfig` to initialize a match. The file path must be either rooted or relative to ``csgo/PugSharp/Config/`` |
+----------------------------------------+---------------------------------------------------------------------------------------------------------------------------------------------------+
| ``!ps_restorematch <matchId> <round>`` | Restores match in the given round.                                                                                                                |
+----------------------------------------+---------------------------------------------------------------------------------------------------------------------------------------------------+
| ``!ps_stopmatch``                      | Danger! Stops the current match immediately and resets the server.                                                                                |
+----------------------------------------+---------------------------------------------------------------------------------------------------------------------------------------------------+
| ``!ps_dumpmatch``                      | Dumps the current matchstate and config to console                                                                                                |
+----------------------------------------+---------------------------------------------------------------------------------------------------------------------------------------------------+
