## Crash Twinsanity Editor

## Description

This is an editor of game files for Crash Twinsanity (PS2/XBOX) and Super Monkey Ball Adventure (PS2/PSP).  
(Editor originally created by NeoKesha)

The following formats are supported:

- .BD/.BH (PS2 file archives) extraction and rebuilding
- .MB/.MH (PS2 music archives) extraction and rebuilding
- .RM2 (PS2 level resources) editing
- .SM2 (PS2 scenery resources) editing
- .RMX (XBOX level resources) editing
- .SMX (XBOX scenery resources) editing
- .RM2/.SM2 editing support of Crash Twinsanity demo
- .RM2/.SM2 viewing support of Super Monkey Ball Adventure (PS2)
- .RM (PSP level resources) limited editing
- .SM (PSP level resources) limited editing

## Installation

- Download the latest version from [releases](https://github.com/preunpatching/twinsanity-editor/releases)
- Visual C++ 2013 Redistributable is required to use the viewer.

## Current roadmap

Here's the list of what needs to be implemented:

- [x] Model viewing
- [x] Collision viewing
- [x] Scenery viewing
- [x] Sound playback
- [x] Fully functional editing GUI
- [x] Animations playback
- [x] Fully implement demo editing
- [ ] Ditch any unnecessery dependencies
- [ ] General code refactoring and rewriting
- [ ] More API optimizations?
- [ ] A lot of other smaller stuff

## Compiling

Clone the repository (or download the ZIP archive), open the solution project in at least Visual Studio 2017 with .NET 4.7 (VS 2026 and .NET 4.8 confirmed to work) and compile it.
