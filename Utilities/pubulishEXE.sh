#!/bin/bash

current_dir=$(pwd)
Root="$(dirname "$current_dir")"
# echo $Root

# dotnet publish /p:Version=$CurrentVersion /p:FileVersion=$CurrentVersion -p:PublishProfile=wrapper-x64 --configuration Release "$Root\Desktop.Win"
if [[ -f "$Root/Desktop.Win.Wrapper/Remotely_Desktop.zip" ]]
then
    sudo rm $Root/Desktop.Win.Wrapper/Remotely_Desktop.zip
fi

dotnet msbuild "$Root/Desktop.Win.Wrapper" -t:Build -p:configuration=Release -p:plateform=x64 
