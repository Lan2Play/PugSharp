
Debugging
==================================================

On Linux with vscode
----------------------------------------

.. warning::
    Currently this guide is only suited for Linux. The development on windows including the testing and debugging in a windows based cs2 server is possible right now, but our makefile and the documentation is lacking how. We will add the support for that soon.

* You have to make sure you have followed the :doc:`/develop/quickstart` and you have your server already running.

* Open the project in vscode

* Make sure you have the recommended extensions installed in vscode (press ``F1`` and select ``Extensions: Show Recommended Extensions``)

* Make sure you have installed the `.NET SDK <https://learn.microsoft.com/en-us/dotnet/core/install/linux>`_ locally via your package manager (check which version is currently required `here <https://github.com/Lan2Play/PugSharp/blob/main/PugSharp/PugSharp.csproj>`_ ) 

* run ``make install-vsdebug`` ( :ref:`develop/makefile:install-vsdebug` ) to install vsdebug inside the cs2 container (you have to do this every time you restart the cs server container)

* go to the ``Run and Debug`` tab in vscode and select the configuration ``PugSharp Docker Attach`` from the list at the top and klick on the ``Start Debugging`` button or hit ``F5`` on your keyboard (which should work after that even if you don't have the tab opened).

* you will now see a process selection on top of your window, where you have to select the ``cs2`` process

* run ``make`` one more time, to make sure the code that is running inside your server is acutally the code in your ide and now your debugging / pausing / breakpoints should work.

* you can leave the debugging running and use ``make`` after you make changes to the code, the server will hotreload your changes and you should be able to change and debug the code without restarting anything.