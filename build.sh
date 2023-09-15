#!/bin/bash

dotnet publish -r linux-x64 -p:PublishSingleFile=true -p:DebugType=embedded --self-contained true -o .