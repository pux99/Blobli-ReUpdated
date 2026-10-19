# Informe — Trabajo Práctico n°1

**Implementación sonora de una región de juego**
Materia: Sonido y Música para Videojuegos · UADE
Juego: **Blobli** · Motor: Unity · Middleware: Wwise 2023.1.19

> Completar carátula, comisión e integrantes.

---

## 1 · Descripción del proyecto

**El juego.** Blobli es un puzzle 2D por niveles sobre una grilla. El personaje es
vulnerable a la luz: avanza recolectando gemas, combinándolas para proyectar sombras
y usando esas sombras para cruzar las zonas expuestas. Cada nivel tiene dos *Key
gems* obligatorias que habilitan el Altar del final. **El entorno sólo avanza cuando
el jugador hace un movimiento**: el tiempo del juego es discreto, por turnos.

**La región implementada.** El claro: una explanada seca atravesada por hongos, bajo
un sol que avanza, con el Altar de piedra en un extremo. La idea que ordena todo el
diseño es que **la luz es la amenaza y la sombra es el refugio**, y eso se oye en
tres niveles:

1. **El paisaje** cambia con la exposición al sol: del void callado al chisporroteo
   del sol pleno, pasando por el viento.
2. **Las pisadas** cambian con la baldosa, para que el jugador reconozca por el oído
   sobre qué está parado.
3. **El tiempo se oye**: a medida que se gastan los turnos el mundo se vacía y el
   void se acerca.

**Fuera de alcance:** música interactiva, posicionamiento 3D y Attenuations (el juego
es 2D sobre grilla, con escucha fija) y generación de SoundBank.

---

## 2 · Mapa del proyecto

### Jerarquía Actor-Mixer

```
Player.wwu
└── AMX_Player                     → Bus_Player · UserAuxSend0 Aux_Altar −6 dB
    ├── SW_Steps_Tile              Switch · Tile · Default Ground · STEP · Volume −6 dB
    │   ├── RND_Step_Ground        Random Shuffle · Step · Volume ±1 dB
    │   ├── RND_Step_Mushroom      idem
    │   └── RND_Step_Shadow        idem
    └── [Actions]  (Virtual Folder)
        ├── RND_Gem_Pickup         Random Shuffle · Step · Limit rep. 2 · Pitch ±200 · Vol ±1.5
        ├── SFX_Shadow_Place
        ├── SFX_Key_Place
        └── SFX_Death

Ambience.wwu
└── AMX_Ambience                   → Bus_AMB · UserAuxSend0 Aux_Altar −10 dB
    └── SW_Zone_Ambience           Switch · Zone · Default Field · CONTINUOUS
        ├── BLD_Field              Blend · Continuous          [switch Field]
        │   ├── Blend Track "Sun" · SunExposure · Crossfade ON
        │   │   ├── SFX_Wind_Soft      zona  0 – 40    Loop Infinite
        │   │   ├── SFX_Wind_Med       zona 25 – 75    Loop Infinite
        │   │   └── BLD_Sun_High       zona 60 – 100   Blend agrupador
        │   │       ├── SFX_Wind_Hot       Loop Infinite
        │   │       └── SFX_Sun_Sizzle     Loop Infinite + 2 efectos (ver §9)
        │   └── SW_Fauna           Switch · Fauna · CONTINUOUS   (sin Blend Track)
        │       ├── Active → BLD_Fauna      Blend agrupador
        │       │     ├── RND_Insects       Continuous + Loop · Delay 7 s −3/+5
        │       │     └── RND_Mushrooms     Continuous + Loop · Delay 11 s −5/+7
        │       └── Silent → (vacío)
        ├── BLD_Altar              Blend agrupador · Continuous  [switch Altar]
        │   ├── SFX_Altar_Voices · SFX_Altar_Static · SFX_Altar_Aura   Loop Infinite
        └── SFX_Void               Loop Infinite · asignado a LOS DOS switches
                                   Continue to play across switches · fades 2 s

Interface.wwu
└── AMX_UI                         → Bus_UI  (sin State, sin send)
    ├── SFX_Turn_Pulse             RTPC Turns → Pitch y Volume
    ├── SFX_UI_Hover               misma source que Click · +250 cents · −7 dB
    ├── SFX_UI_Click
    └── SFX_Win_Level
```

