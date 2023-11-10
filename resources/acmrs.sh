#!/bin/bash

# acmrs.sh by https://github.com/ghostcap-gaming/cs2-metamod-re-enable-script
TARGET_DIR="cs2/game/csgo"
GAMEINFO_FILE="${TARGET_DIR}/gameinfo.gi"

if [ ! -f "${GAMEINFO_FILE}" ]; then
    echo "Error: ${GAMEINFO_FILE} does not exist in the specified directory."
    exit 1
fi

NEW_ENTRY="            Game    csgo/addons/metamod"

if grep -Fxq "$NEW_ENTRY" "$GAMEINFO_FILE"; then
    echo "The entry '$NEW_ENTRY' already exists in ${GAMEINFO_FILE}. No changes were made."
else
    awk -v new_entry="$NEW_ENTRY" '
        BEGIN { found=0; }
        // {
            if (found) {
                print new_entry;
                found=0;
            }
            print;
        }
        /Game_LowViolence/ { found=1; }
    ' "$GAMEINFO_FILE" > "$GAMEINFO_FILE.tmp" && mv "$GAMEINFO_FILE.tmp" "$GAMEINFO_FILE"

    echo "The file ${GAMEINFO_FILE} has been modified successfully. '$NEW_ENTRY' has been added."
fi