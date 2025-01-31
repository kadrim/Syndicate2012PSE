#!/usr/bin/bash

chmod -R 777 logs
chmod -R 777 player

if [ "$HEADLESS" == "true" ]
    then
        echo "Running in headless mode"
        sudo -u appuser bash -c 'umask 0000 && xvfb-run -a wine Syndicate2012Server.exe'
    else
        echo "Running in GUI mode"
        sudo -u appuser bash -c 'umask 0000 && wine Syndicate2012Server.exe'
fi