### Master-Mixer

```
Master Audio Bus
├── Bus_SFX              State Recinto · Altar → Lowpass 10 · Volume −1
│   ├── Bus_Player
│   └── Bus_AMB
├── Bus_UI               sin State, sin send
└── Aux_Altar  [AuxBus]  Effect ShareSet Reverb_Altar (Wwise RoomVerb)
                         State Recinto · None −96 · Exterior −96 · Altar 0 dB
```

### Game Syncs

| Game Sync | Tipo | Rango / valores | Quién lo consume | Quién lo setea |
|---|---|---|---|---|
| `SunExposure` | Game Parameter | 0–100, default 0 | Blend Track `Sun`; Switch Group `Fauna` | Unity, según la luz sobre la celda |
| `AltarDistance` | Game Parameter | 0–100, default 100 | Switch Group `Zone` | Unity, distancia al Altar |
| `Turns` | Game Parameter | 0–100, default 0 | 7 curvas RTPC (ver §6) | Unity, turnos transcurridos |
| `Tile` | Switch Group | Ground · Mushroom · Shadow | `SW_Steps_Tile` | `Set Switch` antes de cada `Play_Footstep` |
| `Zone` | Switch Group | Altar · Field | `SW_Zone_Ambience` | **Use Game Parameter** → `AltarDistance` (0–35 / 35–100) |
| `Fauna` | Switch Group | Active · Silent | `SW_Fauna` | **Use Game Parameter** → `SunExposure` (0–65 / 65–100) |
| `Recinto` | State Group | None · Exterior · Altar | `Bus_SFX` y `Aux_Altar` | `Set State` al cruzar el umbral |

**Vale la pena subrayarlo:** un Game Parameter lo pueden consumir tres cosas — un
Switch Group con *Use Game Parameter*, un Blend Track, o una curva RTPC. **Este
proyecto usa las tres.** Cada decisión está resuelta con el mecanismo que le
corresponde en lugar de usar RTPC para todo.

### Events (13)

| Event | Acciones | Lo dispara Unity cuando… |
|---|---|---|
| `Play_Ambience` | Play `SW_Zone_Ambience` | se carga el nivel (una sola vez) |
| `Stop_Ambience` | Stop, Fade 3 s | se descarga el nivel |
| `Play_Footstep` | Play `SW_Steps_Tile` | Blobli entra a una celda |
| `Play_Turn` | Play `SFX_Turn_Pulse` | se consume un turno |
| `Play_Gem_Pickup` | Play `RND_Gem_Pickup` | se recoge una gema |
| `Play_Shadow_Place` | Play `SFX_Shadow_Place` | se proyecta una sombra |
| `Play_Key_Place` | Play `SFX_Key_Place` | se coloca una Key gem |
| `Play_Death` | Play `SFX_Death` + **Stop** ambiente (Delay 0.2 s, Fade 1.5 s) | Blobli queda expuesto |
| `Play_Win` | Play `SFX_Win_Level` + **Stop** ambiente (Delay 0.5 s, Fade 2.5 s) | se completa el nivel |
| `Play_UI_Hover` / `Play_UI_Click` | Play | interacción de menú |
| `Set_Recinto_Altar` / `Set_Recinto_Exterior` | Set State, Scope Global | se cruza el umbral de la cámara |

---

## 3 · Sonidos del personaje — footsteps

### El asset se arregla antes de entrar al motor

Este es el criterio que ordena toda la sección: **el middleware no corrige un mal
asset.** Si se parchea con filtros y recortes en Wwise, cada objeto necesita su
propia corrección y la configuración se vuelve frágil. Si se arregla en el archivo,
la configuración queda mínima y el asset sirve en cualquier contexto.

Medimos los nueve WAV originales y encontramos dos cosas:

- eran **mono duplicado en dos canales** (`CorrLR = 1.000`, energía lateral 0 %);
- entre el **61 % y el 91 % de la energía estaba por debajo de 120 Hz**.

