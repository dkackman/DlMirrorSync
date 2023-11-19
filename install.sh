#!/bin/bash

# run with bash install.sh (not sh install.sh)

set -o errexit

# Create the destination directory if it doesn't exist
sudo mkdir -p /usr/local/bin/dlsync
# and copy the standa lone binarty to it
sudo cp -r ./publish/standalone/linux-x64/* /usr/local/bin/dlsync/

# Get the directory of the current script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

# Get the username of the logged in user
# the service will run as the installing user
# pass the username as the first argument to the script
# default to logged in user if not supplied
USERNAME=${1:-$(logname)}

# Replace all instances of 'USERNAME' with the runas user's name
# in 'dlsync.service' and save as '/etc/systemd/system/dlsync.service'
sudo sed "s/USERNAME/$USERNAME/g" "$DIR/dlsync.service" > "/etc/systemd/system/dlsync.service"


sudo systemctl start dlsync
sudo systemctl status dlsync
sudo systemctl enable dlsync
