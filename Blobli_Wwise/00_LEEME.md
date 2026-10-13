# Blobli — TP1 · Léeme primero

Proyecto de Wwise 2023.1.19 (`Blobli.wproj`) con la implementación sonora de una
región de *Blobli*, armado contra la consigna del TP1 y contra la lista de sonidos
del equipo.

Los 43 `.wwu` están validados contra el XSD oficial de Wwise
(`ObjectDataSchema.119.xsd`), sin referencias rotas ni GUIDs duplicados. **Lo único
que falta son los audios.**

---

## 1 · Qué hacer para que suene

### Paso 1 — Poner los WAV

Los 31 archivos van en:

```
Blobli_TP1\Originals\SFX\Blobli\
```

con **exactamente** estos nombres. La carpeta ya está creada y vacía.

| Archivo | Qué es | ¿Loopea? | Objeto en Wwise |
|---|---|---|---|
| `STEP_Ground_01.wav` … `_03` | pisada en piso normal | no | `RND_Step_Ground` |
| `STEP_Mushroom_01.wav` … `_03` | pisada en hongo | no | `RND_Step_Mushroom` |
| `STEP_Shadow_01.wav` … `_03` | pisada en sombra | no | `RND_Step_Shadow` |
| `AMB_Void.wav` | el drone del void | **sí** | `SFX_Void` |
| `AMB_Wind_Soft.wav` | viento quieto (sombra) | **sí** | `SFX_Wind_Soft` |
| `AMB_Wind_Med.wav` | viento medio | **sí** | `SFX_Wind_Med` |
| `AMB_Wind_Hot.wav` | viento seco (sol pleno) | **sí** | `SFX_Wind_Hot` |
| `AMB_Insects_01.wav`, `_02` | bichos, one-shots cortos | no | `RND_Insects` |
| `AMB_Mushrooms_01.wav`, `_02` | hongos, one-shots cortos | no | `RND_Mushrooms` |
| `AMB_SunSizzle.wav` | el sol quemando | **sí** | `SFX_Sun_Sizzle` |
| `ALTAR_Voices.wav` | coro / voces del altar | **sí** | `SFX_Altar_Voices` |
| `ALTAR_Static.wav` | estática | **sí** | `SFX_Altar_Static` |
| `ALTAR_Aura.wav` | aura | **sí** | `SFX_Altar_Aura` |
| `GP_Turn.wav` | pulso de turno, muy corto | no | `SFX_Turn_Pulse` |
| `GEM_Pickup_01.wav` … `_04` | agarrar gema | no | `RND_Gem_Pickup` |
| `ACT_ShadowPlace.wav` | proyectar sombra | no | `SFX_Shadow_Place` |
| `ACT_KeyPlace.wav` | poner key gem | no | `SFX_Key_Place` |
| `ACT_Death.wav` | morir por la luz | no | `SFX_Death` |
| `UI_Click.wav` | click de menú | no | `SFX_UI_Click` **y** `SFX_UI_Hover` |
| `WIN_Level.wav` | nivel completado | no | `SFX_Win_Level` |

**Los 7 "¿Loopea? sí" son los únicos que tienen que ser material loopeable**: algo
estadísticamente estable, sin un evento reconocible adentro. Si un asset tiene un
golpe o una figura identificable, el oído la va a oír volver en cada vuelta y el
loop se delata. Los demás son one-shots.

`UI_Click.wav` lo usan dos objetos distintos: `SFX_UI_Click` lo reproduce tal cual y
`SFX_UI_Hover` lo reproduce con +250 cents y −7 dB. Son dos Sound SFX sobre una
sola source, así que no se duplica nada en disco.

> Si vas a importar desde Wwise en vez de copiar los archivos a mano, importá a la
> carpeta `Blobli` y respetá los nombres: Wwise va a reemplazar la source del objeto
> correspondiente automáticamente.

### Paso 2 — Abrir y guardar

Abrí `Blobli.wproj` y guardá enseguida (Ctrl+S). Ese primer guardado es el que hace
que Wwise asigne los ShortID y normalice el XML. Mirá el Log por si aparece algún
warning.

### Paso 3 — Hacer los loops a mano

Esto no se puede dejar hecho desde afuera: hay que mirar la waveform. Para cada uno
de los 7 assets que loopean, en el **Source Editor** (doble click en la fila del
archivo en el Contents Editor, no en el objeto):

1. `Loop → Infinite` ya está tildado en **General Settings** de cada objeto.
2. Tildar **Override wav loop points**.
3. Poner **Loop Start / Loop End** en una zona de amplitud pareja, sin eventos
   reconocibles adentro y **con margen a los dos lados**.