Ese exceso de grave es lo que se percibía como retumbe: **los graves son lo que
decae más lento**, así que una pisada con la energía concentrada abajo suena a sala
aunque no tenga reverb.

Proceso aplicado: mono → quitar DC → **pasa-altos** → recorte donde la envolvente cae
por debajo de un umbral, con **fade-out** → fade-in de 0.3 ms → **normalización de los
nueve a la misma sonoridad percibida** (−25.75 LUFS).

**Los parámetros no son iguales para los tres grupos, y esa es una decisión
deliberada.** Un único corte para todos trataba a tres materiales distintos como si
fueran el mismo:

| Grupo | Pasa-altos | Recorte | Fade-out | Duración |
|---|---|---|---|---|
| Ground | **60 Hz, 1er orden (6 dB/oct)** | −24 dB | 6 ms | 271 → **~32 ms** |
| Mushroom | 150 Hz, 2º orden (12 dB/oct) | −32 dB | 25 ms | 527–569 → **~293 ms** |
| Shadow | 150 Hz, 2º orden (12 dB/oct) | −32 dB | 25 ms | 457–649 → **~235 ms** |

Ground arrancó con el mismo corte de 150 Hz y 2º orden que los otros dos, y sonaba
mal: una vibración grave al final, como si saturara. Midiendo se encontró el motivo, y
**no era la frecuencia de corte sino el orden del filtro**.

Un Butterworth de 2º orden tiene Q = 0.707: está levemente sub-amortiguado, así que su
respuesta al escalón **sobrepasa y oscila** antes de asentarse. Sobre una señal que
tenía el 70 % de su energía en graves, el filtro tiene que remover muchísimo y esa
oscilación queda a la vista: la cola del archivo procesado concentraba entre el 2 % y
el 3.6 % de la energía total alrededor de 127–151 Hz, es decir **cerca de la propia
frecuencia de corte**, contra 1.1–2 % sin filtrar. Además el pico del ataque se corría
de los primeros 2-4 ms a los 6-8 ms.

La solución fue cambiar el **orden**, no la frecuencia. Un filtro de primer orden tiene
un único polo real: su respuesta al impulso es una exponencial decreciente monótona y
**no puede oscilar**. Con 60 Hz a 6 dB/oct, Ground conserva su cuerpo, recupera el pico
en el primer milisegundo y pierde la vibración. Mushroom y Shadow siguen con el de 2º
orden porque son sonidos largos donde el asentamiento queda enterrado en el
decaimiento natural, y ahí el corte más firme sí hacía falta.

También se bajó el fade-in de 2 ms a 0.3 ms: en un sonido percusivo cuyo ataque vive
en los primeros milisegundos, un fade-in de 2 ms **atenúa justamente el transitorio**.
Con el recorte el archivo ya arranca 24 dB por debajo del pico, así que 0.3 ms alcanza
para garantizar el cruce por cero.

### Emparejar por sonoridad, no por RMS

Una vez procesados, los nueve quedaron al **mismo RMS exacto**… y las pisadas de
Ground seguían oyéndose más bajas que las otras dos. Midiendo con el estándar de
sonoridad **ITU-R BS.1770** (el de LUFS, el que usa broadcast) apareció el motivo:

| | RMS | **Sonoridad** |
|---|---|---|
| Ground | −18.64 dB | **−30.75 LUFS** |
| Mushroom | −18.64 dB | −22.31 LUFS |
| Shadow | −18.64 dB | −22.14 LUFS |

**8.5 dB de diferencia percibida con el mismo RMS.** Dos causas se suman:

1. **La duración.** El oído integra energía en una ventana de unos 200 ms. Ground
   dura 36 ms y Mushroom 293: a igual RMS, el corto entrega mucha menos energía
   dentro de esa ventana y por lo tanto se percibe más bajo.
2. **El contenido espectral.** Ground conserva 42–56 % de su energía por debajo de
   120 Hz, donde el oído es bastante menos sensible; Shadow sólo 16–33 %. El RMS
   trata todas las frecuencias por igual; el oído no.

