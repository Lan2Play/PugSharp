Admins quickstart guide
==================================================

Prerequisites
-------------------------------
- Linux Dedicated Server
- `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_
- `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  (depending on your selected package, there is no need to install this seperatly)
- Dotnet Runtime needed by `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ (depending on your selected package, there is no need to install this seperatly)

   .. Installation - Stable
   .. -------------------------------
   .. We have multiple packages depending on your operating system and if you want to install `CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp>`_ and the dotnet runtime seperatly. You will find some ``X.X.X`` placeholders in this guide, replace them with the regarding version.

   .. Linux - Stable with CounterStrikeSharp and Dotnet Runtime (recommended)
   .. ................................................................................

   .. - Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Linux Dedicated Server 
   .. - Download one of our releases including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ and the Dotnet Runtime named ``PugSharp_with_cssharp_and_runtime_linux_X.X.X.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder. 
   .. - Have fun and report bugs :D

   .. Windows - Stable with CounterStrikeSharp and Dotnet Runtime (recommended)
   .. ..............................................................................

   .. - Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Windows Dedicated Server 
   .. - Download one of our releases including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ and the Dotnet Runtime named ``PugSharp_with_cssharp_and_runtime_windows_X.X.X.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder. 
   .. - Have fun and report bugs :D

   .. Linux - Stable with CounterStrikeSharp 
   .. ........................................

   .. .. warning::
   ..    This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so. Only use this if you know why you want to do that.

   .. - Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Linux Dedicated Server 
   .. - Download one of our releases including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  named ``PugSharp_with_cssharp_linux_X.X.X.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder. 
   .. - check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-linux-x64.tar.gz`` linked there. 
   .. - Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
   .. - move the contents of  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/X.X.X`` into  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/`` of your csgo server
   .. - Have fun and report bugs :D

   .. Windows - Stable With CounterStrikeSharp 
   .. ..........................................

   .. .. warning::
   ..    This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so. Only use this if you know why you want to do that.

   .. - Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Windows Dedicated Server 
   .. - Download one of our releases including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ named ``PugSharp_with_cssharp_windows_X.X.X.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder.
   .. - check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-win-x64.zip`` linked there. 
   .. - Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
   .. - Have fun and report bugs :D


   .. Linux Stable
   .. ................................

   .. .. warning::
   ..    This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so and also update `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  if the version of it that PugSharp depends on changes. Only use this if you know why you want to do that.

   .. - Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Linux Dedicated Server 
   .. - Download one of our releases named ``PugSharp_linux_X.X.X.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder. 
   .. - check the ``CounterStrikeSharp.API`` dependency version in `the csproj file of Pugsharp <https://github.com/Lan2Play/PugSharp/blob/main/PugSharp/PugSharp.csproj>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the `CounterStrikeSharp release <https://github.com/roflmuffin/CounterStrikeSharp/releases>`_ that is configured there. 
   .. - Extract the contents of the CounterStrikeSharp into the csgo folder of your csgo server
   .. - check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-linux-x64.tar.gz`` linked there. 
   .. - Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
   .. - move the contents of  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/X.X.X`` into  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/`` of your csgo server
   .. - Have fun and report bugs :D


   .. Windows Stable
   .. .................................

   .. .. warning::
   ..    This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so and also update `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  if the version of it that PugSharp depends on changes. Only use this if you know why you want to do that.

   .. - Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Windows Dedicated Server 
   .. - Download one of our releases named ``PugSharp_windows_X.X.X.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder.
   .. - check the ``CounterStrikeSharp.API`` dependency version in `the csproj file of Pugsharp <https://github.com/Lan2Play/PugSharp/blob/main/PugSharp/PugSharp.csproj>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the `CounterStrikeSharp release <https://github.com/roflmuffin/CounterStrikeSharp/releases>`_ that is configured there. 
   .. - Extract the contents of the CounterStrikeSharp into the csgo folder of your csgo server
   .. - check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-win-x64.zip`` linked there. 
   .. - Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
   .. - Have fun and report bugs :D



Installation - Beta
-------------------------------
We have multiple packages depending on your operating system and if you want to install `CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp>`_ and the dotnet runtime seperatly. You will find some ``X.X.X`` placeholders in this guide, replace them with the regarding version.

