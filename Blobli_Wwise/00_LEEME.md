# Blobli — Wwise · Léeme primero

Proyecto de Wwise 2023.1.19 (`Blobli.wproj`) con la implementación sonora de una
región de **Blobli**, armado contra la consigna del TP1.

Los 43 `.wwu` están validados contra el XSD oficial de Wwise
(`ObjectDataSchema.119.xsd`), sin referencias rotas ni GUIDs duplicados.

---

## 1 · Estado actual

**21 de 31 sources resueltas.** Lo que falta son los sonidos de acciones del jugador
y de interfaz:

| Falta | Objeto que lo espera |
|---|---|
| `Blobli\GP_Turn.wav` | `SFX_Turn_Pulse` ← **el más urgente** |
| `Blobli\GEM_Pickup_01..04.wav` | `RND_Gem_Pickup` |
| `Blobli\ACT_ShadowPlace.wav` | `SFX_Shadow_Place` |
| `Blobli\ACT_KeyPlace.wav` | `SFX_Key_Place` |
| `Blobli\ACT_Death.wav` | `SFX_Death` |
| `Blobli\UI_Click.wav` | `SFX_UI_Click` |
| `Blobli\UI_Hover.wav` | `SFX_UI_Hover` |
| `Blobli\WIN_Level.wav` | `SFX_Win_Level` |

Van en `Originals\SFX\Blobli\` con **exactamente** esos nombres. Al ponerlos, Wwise
los levanta solo: no hay que tocar nada en la jerarquía.

> `GP_Turn.wav` es prioritario porque `SFX_Turn_Pulse` tiene dos curvas RTPC
> configuradas y funcionando, pero **hoy están mudas** porque el objeto no tiene
> audio.

Las camas del ambiente y las nueve pisadas ya están puestas y funcionando.

## 2 · Las pisadas están procesadas, no crudas

Los nueve WAV de pisadas se limpiaron antes de entrar al proyecto. Esto importa
saberlo porque **la configuración en Wwise es deliberadamente mínima**: el trabajo
está hecho en el archivo, no en el motor.

| Paso | Por qué |
|---|---|
| **Mono** | los originales eran dos canales idénticos (`CorrLR = 1.000`): ocupaban el doble sin aportar nada |
| **Quitar DC offset** | higiene básica |
| **Pasa-altos, con corte distinto por grupo** | tenían entre 61 % y 91 % de la energía bajo 120 Hz; ese exceso de grave es lo que se escuchaba como retumbe |
| **Recorte + fade-out** | saca la cola sin que el corte se note |
| **Fade-in de 0.3 ms** | garantiza arranque en cruce por cero, sin click, y sin tocar el ataque |
| **Normalizar los 9 a la misma SONORIDAD** | −25.75 LUFS exacto (ITU-R BS.1770), no al mismo RMS |

Los ajustes no son iguales para los tres, porque los materiales no lo son:

| Grupo | Pasa-altos | Recorte | Fade-out | Resultado |
|---|---|---|---|---|
| Ground | **60 Hz, 1er orden** | −24 dB | 6 ms | ~32 ms |
| Mushroom | 150 Hz, 2º orden | −32 dB | 25 ms | ~293 ms |
| Shadow | 150 Hz, 2º orden | −32 dB | 25 ms | ~235 ms |

Ground usa un filtro de **primer orden** y no de segundo, y la razón es física: un
filtro de 2º orden Butterworth tiene Q = 0.707, o sea que está levemente
sub-amortiguado y **oscila al asentarse**. Sobre un sonido percusivo corto y grave como
el de tierra, esa oscilación se escucha como una vibración al final. Un filtro de
primer orden tiene un solo polo real: su respuesta es una exponencial monótona y **no
puede oscilar**. Mushroom y Shadow conservan el de 2º orden porque son sonidos largos
donde el asentamiento queda enterrado en el decaimiento natural.

**Los WAV originales sin procesar están guardados aparte.** Si hay que rehacer el
proceso, pedilos antes de tocar los actuales.

## 3 · Cómo abrir y probar

1. Abrí `Blobli.wproj` y guardá (Ctrl+S) la primera vez.
2. Abrí la sesión **`SC_Blobli_TP1`** del Soundcaster.
3. Seguí el recorrido de prueba de `01_INFORME.md`, sección 8.

**Regla importante para trabajar en equipo:** si alguien va a editar los `.wwu`
desde afuera de Wwise, **Wwise tiene que estar cerrado**. Si está abierto, al
guardar pisa los archivos con su copia en memoria y revierte los cambios en
silencio.

## 4 · Pendientes conocidos

| | Qué |
|---|---|
| ① | Los 10 WAV que faltan |
| ② | Tildar **Render** en los dos effect slots de `SFX_Sun_Sizzle` (ver `01_INFORME.md`, sección 9) |
| ③ | Mover a `Originals\SFX\Blobli\` los 6 WAV que quedaron sueltos en `Originals\SFX\` — cosmético |
| ④ | Generar un SoundBank para integrar con Unity — fuera del alcance del TP |

## 5 · Qué ignora git

El `.gitignore` de la raíz excluye los archivos que Wwise regenera por usuario,
porque si se versionan generan conflictos de merge en cada apertura:

```
*.validationcache
*.wsettings
Blobli_Wwise/.cache/
Blobli_Wwise/GeneratedSoundBanks/
Blobli_Wwise/.wwiseLogs/
Blobli_Wwise/Originals.txt
```

Los `.wwu` y los `Originals/` **sí** se versionan: son el proyecto.