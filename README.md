# PugSharp

[![PugSharp_test_and_build](https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml/badge.svg)](https://github.com/Lan2Play/PugSharp/actions/workflows/test_and_build.yml)
[![PugSharp_website_build](https://github.com/Lan2Play/PugSharp/actions/workflows/website_build.yml/badge.svg)](https://github.com/Lan2Play/PugSharp/actions/workflows/website_build.yml)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Lan2Play_PugSharp&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Lan2Play_PugSharp)

<!-- [![Translation status](https://translate.lan2play.de/widgets/netevent-client/-/netevent-client/svg-badge.svg)](https://translate.lan2play.de/engage/netevent-client/) -->

Pugsharp is a PUG System Plugin for CS2 based on the awsome [CounterStrikeSharp by roflmuffin](https://github.com/roflmuffin/CounterStrikeSharp). Its intended purpose is to be used with our fork of [eventula](https://github.com/Lan2Play/eventula-manager), but ofc can be used in a different environment as well. We also try to build an compatible api with most apis of the awesome [Get5](https://github.com/splewis/get5).

> **Warning**
> This Plugin is in a very early state of development and some things are not fully working right now! Please report any issues you find either on Discord or in our issues tab .

You can find the full documentation on [pugsharp.lan2play.de](https://pugsharp.lan2play.de) .

If you want to help developing or translating, join our discord:

[![Discord](https://discordapp.com/api/guilds/748086853449810013/widget.png?style=banner3)](https://discord.gg/zF5C9WPWFq)

## Tanslation

[![Translation status](https://translate.lan2play.de/widgets/pugsharp/-/multi-auto.svg)](https://translate.lan2play.de/engage/pugsharp/)

## Usage

> **Warning**
> Don't use this in production right now!

If you want to know how to use PugSharp, hop over to our [Documentation](https://pugsharp.lan2play.de).


## Working features

- [x] Configuration via http(s) json ([description](#Match_Config) and [example config](#MatchConfig))
- [x] Configuration via json file
- [x] api reporting to a http(s) server
  - [x] Report start of match
  - [x] Report round results
  - [x] Report map ended
  - [x] Report series ended (for bo3, ...)
- [x] automatic team assignment
- [x] map and starting team vote
- [x] automatic pause if player disconnects
- [x] pause / unpause feature
- [x] demo recording
- [x] demo upload
- [x] automatic round backups
- [x] restore round backups

## partially working features

- Api compatibility for the awesome [Get5](https://github.com/splewis/get5)

## Credits

- Awesome match plugin for CS:GO and inspiration: [Get5](https://github.com/splewis/get5)
- Plugin Framework: [CounterStrikeSharp by roflmuffin](https://github.com/roflmuffin/CounterStrikeSharp)
- Docker test setup: [joedwards32/cs2](https://github.com/joedwards32/CS2)
- Metamod fix script: [ghostcap-gaming/cs2-metamod-re-enable-script](https://github.com/ghostcap-gaming/cs2-metamod-re-enable-script)
- Metamod: https://www.sourcemm.net/
- Version detection stolen (and modified) with permission from [CS2-AutoUpdater](https://github.com/dran1x/CS2-AutoUpdater)

