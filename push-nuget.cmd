@echo off

set nuget="tools\nuget\nuget.exe"
set output="artifacts"
set nugetkey=
set nugetserver=http://nexus.infocaster.net/repository/infocaster-nuget/

IF [%1] == [] GOTO version_not_set

echo PUSHING PACKAGES

%nuget% push %output%\Infocaster.UmbracoAwesome.DateFolders.%1.nupkg %nugetkey% -Source %nugetserver%

GOTO :eof

:version_not_set
echo Version not set, use "push-nuget versionnumber" to push
exit /b 1
