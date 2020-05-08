@echo off

setlocal ENABLEDELAYEDEXPANSION

cd /d %~dp0

mklink /D Assets\MRTK ..\MRTK-Quest\External\MixedRealityToolkit-Unity\Assets\MRTK
mklink Assets\MRTK.meta ..\MRTK-Quest\External\MixedRealityToolkit-Unity\Assets\MRTK.meta
mklink /D Assets\MixedRealityToolkit.ThirdParty ..\MRTK-Quest\Assets\MixedRealityToolkit.ThirdParty
mklink Assets\MixedRealityToolkit.ThirdParty.meta ..\MRTK-Quest\Assets\MixedRealityToolkit.ThirdParty.meta

pause