Por eso los nueve se normalizaron a **−25.75 LUFS** en lugar de a un RMS común: es la
medida que corresponde cuando lo que se quiere emparejar es *lo que se escucha*.
Ground subió ~5 dB y los otros dos bajaron ~3.5 dB, con todos los picos por debajo de
−2.88 dBFS. El nivel general se recuperó subiendo `SW_Steps_Tile` de −6 a −2.5 dB.

### La configuración en Wwise

```
SW_Steps_Tile      Switch Container · Switch Group Tile · Default Ground
                   Play Mode STEP · Voice Volume −2.5 dB
RND_Step_*         Random SHUFFLE · Step · randomizer de Volume ±1 dB
```

Nada más: sin filtros, sin randomizer de Pitch, sin recortes. Los justificativos:

- **Play Mode Step** porque el ritmo lo marca el juego — un paso por movimiento en
  la grilla. En Continuous un solo Play caminaría para siempre.
- **Switch y no Random para la superficie**: la baldosa es una *condición del juego*,
  no azar. Unity hace `Set Switch Tile` antes de llamar al Event, así que el cambio
  ya se oye en la pisada siguiente y no hace falta Continuous.
- **Random Shuffle** recorre las tres variaciones antes de repetir ninguna. *"Limit
  repetition to"* queda en **1**: con sólo 3 hijos, subirlo a 2 dejaría un único
  candidato y la elección pasaría a ser determinista.
- **El randomizer de Volume es de ±1 dB** y no más, porque los nueve archivos ya
  están emparejados en sonoridad. Su función no es dar variación —eso lo hacen las
  tres muestras— sino evitar que dos pisadas consecutivas sean idénticas bit a bit.
- **No hay volumen base por superficie.** Antes había offsets de +2 / −1.5 dB para
  compensar diferencias entre grupos; al emparejar los archivos dejaron de hacer
  falta. Corregir en la fuente es preferible a compensar en el motor.

**Optimización:** fuera de su switch, el Random de esa superficie **no se instancia**.

---

## 4 · Ambiente

### Un solo Event

Todo el paisaje arranca con **`Play_Ambience`**, que Unity llama una vez al cargar el
nivel. De ahí en más el código no vuelve a decidir qué suena: la lógica vive en la
jerarquía y se reconfigura sola con los Game Syncs.

### `SW_Zone_Ambience` — las dos zonas

**Switch Container, Play Mode CONTINUOUS.** El Event se llama una sola vez y el
contenedor **queda escuchando** los cambios de `Zone`. Al acercarse al Altar cambia
de hijo solo, con Fade-in/Fade-out de 2 s. **En Step no pasaría nada hasta el próximo
Play**, y no hay próximo Play.

`Zone` se deriva de `AltarDistance` con **Use Game Parameter**, curva de escalones
0–35 `Altar` / 35–100 `Field`, con mesetas planas y **ningún tramo en `None`** (un
tramo en None sería silencio total).

**Optimización:** fuera de su switch un objeto no se instancia. En el campo las tres
capas del Altar no existen; dentro del Altar no existe ninguna capa de clima.

### El sonido compartido

`SFX_Void` está asignado a **los dos switches**, porque el void está siempre. Lleva
**Continue to play across switch transitions** para no reiniciarse al cruzar, más
fades de 2 s.

### Sistema dinámico de clima

`BLD_Field` es un **Blend Container en Continuous** con un **Blend Track** llamado
`Sun`, **Crossfade tildado** y `SunExposure` como Game Parameter.

| Capa | Zona | Contenido |
|---|---|---|
| `SFX_Wind_Soft` | 0 – 40 | viento quieto de la zona en sombra |
| `SFX_Wind_Med` | 25 – 75 | el sol empieza a dar sobre el claro |
| `BLD_Sun_High` | 60 – 100 | viento seco **+** el sol quemando |

- **Las zonas se solapan** a propósito: si sólo se tocaran en el borde, el Crossfade
  no tendría sobre qué cruzar y se oiría un escalón.
