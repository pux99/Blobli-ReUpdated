# Informe — Trabajo Práctico n°1

**Implementación sonora de una región de juego**
Materia: Sonido y Música para Videojuegos · UADE
Juego: **Blobli** · Motor: Unity · Middleware: Wwise 2023.1.19

> Borrador para completar. Está escrito con las justificaciones que faltan en un
> informe que sólo describe. Completá la carátula, los nombres del grupo y la
> sección 9 con lo que efectivamente hayan grabado o conseguido.

---

## 1 · Descripción del proyecto

**El juego.** Blobli es un puzzle 2D por niveles sobre una grilla. El personaje es
vulnerable a la luz: avanza recolectando gemas, combinándolas para proyectar sombras
y usando esas sombras para cruzar las zonas expuestas. En cada nivel hay dos *Key
gems* obligatorias que habilitan el Altar del final. **El entorno sólo avanza cuando
el jugador hace un movimiento**: el tiempo del juego es discreto, por turnos.

**La región implementada.** El claro: una explanada seca atravesada por hongos,
bajo un sol que avanza, con el Altar de piedra en un extremo. La idea que ordena
todo el diseño sonoro es que **la luz es la amenaza y la sombra es el refugio**, y
eso se oye en tres niveles a la vez:

1. **El paisaje** cambia con la exposición al sol: del void callado al chisporroteo
   del sol pleno, pasando por el viento.
2. **Las pisadas** cambian con la baldosa, y la baldosa de sombra suena más apagada
   y más lejana, así el jugador aprende a reconocer por el oído si está a salvo.
3. **El tiempo** se oye: cada movimiento deja un pulso que se vuelve más agudo y más
   presente a medida que se agota el nivel.

**Lo que no está implementado y por qué.** No hay música interactiva (fuera del
alcance del TP), no hay posicionamiento 3D ni Attenuations (el juego es 2D sobre una
grilla y la escucha es fija), y no hay SoundBank generado (el TP se evalúa en el
Soundcaster).

---

## 2 · Mapa del proyecto

### Jerarquía Actor-Mixer

```
Player.wwu
└── AMX_Player                        → Bus_Player · UserAuxSend0 Aux_Altar −6 dB
    ├── SW_Steps_Tile                 Switch Container · Tile · Default Ground · STEP
    │   ├── RND_Step_Ground           Random Shuffle · Step · Pitch ±150 · Vol ±2
    │   ├── RND_Step_Mushroom         + Low-pass randomizer 0–15
    │   └── RND_Step_Shadow           + Low-pass FIJO 25 · −1.5 dB
    └── [Actions]  (Virtual Folder)
        ├── RND_Gem_Pickup            Random Shuffle · Step · Limit rep. 2 · Pitch ±200
        ├── SFX_Shadow_Place
        ├── SFX_Key_Place
        └── SFX_Death

Ambience.wwu
└── AMX_Ambience                      → Bus_AMB · UserAuxSend0 Aux_Altar −9 dB
    └── SW_Zone_Ambience              Switch Container · Zone · Default Field · CONTINUOUS
        ├── BLD_Field                 Blend · Continuous        [switch Field]
        │   ├── Blend Track "Sun" · SunExposure · Crossfade ON
        │   │   ├── SFX_Wind_Soft         zona  0 – 40    Loop Infinite
        │   │   ├── SFX_Wind_Med          zona 25 – 75    Loop Infinite
        │   │   └── BLD_Sun_High          zona 60 – 100   Blend agrupador · Continuous
        │   │       ├── SFX_Wind_Hot          Loop Infinite
        │   │       └── SFX_Sun_Sizzle        Loop Infinite
        │   └── SW_Fauna              Switch Container · Fauna · CONTINUOUS
        │       │                     (sin Blend Track: vive en paralelo al viento)
        │       ├── Active → BLD_Fauna        Blend agrupador · Continuous
        │       │     ├── RND_Insects         Continuous + Loop · Delay 7 s −3/+5
        │       │     │   ├── SFX_Insects_01      w 30
        │       │     │   ├── SFX_Insects_02      w 30
        │       │     │   └── SIL_Insects_Gap     w 40 · 6 s −3/+8
        │       │     └── RND_Mushrooms       Continuous + Loop · Delay 11 s −5/+7
        │       │         ├── SFX_Mushrooms_01    w 25
        │       │         ├── SFX_Mushrooms_02    w 25
        │       │         └── SIL_Mushrooms_Gap   w 50 · 10 s −5/+14
        │       └── Silent → (vacío)
        ├── BLD_Altar                 Blend agrupador · Continuous  [switch Altar]
        │   ├── SFX_Altar_Voices          Loop Infinite
        │   ├── SFX_Altar_Static          Loop Infinite
        │   └── SFX_Altar_Aura            Loop Infinite
        └── SFX_Void                  Loop Infinite · asignado a LOS DOS switches
                                      Continue to play across switches · fades 2 s
                                      RTPC Turns → Voice Volume (−6 → 0 dB)

Interface.wwu
└── AMX_UI                            → Bus_UI  (sin State, sin send)
    ├── SFX_Turn_Pulse                RTPC Turns → Pitch (0 → +500 cents)
    │                                 RTPC Turns → Volume (−9 → 0 dB)
    │                                 randomizer de Pitch ±40
    ├── SFX_UI_Hover                  misma source que Click · +250 cents · −7 dB
    ├── SFX_UI_Click
    └── SFX_Win_Level
```

