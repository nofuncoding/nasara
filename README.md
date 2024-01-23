# Nasara Godot Manager

<picture>
 <source media="(prefers-color-scheme: dark)" srcset="/doc/readme-header-dark.png">
 <source media="(prefers-color-scheme: light)" srcset="/doc/readme-header-light.png">
 <img alt="header" src="readme-header-light.png">
</picture>

A Simple Godot Versions Manager, and it's also made with Godot!

> [!CAUTION]
> Nasara is still in the early stages of development. Important features are missing.
> On your own head be it!

## Features

- Manage Godot Installations
  - Install Latest Release
  - Add Exists Godot (WIP)
  - Remove / Update Godot Version (WIP)
- Manage Your Projects between different Godot versions (WIP)

And More!

## Development

This application needs Godot 4.2 above and Mono support

The application's architecture is like this:

- `App` (Base of the whole application)
  - `ViewSwitch` (Switching Views)
    - EditorView      -> View
      - EditorList    -> Component
    - ProjectView     -> View
    - ...
  - `NavBar` (control `ViewSwitch`)
    - Auto-generated `NavButton`

If you got any problems, please write an issue.

## License

Nasara is licensed under the MIT License.

© 2023-2024 NoFun & Contributors