- **Cubren 0 a 100 completo**: un tramo sin capa sería un tramo mudo.
- **Blend y no Switch** porque la exposición al sol es una variable analógica y
  queremos cambio continuo.
- **El Loop va en el hijo**, porque un Blend no repite a sus hijos.
- **Optimización:** en Continuous el Blend no instancia los hijos fuera de zona. Con
  tres curvas RTPC de volumen las tres capas sonarían siempre, dos de ellas
  inaudibles, gastando voces — y se vería en el Profiler.

### La fauna: randomización del ambiente

`SW_Fauna` es un **Switch Container en Continuous** colgado de `BLD_Field` pero **no
asignado a ningún Blend Track**, así que vive en paralelo a las capas de viento. Su
Switch Group se deriva de `SunExposure`: **0–65 `Active`, 65–100 `Silent`**, y
`Silent` **está vacío** — con el sol alto los bichos se callan y sus contenedores no
se instancian. Idea de diseño y camino óptimo a la vez.

Adentro, dos sistemas intermitentes con cadencias distintas:

| | `RND_Insects` | `RND_Mushrooms` |
|---|---|---|
| Play Mode | Continuous + **Loop Infinite** | Continuous + **Loop Infinite** |
| Transition | **Delay** 7 s, randomizer −3/+5 | **Delay** 11 s, randomizer −5/+7 |
| Hijos | 2 one-shots (w 30/30) + Silence (w 40) | 2 one-shots (w 25/25) + Silence (w 50) |
| Randomizers | Pitch ±200 · Volume ±3 · LPF 0–30 | Pitch ±250 · Volume ±2.5 · LPF 0–25 |

- **El Loop va en el CONTENEDOR.** Un hijo que loopea no termina nunca y el
  contenedor jamás dispararía el siguiente.
- **Delay y no Trigger Rate**: el Delay cuenta desde que *termina* un elemento, así
  que dos bichos nunca se pisan.
- **El Delay va randomizado**: fijo sonaría a metrónomo.
- Los **objetos Silence** con Duration y Random Min/Max abren los huecos largos.
- **Random Standard con "Limit repetition" desactivado**, a propósito: con Shuffle o
  con el límite activo el silencio no podría salir dos veces seguidas y **los weights
  quedarían sin efecto**.

---

## 5 · Recinto adicional: el Altar

- **State Group `Recinto`** (`Exterior` / `Altar`), **Default Transition Time 0.8 s**.
  Lo setea Unity con `Set State`.
- **Auxiliary Bus `Aux_Altar`** con el ShareSet **`Reverb_Altar`** (Wwise RoomVerb:
  DecayTime 2.2 s, RoomSize 55, PreDelay 18 ms, ReverbLevel −3 dB, ERLevel −12 dB,
  **HFDamping 3**, tone control −2 dB desde 5.5 kHz, **DryLevel −96**).
- **Sends:** `AMX_Player` −6 dB y `AMX_Ambience` −10 dB, puestos una vez en el
  Actor-Mixer y no objeto por objeto.
- **Qué hace el State:** sobre `Aux_Altar`, `None` y `Exterior` → −96 dB y `Altar` →
  0 dB; sobre `Bus_SFX`, `Altar` → Lowpass 10 y −1 dB.

### Justificaciones

**Por qué un State y no un Switch.** El State es un **offset global** que modifica
propiedades de lo que **ya está sonando**: no crea ni elimina instancias. Al entrar a
la cámara no queremos re-disparar el ambiente, queremos que el mismo ambiente suene
distinto. El Switch, en cambio, es por Game Object y **elige qué objeto suena**. Por
eso `Zone` decide *qué* se oye y `Recinto` decide *cómo suena el espacio*: son dos
preguntas distintas y por eso hay dos mecanismos.

**Por qué la reverb va en un Auxiliary Bus.** Una sola instancia del efecto procesa a
todos los emisores y el retorno se controla en un solo lugar. Por eso el ShareSet
tiene `DryLevel −96`: el camino seco ya va por `Bus_SFX`.

**Por qué el State mueve el volumen del Aux y no el send de cada objeto.** Un único
punto de control, y la reverb **nunca se re-instancia**: la misma instancia sube y
baja con el Default Transition Time.

