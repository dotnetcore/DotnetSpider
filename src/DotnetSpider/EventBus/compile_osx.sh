#!/bin/bash

~/.nuget/packages/grpc.tools/1.22.0/tools/macosx_x64/protoc \
    --proto_path=.\
    --csharp_out=. \
    event.proto