#!/usr/bin/bash

chmod -R 777 logs
chmod -R 777 player

if [ "$HEADLESS" == "true" ]
    then
        echo "Running in headless mode"
        sudo -u appuser xvfb-run -a wine Syndicate2012Server.exe
    else
        echo "Running in GUI mode"
        sudo -u appuser wine Syndicate2012Server.exe
fi
