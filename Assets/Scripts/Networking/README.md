# Networking

## Purpose

Contains all multiplayer functionality.

## Structure

FishNet -> Player ownership -> Steam transport -> Lobby management

## Scripts

### DynamicSpawnPoint

Registers runtime spawn locations.

### NetworkPlayerSetup

Initializes local player ownership and disables
local-only systems on remote proxies.

### SteamNetworkManager

Responsibilities

- create lobby
- join lobby
- leave lobby
- synchronize FishNet
- reconnect after disconnect

## Dependencies

- Steamworks.NET
- FishySteamworks
- FishNet