4. **Crossfade 500–1000 ms, Shape Sine.**
5. Los **Fade-in / Fade-out tienen que quedar FUERA de la región del loop**. Si un
   fade cae adentro, el ambiente "late": baja a silencio y vuelve en cada vuelta.

Si te aparece un triángulo de advertencia en el crossfade, es que no hay suficiente
material fuera de los puntos: corré los marcadores hacia adentro.

### Paso 4 — Tunear a oído en el Soundcaster

Abrí la sesión `SC_Blobli_TP1` y seguí el recorrido de prueba que está en
`01_INFORME.md`, sección 8. Lo que seguro vas a querer ajustar:

- los tiempos de **Delay** de `RND_Insects` y `RND_Mushrooms`;
- los **weights** de los objetos Silence (son los que deciden qué tan poblado suena
  el claro);
- los niveles de los **User Aux Send** a `Aux_Altar` (hoy −6 dB el jugador, −9 dB el
  ambiente);
- el `ReverbLevel` y el `DecayTime` del ShareSet `Reverb_Altar`;
- el balance entre `Bus_Player`, `Bus_AMB` y `Bus_UI` desde el Mixing Desk.

### Paso 5 — Entregar

Zipear la **carpeta entera**, incluida `Originals\`. La entrega anterior no la tenía
y por eso el proyecto abría sin audio.

---

## 2 · Checklist contra la consigna

| Consigna | Dónde está | Estado |
|---|---|---|
| **1** · Footsteps en distintas superficies | `SW_Steps_Tile` con `RND_Step_Ground` / `_Mushroom` / `_Shadow` | listo, faltan los WAV |
| **1** · Evitar la reiteración | Random **Shuffle** + randomizers de Pitch y Volume en cada contenedor; además `RND_Gem_Pickup` con 4 variaciones | listo |
| **2** · Bioma / entorno | el claro del void: `BLD_Field` | listo |
| **2** · Elementos del paisaje (viento, agua, fauna, máquinas) | viento ×3, insectos, hongos, sol, void, altar | listo |
| **2** · Sistema dinámico de clima | Blend Track `Sun` sobre `SunExposure`, 3 zonas solapadas 0–40 / 25–75 / 60–100 con Crossfade | listo |
| **2** · Randomización del ambiente | `RND_Insects` y `RND_Mushrooms`: Continuous + Loop + Delay randomizado + Silence + weights + randomizers de Pitch/Volume/LPF | listo |
| **3** · Recinto con States + reverb | State Group `Recinto`, Aux Bus `Aux_Altar` con ShareSet `Reverb_Altar`, Low-pass en `Bus_SFX` | listo |
| **4** · Variable de gameplay | `Turns` → dos curvas RTPC en `SFX_Turn_Pulse` (Pitch y Volume) y una en `SFX_Void` (Volume) | listo |
| **5** · Soundcaster | sesión `SC_Blobli_TP1` con los 13 Events, 3 sliders, selector de Switch y de State | listo |
| **Entrega** · proyecto completo en zip con los assets | falta meter los WAV en `Originals\SFX\Blobli\` | pendiente |
| **Entrega** · documentación | `01_INFORME.md` | borrador casi completo |

---

## 3 · Lo que saqué respecto de lo que había

- **Se fue el heartbeat.** No hay sistema de salud en Blobli: la luz mata de una. El
  componente "variable de gameplay" ahora lo resuelve `Turns`, que es la variable
  real del juego (el entorno sólo avanza cuando el jugador se mueve).
- **Se fue el Switch Group `Health`** y el Game Parameter asociado.
- **Se fue la lluvia y los truenos** que había puesto en el intento anterior: eran de
  los placeholders que estaban en `Originals.txt`, no de este juego.
- **Se fue el RTPC a `MidiVelocityOffset`** que tenía el proyecto original: esa
  propiedad sólo afecta a objetos disparados por MIDI, para SFX disparados por Events
  no hace nada.
- **Se fue el RTPC de Voice Volume a −200 dB** sobre el contenedor de clima: se
  peleaba con el Blend Track sobre el mismo parámetro, y además es el anti-patrón que
  el profe penaliza (deja la instancia viva gastando una voz).
- **Se fue el objeto Silence que resolvía un umbral** en el viejo `Sun_Climate`: en
  Continuous un Blend directamente no instancia a los hijos fuera de zona.

El detalle de por qué, con los nombres de las propiedades, está en `02_GUIA.md`.