Linux - Beta with CounterStrikeSharp and Dotnet Runtime (recommended)
................................................................................

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Linux Dedicated Server 
- Download one of our pre-releases including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ and the Dotnet Runtime named ``PugSharp_with_cssharp_and_runtime_linux_X.X.X-beta.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder. 
- Have fun and report bugs :D

Windows - Beta with CounterStrikeSharp and Dotnet Runtime (recommended)
..............................................................................

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Windows Dedicated Server 
- Download one of our pre-releases including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ and the Dotnet Runtime named ``PugSharp_with_cssharp_and_runtime_windows_X.X.X-beta.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder. 
- Have fun and report bugs :D

Linux - Beta with CounterStrikeSharp 
........................................

.. warning::
   This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so. Only use this if you know why you want to do that.

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Linux Dedicated Server 
- Download one of our pre-releases including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  named ``PugSharp_with_cssharp_linux_X.X.X-beta.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder. 
- check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-linux-x64.tar.gz`` linked there. 
- Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
- move the contents of  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/X.X.X`` into  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/`` of your csgo server
- Have fun and report bugs :D

Windows - Beta With CounterStrikeSharp 
..........................................

.. warning::
   This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so. Only use this if you know why you want to do that.

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Windows Dedicated Server 
- Download one of our pre-releases including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ named ``PugSharp_with_cssharp_windows_X.X.X-beta.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder.
- check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-win-x64.zip`` linked there. 
- Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
- Have fun and report bugs :D


Linux Beta
................................

.. warning::
   This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so and also update `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  if the version of it that PugSharp depends on changes. Only use this if you know why you want to do that.

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Linux Dedicated Server 
- Download one of our pre-releases named ``PugSharp_linux_X.X.X-beta.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder. 
- check the ``CounterStrikeSharp.API`` dependency version in `the csproj file of Pugsharp <https://github.com/Lan2Play/PugSharp/blob/main/PugSharp/PugSharp.csproj>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the `CounterStrikeSharp release <https://github.com/roflmuffin/CounterStrikeSharp/releases>`_ that is configured there. 
- Extract the contents of the CounterStrikeSharp into the csgo folder of your csgo server
- check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-linux-x64.tar.gz`` linked there. 
- Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
- move the contents of  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/X.X.X`` into  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/`` of your csgo server
- Have fun and report bugs :D


Windows Beta
.................................

.. warning::
    This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so and also update `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  if the version of it that PugSharp depends on changes. Only use this if you know why you want to do that.

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Windows Dedicated Server 
- Download one of our pre-releases named ``PugSharp_windows_X.X.X-beta.zip`` from our `releases <https://github.com/Lan2Play/PugSharp/releases>`_ and unpack it to your csgo folder.
- check the ``CounterStrikeSharp.API`` dependency version in `the csproj file of Pugsharp <https://github.com/Lan2Play/PugSharp/blob/main/PugSharp/PugSharp.csproj>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the `CounterStrikeSharp release <https://github.com/roflmuffin/CounterStrikeSharp/releases>`_ that is configured there. 
- Extract the contents of the CounterStrikeSharp into the csgo folder of your csgo server
- check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-win-x64.zip`` linked there. 
- Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
- Have fun and report bugs :D





Installation - Alpha
-------------------------------
We have multiple packages depending on your operating system and if you want to install `CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp>`_ and the dotnet runtime seperatly. You will find some ``X.X.X`` placeholders in this guide, replace them with the regarding version.

Linux - Alpha with CounterStrikeSharp and Dotnet Runtime (recommended)
................................................................................

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Linux Dedicated Server 
- Download one of our alphas including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ and the Dotnet Runtime named ``latest_build_linux_with_cssharp_and_runtime`` from our `alpha builds <https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml>`_ (click on a run, scroll down to artifacts) and unpack the containing ``PugSharp_with_cssharp_and_runtime_linux_X.X.X-alpha.zip`` to your csgo folder. 
- Have fun and report bugs :D

Windows - Alpha with CounterStrikeSharp and Dotnet Runtime (recommended)
..............................................................................

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Windows Dedicated Server 
- Download one of our alphas including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ and the Dotnet Runtime named ``latest_build_windows_with_cssharp_and_runtime`` from our `alpha builds <https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml>`_ (click on a run, scroll down to artifacts) and unpack the containing ``PugSharp_with_cssharp_and_runtime_windows_X.X.X-alpha.zip`` to your csgo folder. 
- Have fun and report bugs :D

