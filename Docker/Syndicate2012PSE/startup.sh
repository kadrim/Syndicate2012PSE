#!/bin/bash

if [ "$HEADLESS" == "true" ]
    then
        echo "Running in headless mode"
        xvfb-run wine Syndicate2012Server.exe
    else
        echo "Running in GUI mode"
        wine Syndicate2012Server.exe
fi