**Por qué además hay un Low-pass.** Las paredes de piedra bloquean los agudos y dejan
pasar los graves. Con reverb sola el efecto suena a "le puse reverb"; con reverb
**más** Low-pass suena a estar adentro.

**Por qué el HF Damping es moderado y no alto.** Es el ajuste que más costó acertar.
Arrancó en 7.5 (de un rango 0.5–10) buscando una sala de piedra oscura, pero desde que
las pisadas pasaron por un pasa-altos su energía vive entre 150 Hz y 5 kHz — **justo la
banda que una cola tan amortiguada se come**. El resultado era que entrar al Altar casi
no se notaba. Bajándolo a 3, y suavizando el tone control de −7 a −2 dB, la cola
conserva la banda donde hay señal y el cambio de recinto se percibe. La lección: el
ajuste de una reverb no es independiente del contenido espectral de lo que le entra.

**Por qué `None` también lleva −96 dB.** Si Unity nunca llamara a `Set State`, el
State activo sería `None`; sin ese offset habría reverb de cámara en campo abierto.

**Por qué `Bus_UI` queda afuera.** Cuelga directo del Master, sin State ni send: el
hover, el click y el jingle **no ocurren adentro del mundo**. Si se oscurecieran al
entrar al Altar, el jugador perdería feedback en el momento más importante.

---

## 6 · Variable de gameplay: `Turns`

En Blobli **el entorno sólo avanza cuando el jugador se mueve**. El tiempo no corre:
se gasta. Por eso la variable es `Turns` y no un reloj en segundos.

El sistema está construido **por sustracción**: a medida que se agotan los turnos el
mundo se vacía mientras el void se acerca. Siete curvas RTPC, todas sobre `Turns`:

| Objeto | Propiedad | Turns 0 | Turns 50 | Turns 100 |
|---|---|---|---|---|
| `SFX_Void` | Voice Volume | −10 dB | — | **−4 dB** |
| `SFX_Void` | Low-pass | 45 | 30 | **0** |
| `BLD_Fauna` | Voice Volume | 0 dB | −2 dB | **−10 dB** |
| `RND_Insects` | Delay (offset) | +0 s | +2 s | **+8 s** |
| `RND_Mushrooms` | Delay (offset) | +0 s | +2.5 s | **+10 s** |
| `SFX_Turn_Pulse` | Voice Pitch | 0 cents | +120 (en 60) | **+500 cents** |
| `SFX_Turn_Pulse` | Voice Volume | −9 dB | — | **0 dB** |

### Justificaciones

- **Alargar el `Delay` con una curva RTPC** es la técnica de *Trigger Rate con RTPC*:
  se cambia **cuándo vuelve** el sonido sin tocar el sonido. Acá está usada al revés
  de lo obvio — más lento es más vacío, y más vacío es más tenso.
- **Las curvas son no lineales a propósito**: casi no se mueven en la primera mitad
  del nivel y se disparan en la segunda, para que el juego no apure desde el turno 1.
- **El void sube de nivel y se le abre el Low-pass a la vez**: pasa de lejano y sordo
  a presente y abierto. Dos ejes sobre el mismo objeto se leen como acercamiento.
- **La tensión no agrega voces.** Todo se hace modulando lo que ya suena: no hay un
  solo objeto nuevo instanciándose.
- **Cada curva está en un objeto distinto**: ninguna se suma con otra. Dos curvas
  iguales en padre e hijo se sumarían.
- **Sobre `SFX_Turn_Pulse`:** el Voice Pitch de Wwise es *velocidad de reproducción*,
  o sea que cambia altura **y duración**. Sobre el final del nivel el tic también se
  acorta, lo que refuerza el apuro sin tocar el asset.

---

## 7 · Mezcla

```
Master Audio Bus
├── Bus_SFX        lo diegético (recibe el Low-pass del State)
│   ├── Bus_Player
│   └── Bus_AMB
├── Bus_UI         lo no diegético (no recibe nada)
└── Aux_Altar      el camino húmedo
```