Linux - Alpha with CounterStrikeSharp 
........................................

.. warning::
   This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so. Only use this if you know why you want to do that.

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Linux Dedicated Server 
- Download one of our alphas including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  named ``latest_build_linux_with_cssharp`` from our `alpha builds <https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml>`_ (click on a run, scroll down to artifacts) and unpack the containing ``PugSharp_with_cssharp_linux_X.X.X-alpha.zip`` to your csgo folder. 
- check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-linux-x64.tar.gz`` linked there. 
- Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
- move the contents of  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/X.X.X`` into  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/`` of your csgo server
- Have fun and report bugs :D

Windows - Alpha With CounterStrikeSharp 
..........................................

.. warning::
   This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so. Only use this if you know why you want to do that.

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Windows Dedicated Server 
- Download one of our alphas including `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ named ``latest_build_windows_with_cssharp`` from our `alpha builds <https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml>`_ (click on a run, scroll down to artifacts) and unpack the containing ``PugSharp_with_cssharp_windows_X.X.X-alpha.zip`` to your csgo folder.
- check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-win-x64.zip`` linked there. 
- Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
- Have fun and report bugs :D


Linux Alpha
................................

.. warning::
   This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so and also update `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  if the version of it that PugSharp depends on changes. Only use this if you know why you want to do that.

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Linux Dedicated Server 
- Download one of our alphas named ``latest_build_linux`` from our `alpha builds <https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml>`_ (click on a run, scroll down to artifacts) and unpack the containing ``PugSharp_linux_X.X.X-alpha.zip`` to your csgo folder. 
- check the ``CounterStrikeSharp.API`` dependency version in `the csproj file of Pugsharp <https://github.com/Lan2Play/PugSharp/blob/main/PugSharp/PugSharp.csproj>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the `CounterStrikeSharp release <https://github.com/roflmuffin/CounterStrikeSharp/releases>`_ that is configured there. 
- Extract the contents of the CounterStrikeSharp into the csgo folder of your csgo server
- check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-linux-x64.tar.gz`` linked there. 
- Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
- move the contents of  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/X.X.X`` into  ``addons/counterstrikesharp/dotnet/shared/Microsoft.NETCore.App/`` of your csgo server
- Have fun and report bugs :D


Windows Alpha
.................................

.. warning::
    This method is not recommended, since you have to make sure you update your dotnet runtime yourself, if `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_ does so and also update `CounterStrikeSharp by roflmuffin <https://github.com/roflmuffin/CounterStrikeSharp>`_  if the version of it that PugSharp depends on changes. Only use this if you know why you want to do that.

- Install `MetaMod 2.0 <https://www.sourcemm.net/downloads.php?branch=dev>`_ on your Windows Dedicated Server 
- Download one of our alphas named ``latest_build_linux`` from our `alpha builds <https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml>`_ (click on a run, scroll down to artifacts) and unpack the containing ``PugSharp_windows_X.X.X-alpha.zip`` to your csgo folder.
- check the ``CounterStrikeSharp.API`` dependency version in `the csproj file of Pugsharp <https://github.com/Lan2Play/PugSharp/blob/main/PugSharp/PugSharp.csproj>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the `CounterStrikeSharp release <https://github.com/roflmuffin/CounterStrikeSharp/releases>`_ that is configured there. 
- Extract the contents of the CounterStrikeSharp into the csgo folder of your csgo server
- check the ``Add dotnet runtime`` step in `the ci file of CounterStrikeSharp <https://github.com/roflmuffin/CounterStrikeSharp/blob/main/.github/workflows/cmake-single-platform.yml>`_ (make sure you use the one from the corresponding release instead the one from the ``main`` branch) and download the ``aspnetcore-runtime-X.X.X-win-x64.zip`` linked there. 
- Extract the contents of the dotnet runtime into ``addons/counterstrikesharp/dotnet`` of your csgo server
- Have fun and report bugs :D



Notes
-------------------------------

.. warning::
   This Plugin is in a very early state of development and some things are not fully working right now! Please report any issues you find either on Discord (you can find it on the `home of the docs <https://pugsharp.lan2play.de/>`_ ) or in our issues tab on `Github <https://github.com/Lan2Play/PugSharp/issues>`_ 


