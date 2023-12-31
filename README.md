# weave 🧵🕹️ (Game) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=weave-game_weave&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=weave-game_weave)

<div align="center">
  <img width="300" src="readme/logo.png">
</div>

- [weave 🧵🕹️ (Game) ](#weave-️-game-)
  - [About](#about)
    - [Gameplay](#gameplay)
  - [Development](#development)
    - [Setup Scoreboard (Environment Variables)](#setup-scoreboard-environment-variables)
    - [Formatting (CSharpier)](#formatting-csharpier)
      - [Plugins](#plugins)

## About

Steer your line without crashing into each other, work together to get the highest score.

A game made in the Game Development Project course at Chalmers/GU.

<center>
  <table>
    <tr>
      <td>
        <img width="300" src="readme/start-screen.png">
      </td>
      <td>
        <img width="300" src="readme/in-game.png">
      </td>
    </tr>
    <tr>
      <td>Start screen</td>
      <td>Gameplay</td>
    </tr>
  </table>
</center>

### Gameplay

[![Video](https://img.youtube.com/vi/Fw0T2zQHsvo/maxresdefault.jpg)](https://youtu.be/Fw0T2zQHsvo?si=y7i0zsi_a19gQXTo)

## Development

### Setup Scoreboard (Environment Variables)

Run the following commands in the `weave` directory:

```bash
dotnet user-secrets set ConnectionString <mongo-db-dbconnection-string>
```

### Formatting (CSharpier)

Install: `dotnet tool install csharpier -g`

Run: `dotnet-csharpier .` inside the `weave` directory.

#### Plugins

- Install [Roslynator](https://marketplace.visualstudio.com/items?itemName=josefpihrt-vscode.roslynator) plugin for VSCode (or your preferred IDE).
- Install [EditorConfig](https://marketplace.visualstudio.com/items?itemName=EditorConfig.EditorConfig) plugin for VSCode (or your preferred IDE).
