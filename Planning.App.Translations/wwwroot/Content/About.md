# Timetable Planner

The **Timetable Planner** is an application for planning model railway operations
based on schedules. It is designed primarily for FREMO module meetings, but also
works for fixed club layouts and home layouts.

## Features

- Define track layouts with stations, tracks, and stretches.
- Create and edit train schedules with automatic time calculations.
- Assign locomotives and trainsets to trains.
- Plan driver duties for operating sessions.
- Validate schedules for conflicts and consistency.
- Display graphical timetables (time–distance diagrams).
- Generate printed output: train cards, station books, and driver duty sheets.

## Getting started

Use the **Settings** tab to configure your layout name, timing parameters,
and default values. Then define your operational places in the **Operation locations** tab, and start
adding trains in the **Trains** tab.

## About this project

This application is part of the Tellurian Trains suite of tools for model
railway operations. It is open source and available on GitHub.

## Technical stuff
The app is a *progressive web application* and can run offline. 
The app runs as native web assembly in the browser of choice.
When online, the app updates automatically to latest version.

Developed using:
- **.NET** and **Blazor**, an open souce web framework from Microsoft and the .NET Foundation.
- **Claude Code** from Antrophic.

