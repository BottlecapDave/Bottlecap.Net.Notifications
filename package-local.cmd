dotnet restore ./src --force

dotnet build ./src/Bottlecap.Net.Notifications/ -c Release -o out /p:Version=0.1.0-alpha
dotnet pack ./src/Bottlecap.Net.Notifications/ -c Release -o out /p:Version=0.1.0-alpha

dotnet build ./src/Bottlecap.Net.Notifications.Transporters.SendGrid/ -c Release -o out /p:Version=0.1.0-alpha
dotnet pack ./src/Bottlecap.Net.Notifications.Transporters.SendGrid/ -c Release -o out /p:Version=0.1.0-alpha

XCOPY "src/Bottlecap.Net.Notifications/out" "C:\nuget.local" /s
XCOPY "src/Bottlecap.Net.Notifications.Transporters.SendGrid/out" "C:\nuget.local" /s