@echo off
SET a = "0"
:startover
echo (%time%) App started.
cd ../src/API
dotnet watch run

if %a%=="0" (
	echo (%time%) WARNING: App closed or crashed, restarting with a=%a%
	set a="1"

	goto startover
)

else(echo "Exit")
