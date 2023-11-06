#!/bin/bash

sed -i 's|#PasswordAuthentication yes|PasswordAuthentication yes|g' /etc/ssh/sshd_config

echo "${USER}:steam" | chpasswd

service ssh start

su "${USER}" -c \
    "cd \"${HOMEDIR}\" \
    && ./entry.sh"