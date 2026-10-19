# Blobli — Wwise · Léeme primero

Proyecto de Wwise 2023.1.19 (`Blobli.wproj`) con la implementación sonora de una
región de **Blobli**, armado contra la consigna del TP1.

Los 43 `.wwu` están validados contra el XSD oficial de Wwise
(`ObjectDataSchema.119.xsd`), sin referencias rotas ni GUIDs duplicados.

---

## 1 · Estado actual

**31 de 32 sources resueltas.** Zeki subió los diez sonidos que faltaban. Queda uno:

| Falta | Objeto que lo espera |
|---|---|
| `Blobli\UI_Hover.wav` | `SFX_UI_Hover` |

Va en `Originals\SFX\Blobli\` con **exactamente** ese nombre. Hasta que esté, ese
único objeto queda mudo; todo lo demás suena.

> Cuando aparezca hay que **medirle el nivel** y ponerle su `Voice Volume`, como se
> hizo con los otros nueve: el objetivo es dejarlo cerca de −24 LUFS máximo, igual
> que `SFX_UI_Click`.

### Nivelación de los one-shots

Los diez archivos nuevos llegaron con un rango de **17 dB** entre el más bajo y el
más alto. Se nivelaron con `Voice Volume` en cada Sound —sin tocar los WAV— midiendo
**LUFS momentáneo máximo** en ventana de 400 ms, que es la métrica que corresponde a
un sonido corto (la integrada subestima los one-shots):

| Objeto | Medido | Volume | Resultado |
|---|---|---|---|
| `SFX_Key_Place` | −9.0 | −13.0 | −22.0 |
| `SFX_Gem_Pickup_02` | −12.0 | −9.0 | −21.0 |
| `SFX_Shadow_Place` | −12.1 | −10.0 | −22.1 |
| `SFX_Gem_Pickup_04` | −12.2 | −8.8 | −21.0 |
| `SFX_Gem_Pickup_03` | −14.5 | −6.5 | −21.0 |
| `SFX_Gem_Pickup_01` | −17.0 | −4.0 | −21.0 |
| `SFX_Win_Level` | −19.1 | −1.0 | −20.1 |
| `SFX_UI_Click` | −19.3 | −4.5 | −23.8 |
| `SFX_Turn_Pulse` | −24.6 | −1.5 | −26.1 |
| `SFX_Death` | −26.0 | — | −26.0 |

Las pisadas quedan en −28.3 efectivo: son el piso de la mezcla, porque suenan todo
el tiempo. Los cuatro `GEM_Pickup` quedan **todos en −21.0**: dentro de un Random
Container las variaciones tienen que estar al mismo nivel, si no la randomización
se percibe como un error de volumen y no como variedad.

**Sólo se atenúa, nunca se levanta.** Así ninguna voz puede clipear.

### Colas muertas recortadas

Cuatro archivos traían silencio largo al final. Se recortó con `TrimEnd` +
`Fade-out` en el Source Editor, que no modifica el archivo original:

| Objeto | Audio real | Archivo | TrimEnd |
|---|---|---|---|
| `SFX_UI_Click` | 0.65 s | 4.25 s | 0.75 s |
| `SFX_Turn_Pulse` | 0.46 s | 1.10 s | 0.55 s |
| `SFX_Key_Place` | 2.06 s | 5.03 s | 2.20 s |
| `SFX_Win_Level` | 2.27 s | 4.00 s | 2.40 s |

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
| Ground | **60 Hz, 1er orden** | −45 dB | 6 ms | ~57 ms |
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