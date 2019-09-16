# Unitas
A state-of-the art bypass for TrinitySeal

## Usage

`./Unitas.exe <path to assembly> [path to json file with server variables]`

## Info

- Anti-tamper needs to be removed first (dnlib won't keep the encrypted method bodies, nothing much I can do)
- Doesn't work with native packers (kind of obvious)

## Gettings server variables

- In order to do this, you need to have valid credentials
- Drag the assembly on Dumper.exe
- Login with your credentials
- The dumper will automatically close if it gets the variables, they will be saved to `trinityvars.json`

## Credits

- 0xd4d: dnlib
- Washi: for his great .NET knowledge
- Harmony: for being a sick library
- JetBrains: for creating Rider, my IDE of choice