- `Bus_Player` y `Bus_AMB` separados para balancear personaje contra paisaje con un
  solo fader.
- `Bus_UI` fuera de `Bus_SFX` por la razón de §5.
- Criterio general: **se baja el que enmascara, no se sube el enmascarado**, porque
  subir consume headroom y aplana la dinámica.

---

## 8 · Soundcaster: recorrido de prueba

Sesión **`SC_Blobli_TP1`**: los 13 Events como módulos, sliders de `SunExposure`,
`AltarDistance` y `Turns`, selector de Switch `Tile` y selector de State `Recinto`.

`Zone` y `Fauna` no se tocan a mano: se derivan solos. Que el switch activo cambie al
mover el slider es, en sí, la demostración de que *Use Game Parameter* funciona.

1. **`Play_Ambience`.** Arranca en `Field` con `SunExposure 0`: void, viento quieto y
   bichos.
2. **Barrer `SunExposure` de 0 a 100 despacio.** Se cruzan las tres capas de viento
   sin escalones, entra el sol y, al pasar 65, **la fauna desaparece**. Verificar que
   en 100 sigue sonando algo.
3. **Volver `SunExposure` a 20.** La fauna vuelve con un fade de 3 s.
4. **Bajar `AltarDistance` por debajo de 35.** Cruza al Altar con fade de 2 s, y **el
   void no se reinicia**.
5. **`Set_Recinto_Altar`.** Entra la reverb y se oscurece lo diegético.
6. **`Play_UI_Click`** estando en el Altar: suena limpio, sin reverb ni Low-pass. Es
   la demostración de por qué `Bus_UI` está afuera de `Bus_SFX`.
7. **`Set_Recinto_Exterior`** y `AltarDistance` a 100.
8. **`Play_Footstep`** repetido cambiando `Tile`: tres timbres distintos y, dentro de
   cada uno, variación sin repetición obvia.
9. **Subir `Turns` de 0 a 100 despacio** con el ambiente sonando. El void se abre y
   sube; la fauna baja de nivel y se espacia hasta casi desaparecer. **Este es el
   paso que demuestra la variable de gameplay.**
10. **`Play_Death`**: suena la muerte y el ambiente se retira con fade. Demuestra un
    Event de dos acciones ordenadas con Delay.

Conviene dejar **capturas del Profiler (F6)** en los pasos 2 y 9: en el 2 se ve que
sobre `SunExposure` 65 las voces de la fauna no existen, y en el 9 se ve `Turns`
moviéndose en tiempo real. Son la prueba visual de los dos argumentos: optimización y
audio adaptativo.

---

## 9 · Problemas abiertos

**① Los dos efectos de `SFX_Sun_Sizzle`.** Ese objeto tiene un **Wwise Pitch
Shifter** y un **Wwise Time Stretch** como inserts en tiempo real. Son los dos
plugins más caros de Wwise (procesamiento espectral) corriendo sobre una cama que
loopea sin parar, y **ningún RTPC los modula**: sus parámetros son fijos.

La corrección es tildar **Render** en los dos effect slots: Wwise hornea el efecto en
la conversión, suena exactamente igual y pasa a costar **cero CPU** en runtime. Está
disponible justamente porque no hay RTPC sobre ellos. Como mejora adicional, `SunShift`
es un ShareSet de fábrica referenciado desde `Factory Effects.wwu` y convendría
duplicarlo al Work Unit de Effects del proyecto. Queda pendiente de confirmar con el
autor del efecto.

**② Faltan 10 sources** (acciones del jugador e interfaz). `SFX_Turn_Pulse` en
particular tiene sus dos curvas RTPC bien configuradas pero sin audio.

**③ Seis WAV quedaron en `Originals/SFX/` en vez de `Originals/SFX/Blobli/`.**
Funcionan, pero rompen la convención de carpeta del resto del set.

**④ Con más tiempo:** un `SEQ_` (Sequence Container) para la apertura del Altar
cuando entran las dos Key gems, una acción **Seek** con punto de inicio random si
alguna cama llegara a instanciarse más de una vez, y la generación del SoundBank.