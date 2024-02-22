# Nasara

Download, Install, and Launch Godot Editors/Projects from one place

> [!CAUTION]
> Nasara is still in the early stages of development. Important features are missing.
> On your own head be it!

## Features

- Manage Godot Installations
  - Install Latest Releases
  - Add Existing Editors
  - Update Godot Version (WIP)
- Manage Your Projects between different Godot versions (WIP)

And More!

## FAQ

Q: Why is this project called Nasara?

A: I don't know. It just born out of my head when I am working on it.

Q: What's different from other projects like ..?

A: Nasara is designed to be like *Unity Hub*. I recommend you to try [Godots](https://github.com/MakovWait/godots) if you don't like Nasara's style.

## Development

This application needs Godot 4.3 above and Mono support

The application's architecture is like this:

- `App` (Base of the whole application)
  - `ViewSwitch` (Switching Views)
    - EditorView      -> View
      - EditorList    -> Component
    - ProjectView     -> View
    - ...
  - `NavBar` (controls `ViewSwitch`)
    - Auto-generated `NavButton`

If you got any problems, please write an issue.

## License

Nasara is licensed under the MIT License.

© 2023-2024 NoFun & Contributors

---

if you like it, please consider [supporting me](https://github.com/nofuncoding#support-me).