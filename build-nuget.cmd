@echo off

set nuget="tools\nuget\nuget.exe"
set output="artifacts"

IF [%1] == [] GOTO version_not_set

echo BUILDING PACKAGES

%nuget% pack src\Infocaster.Umbraco.DateFolders\Infocaster.Umbraco.DateFolders.nuspec -outputdirectory %output% -version %1

GOTO :eof

:version_not_set
echo Version not set, use "build-nuget versionnumber" to build
exit /b 1