### Master-Mixer

```
Master Audio Bus
├── Bus_SFX              State Recinto · Altar → Lowpass 12 · Volume −1
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
| `AltarDistance` | Game Parameter | 0–100, default 100 | Switch Group `Zone` | Unity, distancia en tiles al Altar |
| `Turns` | Game Parameter | 0–100, default 0 | RTPC en `SFX_Turn_Pulse` y `SFX_Void` | Unity, turnos transcurridos normalizados |
| `Tile` | Switch Group | Ground · Mushroom · Shadow | `SW_Steps_Tile` | `Set Switch` antes de cada `Play_Footstep` |
| `Zone` | Switch Group | Altar · Field | `SW_Zone_Ambience` | **Use Game Parameter** → `AltarDistance` |
| `Fauna` | Switch Group | Active · Silent | `SW_Fauna` | **Use Game Parameter** → `SunExposure` |
| `Recinto` | State Group | None · Exterior · Altar | `Bus_SFX` y `Aux_Altar` | `Set State` al cruzar el umbral del Altar |

### Events

| Event | Acciones | Lo dispara Unity cuando… |
|---|---|---|
| `Play_Ambience` | Play `SW_Zone_Ambience` | se carga el nivel (una sola vez) |
| `Stop_Ambience` | Stop, Fade Time 3 s | se descarga el nivel |
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

**Asset.** Tres variaciones por superficie, one-shots **sin Loop**: una pisada es un
transitorio, no material loopeable.

**Contenedor.** `SW_Steps_Tile`, un **Switch Container** con el Switch Group `Tile`,
Default Switch `Ground`, y un **Random Container** por switch.

**Play Mode: Step.** El ritmo lo marca el juego: un paso por movimiento en la
grilla. Cada `Play_Footstep` reproduce **un** hijo y avanza el puntero. En
Continuous, un solo Play caminaría para siempre.

**Por qué Switch y no Random para la superficie.** La baldosa es una **condición del
juego**, no azar. Unity hace `Set Switch Tile` con el tipo de la celda de destino
*antes* de llamar al Event, así que el cambio de superficie ya se oye en la pisada
siguiente y no hace falta Continuous.

**Variación en dos ejes.** Esto es lo que evita el *machine gun effect*:

- **Qué suena** → Random **Shuffle**: recorre las tres variaciones antes de repetir
  ninguna. *"Limit repetition to"* queda en **1**, no en 2: con sólo 3 hijos,
  subirlo a 2 dejaría un único candidato y la elección pasaría a ser determinista.
- **Cómo suena** → **randomizers puestos en el contenedor** para que se hereden:
  **Pitch ±150 cents** y **Volume ±2 dB**. En Wwise el Voice Pitch es *velocidad de
  reproducción*: cambia altura **y duración** a la vez, lo que hace que cada pisada
  tenga un peso levemente distinto.

Con 3 assets por superficie, los dos ejes combinados dan muchas más pisadas
distinguibles que 3. Esa es la idea que atraviesa todo Wwise: **pocos assets, mucha
variación**.

**Diferencias por material, además de la variación:**

| Contenedor | Qué se le agregó | Por qué |
|---|---|---|
| `RND_Step_Ground` | sólo los randomizers | es la referencia |
| `RND_Step_Mushroom` | Pitch ±180 y **randomizer de Low-pass 0–15** | el hongo es blando y esponjoso: responde distinto en cada pisada y nunca tiene el mismo brillo |
| `RND_Step_Shadow` | **Low-pass FIJO 25** y **−1.5 dB** | pisar dentro de la sombra suena más apagado y más lejano. No es sólo estético: el jugador aprende a reconocer por el sonido si está a salvo |

El Low-pass fijo y el randomizer son dos cosas distintas y conviene decirlo: **el
valor fijo tiñe el timbre, el randomizer evita la repetición.**

**Optimización.** Fuera de su switch, el Random de esa superficie **no se
instancia**: no existe y no gasta voz.

---

## 4 · Ambiente

### 4.1 · Un solo Event

Todo el paisaje arranca con **`Play_Ambience`**, que Unity llama una sola vez al
cargar el nivel. De ahí en más el código no vuelve a decidir qué suena: la lógica
vive en la jerarquía de Wwise y se reconfigura sola con los Game Syncs. Eso es
exactamente para lo que existe el middleware — desacoplar el código del diseño.

### 4.2 · `SW_Zone_Ambience` — las dos zonas

**Switch Container, Play Mode CONTINUOUS.** El porqué es el punto clave: el Event se
llama una sola vez y el contenedor **queda escuchando** los cambios del Switch Group
`Zone`. Cuando Blobli se acerca al Altar, el contenedor cambia de hijo solo, con
Fade-in/Fade-out de 2 s. **En Step no pasaría nada hasta el próximo Play**, y no hay
próximo Play.

El Switch Group `Zone` se deriva de `AltarDistance` con **Use Game Parameter**:
curva de escalones, 0–35 `Altar`, 35–100 `Field`. La curva tiene dos puntos planos
por switch y **ningún tramo cae en `None`**: un tramo mapeado a None sería silencio
total del ambiente.

**Optimización:** fuera de su switch un objeto no se instancia. Parado en el campo,
las tres capas del Altar no existen; dentro del Altar, no existe ninguna capa de
clima.

### 4.3 · El sonido compartido

`SFX_Void` está asignado a **los dos switches a la vez**, porque el void está
siempre, adentro y afuera del Altar. Por eso lleva **Continue to play across switch
transitions**: sin eso, al cruzar de zona el drone se reiniciaría desde el
principio y el corte se escucharía. Los Fade-in / Fade-out Time de 2 s completan la
transición para que no sea un salto.

### 4.4 · Sistema dinámico de clima

`BLD_Field` es un **Blend Container en Continuous** con un **Blend Track** llamado
`Sun`, **Crossfade tildado** y `SunExposure` como Game Parameter.

| Capa | Zona | Contenido |
|---|---|---|
| `SFX_Wind_Soft` | 0 – 40 | viento quieto de la zona en sombra |
| `SFX_Wind_Med` | 25 – 75 | el sol empieza a dar sobre el claro |
| `BLD_Sun_High` | 60 – 100 | viento seco **+** el sol quemando la tierra |

**Las zonas se SOLAPAN a propósito.** Si sólo se tocaran en el borde, el Crossfade no
tendría sobre qué cruzar y se oiría un escalón en cada frontera. Y entre las tres
**cubren 0 a 100 completo**: un tramo del parámetro sin ninguna capa sería un tramo
mudo.

**Por qué Blend y no Switch:** la exposición al sol es una **variable analógica** y
queremos un cambio **continuo, sin escalones**. Con un Switch el cambio sería
discreto.

**Por qué los hijos van con Loop Infinite:** en un Blend Container el **Loop va en el
hijo**, porque el Blend no repite a sus hijos. Un hijo de Blend sin Loop suena una
vez y desaparece. (En un Random/Sequence Continuous es al revés: el Loop va en el
contenedor.)

**Por qué `BLD_Sun_High` es un sub-Blend:** es la única zona que necesita dos
elementos, porque es la que introduce la amenaza. Es un **Blend agrupador**: sin
Blend Track propio, así que sus dos hijos suenan siempre juntos mientras la capa
esté en zona.

**Optimización:** en Continuous el Blend **no instancia** los hijos fuera de zona. Si
esto se resolviera con tres curvas RTPC de Voice Volume, las tres capas estarían
sonando siempre y dos de ellas inaudibles, gastando voces — y se vería en el
Profiler.

### 4.5 · La fauna: randomización del ambiente

`SW_Fauna` es un **Switch Container en Continuous** colgado de `BLD_Field` pero **no
asignado a ningún Blend Track**: por eso vive en paralelo a las capas de viento y
suena mientras estemos en el campo.

Su Switch Group `Fauna` se deriva de `SunExposure` con **Use Game Parameter**:
**0–65 `Active`, 65–100 `Silent`**. El switch `Silent` **no tiene objetos
asignados**: con el sol alto los bichos se callan, y —lo importante— sus Random
Containers **no se instancian**. Es una idea de diseño (la fauna se esconde del sol)
que además es el camino óptimo.

Adentro de `Active`, dos sistemas intermitentes corriendo en paralelo con cadencias
distintas, para que el claro nunca suene a bucle:

| | `RND_Insects` | `RND_Mushrooms` |
|---|---|---|
| Play Mode | Continuous + **Loop Infinite** | Continuous + **Loop Infinite** |
| Transition | **Delay** 7 s, randomizer −3/+5 | **Delay** 11 s, randomizer −5/+7 |
| Hijos | 2 one-shots (w 30 / 30) + Silence (w 40) | 2 one-shots (w 25 / 25) + Silence (w 50) |
| Silence | 6 s, Random Min −3 / Max 8 | 10 s, Random Min −5 / Max 14 |
| Randomizers | Pitch ±200 · Volume ±3 · Low-pass 0–30 | Pitch ±250 · Volume ±2.5 · Low-pass 0–25 |

Las justificaciones, una por una:

- **El Loop va en el CONTENEDOR, no en el hijo.** Un hijo que loopea no termina
  nunca y el contenedor jamás dispararía el siguiente.
- **Delay y no Trigger Rate.** El Delay cuenta **desde que termina** un elemento, así
  que dos bichos nunca se pisan. El Trigger Rate es de inicio a inicio y, si fuera
  menor que la duración del asset, los superpondría.
- **El Delay va randomizado.** Un Delay fijo suena a metrónomo. Con 7 s −3/+5 el
  intervalo real va de 4 a 12 s.
- **Los objetos Silence** (source **Wwise Silence**) con Duration y Random Min/Max
  abren los huecos largos: son los que hacen que el claro respire en vez de estar
  lleno de bichos todo el tiempo.
- **Los weights** hacen que el silencio sea más probable que cualquiera de los dos
  assets.
- **Random Standard con "Limit repetition" DESACTIVADO**, a propósito: con Shuffle, o
  con el límite activo, el silencio no podría salir dos veces seguidas y **los
  weights quedarían sin efecto**. Acá el que manda tiene que ser el peso.
- **Los randomizers de Pitch, Volume y Low-pass** hacen que los mismos dos assets se
  lean como bichos a distintas distancias. El Low-pass es el que más trabaja para
  eso: lo lejano pierde agudos.

---

## 5 · Recinto adicional: el Altar

### Qué se implementó

- **State Group `Recinto`** con los States `Exterior` y `Altar`, y **Default
  Transition Time 0.8 s**. Lo setea Unity con `Set State` al cruzar el umbral de la
  cámara (Events `Set_Recinto_Altar` / `Set_Recinto_Exterior`, Scope Global).
- **Auxiliary Bus `Aux_Altar`** con el Effect ShareSet **`Reverb_Altar`** (Wwise
  RoomVerb: DecayTime 5.2 s, RoomSize 90, PreDelay 32 ms, HFDamping 7.5, tone control
  de −7 dB a partir de 5.5 kHz; **DryLevel −96**).
- **Sends:** `AMX_Player` y `AMX_Ambience` tienen un **User-Defined Auxiliary Send**
  fijo a `Aux_Altar`, a −6 y −9 dB. Se pone una vez en el Actor-Mixer y no objeto
  por objeto.
- **Qué hace el State:**
  - sobre `Aux_Altar`: `None` y `Exterior` → **−96 dB**, `Altar` → **0 dB**;
  - sobre `Bus_SFX`: `Altar` → **Lowpass 12** y **−1 dB**.

### Las justificaciones

**Por qué un State y no un Switch.** El State es un **offset global** que modifica
propiedades de lo que **ya está sonando**: no crea ni elimina instancias. Es
exactamente lo que necesitamos, porque al entrar a la cámara no queremos re-disparar
el ambiente, queremos que el mismo ambiente suene distinto. El Switch, en cambio, es
por Game Object y **elige qué objeto suena** — por eso el Switch Group `Zone` decide
*qué* se oye en cada zona y el State Group `Recinto` decide *cómo suena el espacio*.
Son dos preguntas distintas y por eso hay dos mecanismos.

**Por qué la reverb va en un Auxiliary Bus y no como insert en cada objeto.** Una
sola instancia del efecto procesa a todos los emisores, y el nivel de retorno se
controla en un solo lugar. Por eso el ShareSet tiene `DryLevel −96`: el camino seco
ya va por `Bus_SFX`, el Aux sólo tiene que devolver la señal húmeda.

**Por qué el State mueve el volumen del Aux y no el send de cada objeto.** Es un
único punto de control y, sobre todo, la reverb **nunca se re-instancia**: la misma
instancia sube y baja de nivel con el Default Transition Time del State Group.

**Por qué además del reverb hay un Low-pass.** Las paredes de piedra **bloquean los
agudos y dejan pasar los graves**. Con reverb sola el efecto suena a "le puse
reverb"; con reverb **más** Low-pass suena a estar adentro. Son dos ejes trabajando
a la vez y es lo que hace creíble el recinto.

**Por qué `None` también lleva −96 dB.** Si Unity nunca llamara a `Set State`, el
State activo sería `None`. Sin ese offset tendríamos reverb de cámara en el campo
abierto.

**Por qué `Bus_UI` queda afuera.** `Bus_UI` cuelga directo del Master Audio Bus y no
tiene ni el State ni send a `Aux_Altar`. El hover, el click, el jingle de victoria y
el pulso de turno **no ocurren adentro del mundo de Blobli**: son información para el
jugador. Si se oscurecieran y se llenaran de reverb al entrar al Altar, el jugador
perdería feedback justo en el momento más importante del nivel.

---

## 6 · Variable de gameplay: `Turns`

En Blobli **el entorno sólo avanza cuando el jugador hace un movimiento**. El tiempo
no corre: se gasta. Por eso la variable de gameplay elegida es `Turns` (turnos
transcurridos en el nivel, normalizados 0–100), y no un reloj en segundos.

### 6.1 · El pulso de turno

`SFX_Turn_Pulse` es un **one-shot sin Loop** disparado por el Event `Play_Turn`, que
Unity llama en cada movimiento junto con `Play_Footstep`.

**Por qué un one-shot en Step y no un contenedor en Continuous con Trigger Rate.**
Porque el ritmo lo marca el juego, no el contenedor: el tic existe cuando el jugador
se mueve, y si el jugador se queda quieto el tiempo no avanza. Un Random Continuous
con Trigger Rate generaría su propia cadencia y eso sería mentirle al jugador sobre
la mecánica. **Lo que cambia con la variable no es la cadencia, es el carácter del
sonido.**

Dos curvas RTPC contra `Turns`, las dos sobre este objeto:

| Propiedad | Turns 0 | Turns 60 | Turns 100 |
|---|---|---|---|
| **Voice Pitch** | 0 cents | +120 | **+500 cents** |
| **Voice Volume** | −9 dB | — | **0 dB** |

La curva de Pitch es no lineal a propósito: casi no se mueve en la primera mitad del
nivel y se dispara sobre el final. Y acá conviene recordar que **el Voice Pitch de
Wwise es velocidad de reproducción**: cambia altura **y duración**, así que sobre el
final el tic también se acorta, lo que refuerza la sensación de apuro sin tocar el
asset.

Además lleva un **randomizer de Pitch de ±40 cents** para que dos turnos seguidos no
suenen calcados.

### 6.2 · El void que avanza

`SFX_Void` tiene una curva RTPC contra `Turns` sobre **Voice Volume**, de **−6 dB a
0 dB**. A medida que pasan los turnos el void se hace más presente: el tiempo se
agota y la sombra avanza. Es la misma variable leída desde el otro lado — el
personaje se apura, el mundo se oscurece.

**Por qué RTPC directo y no Switch ni Blend en estos dos casos.** No hay que elegir
entre alternativas ni cruzar capas: es el **ajuste continuo de una propiedad de un
objeto que ya está sonando**. Es el mecanismo más simple y el más barato para eso.

**Por qué las curvas están cada una en un solo objeto.** Si la misma curva estuviera
en el padre y en el hijo, **se sumarían**.

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

- **`Bus_Player` y `Bus_AMB` separados** para poder balancear personaje contra
  paisaje con un solo fader en vez de objeto por objeto.
- **`Bus_UI` colgado del Master**, fuera de `Bus_SFX`, por la razón de la sección 5.
- El criterio general para resolver enmascaramientos: **se baja el que enmascara, no
  se sube el enmascarado**, porque subir consume headroom y aplana la dinámica. Si
  hace falta, HPF/LPF en el sonido secundario para hacerle lugar al principal en la
  zona espectral.

---

## 8 · Soundcaster: recorrido de prueba

Sesión **`SC_Blobli_TP1`**: los 13 Events como módulos, sliders de `SunExposure`,
`AltarDistance` y `Turns`, selector de Switch `Tile` y selector de State `Recinto`.

`Zone` y `Fauna` **no se tocan a mano**: se derivan solos de `AltarDistance` y
`SunExposure`. Que el switch activo cambie al mover el slider es, en sí, la
demostración de que "Use Game Parameter" está funcionando.

1. **`Play_Ambience`.** Arranca en `Field` (AltarDistance 100) con `SunExposure 0`:
   el void, el viento quieto y los bichos.
2. **Barrer `SunExposure` de 0 a 100 despacio.** Se cruzan las tres capas de viento
   sin escalones, entra el sol y, al pasar 65, **la fauna desaparece**. Verificar que
   en 100 sigue sonando algo: si hay silencio, el Blend Track tiene un agujero.
3. **Volver `SunExposure` a 20.** La fauna vuelve con un fade de 3 s.
4. **Bajar `AltarDistance` por debajo de 35.** Cruza a la zona del Altar con fade de
   2 s, y **el void NO se reinicia** (Continue to play across switch transitions).
5. **`Set_Recinto_Altar`.** Entra la reverb y se oscurece todo lo diegético.
6. **`Play_UI_Click`** estando en el Altar: suena limpio, sin reverb ni Low-pass.
   Esa es la demostración de por qué `Bus_UI` está afuera de `Bus_SFX`.
7. **`Set_Recinto_Exterior`** y subir `AltarDistance` a 100. Vuelve al campo.
8. **`Play_Footstep`** repetidas veces cambiando el selector `Tile`: tres timbres
   distintos y, dentro de cada uno, variación sin repetición obvia.
9. **`Play_Turn`** con `Turns` en 0, después en 50, después en 100: el mismo asset se
   vuelve más agudo, más corto y más presente. Y el void sube con él.
10. **`Play_Gem_Pickup`** seis o siete veces seguidas: no se escucha la misma
    variación dos veces seguidas.
11. **`Play_Death`**: suena la muerte y el ambiente se retira con fade de 1.5 s
    arrancando 0.2 s después. Demuestra un Event de dos acciones ordenadas con Delay.
12. **`Play_Ambience`** de nuevo y **`Play_Win`**.

Conviene dejar **una captura del Profiler (F6)** en los pasos 2 y 9: ahí se ve que
con `SunExposure` arriba de 65 las voces de la fauna directamente no existen, y que
las capas de viento fuera de zona tampoco. Es la prueba visual del argumento de
optimización.

---

## 9 · Problemas abiertos

Completar con lo que efectivamente quede pendiente. Candidatos reales:

- Loop points y crossfades ajustados a oído en los 7 assets que loopean.
- El `ReverbLevel` y el `DecayTime` de `Reverb_Altar` están puestos "de taller": hay
  que calibrarlos contra el arte final de la cámara.
- Si en algún nivel hubiera varias fuentes del mismo loop (por ejemplo varios grupos
  de hongos), habría que agregarles una acción **Seek** con punto de inicio random
  para que las copias no suenen en fase.
- Un `SEQ_` (Sequence Container) para algo con orden, por ejemplo la apertura del
  Altar cuando entran las dos Key gems: cerradura → mecanismo → luz.
- Generar un SoundBank y conectar con Unity.
- Música interactiva, fuera del alcance de este TP.
