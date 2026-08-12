#!/bin/sh
set -e

chown steam:steam /app /home/steam/steamcmd /home/steam 2>/dev/null || true

exec gosu steam "$@"