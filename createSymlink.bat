@echo off

setlocal ENABLEDELAYEDEXPANSION

cd /d %~dp0

set FOLDER_1=MixedRealityToolkit
set FOLDER_2=MixedRealityToolkit.SDK
set FOLDER_3=MixedRealityToolkit.Services
set FOLDER_4=MixedRealityToolkit.Providers
set FOLDER_5=MixedRealityToolkit.Examples
set FOLDER_6=MixedRealityToolkit.Extensions
set FOLDER_7=MixedRealityToolkit.Tools

set i=1
:BEGIN
call set f=%%FOLDER_!i!%%
if defined f (
  rem echo Create symbolic link !f!
  mklink /D Assets\!f! ..\MRTKExtensionForOculusQuest\External\MixedRealityToolkit-Unity\Assets\!f!
  mklink Assets\!f!.meta ..\MRTKExtensionForOculusQuest\External\MixedRealityToolkit-Unity\Assets\!f!.meta
  set /A i+=1
  goto :BEGIN
)

mklink /D Assets\MixedRealityToolkit.ThirdParty ..\MRTKExtensionForOculusQuest\Assets\MixedRealityToolkit.ThirdParty
mklink Assets\MixedRealityToolkit.ThirdParty.meta ..\MRTKExtensionForOculusQuest\Assets\MixedRealityToolkit.ThirdParty.meta

pause
