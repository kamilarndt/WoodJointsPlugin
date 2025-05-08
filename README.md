# WoodJointsPlugin

Domyślny plugin RhinoCommon dla Rhino 8 (Windows).

## Wymagania
- Rhino 8 (Windows)
- Visual Studio 2022
- .NET 7.0 (net7.0-windows)
- RhinoCommon.dll (zainstalowane Rhino)

## Instalacja i uruchomienie
1. Otwórz `WoodJointsPlugin.sln` w Visual Studio.
2. Skonfiguruj debugowanie:  
   `Debug > Start external program` → wskaż `Rhino.exe` (np. `C:\Program Files\Rhino 8\System\Rhino.exe`)
3. Zbuduj projekt (`Ctrl+Shift+B`).
4. Załaduj plugin w Rhino:  
   `_PluginManager` → `Install` → wybierz wygenerowany plik `.rhp` z folderu `bin\x64\Debug\net7.0-windows\`
5. Uruchom komendę:  
   `_HelloCommand`

## Struktura katalogów
- `Commands/` – komendy Rhino
- `Properties/` – meta-informacje o pluginie
- `Resources/` – ikony, pliki .rui
- `EmbeddedResources/` – ikony .ico
- `UI/` – interfejs użytkownika 