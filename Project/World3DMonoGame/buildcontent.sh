#!/bin/sh
rm -r bin/Debug/net7.0/Content/
dotnetx64 tool run mgcb -@:Content/Content.mgcb -o:bin/Debug/net7.0/ -r;find bin/Debug/net7.0/Content/ -mindepth 2 -type f -exec mv -i '{}' bin/Debug/net7.0/Content/ ';'
echo "Complete."
