# Guía del proyecto — para entender y defender lo que hay

Escrito para alguien que recién arranca con Wwise. Tres partes: dónde se toca cada
cosa en la GUI, por qué está así, y las reglas que conviene saber de memoria.

---

## 1 · Tres enums de Wwise que están al revés

Esto es lo primero, porque es lo que hace que un proyecto **parezca** bien
configurado sin estarlo. En un `.wwu`, **si una propiedad no está escrita, está en
su valor por defecto**:

| Propiedad (en el XML) | Dónde se ve en la GUI | Default | Ojo |
|---|---|---|---|
| `PlayMechanismStepOrContinuous` | Random/Sequence → Play Mode | **Step** | en el XML **0 = Continuous**, 1 = Step |
| `NormalOrShuffle` | Random → Random type | **Standard** | en el XML **0 = Shuffle**, 1 = Standard |
| `BlendBehavior` | Blend → Play Mode | Step | 0 = Step, 1 = Continuous |
| `SwitchBehavior` | Switch → Play Mode | **Step** | 0 = Step, 1 = Continuous |
| `IsLoopingEnabled` | General Settings → Loop | **apagado** | **nada loopea** si no lo tildás |
| `PlayMechanismSpecialTransitions` | Play Mode → Transitions | **apagado** | no hay Delay ni Trigger Rate por default |
| `RandomAvoidRepeating` / `Count` | Random → Limit repetition to | **activo, 1** | el "no repetir el último" ya viene puesto |
| `Length` del Wwise Silence | Source Editor del Silence | **1 s, sin random** | un Silence recién creado es un hueco fijo de 1 s |
| `UseGameParameter` | Switch Group | apagado | tildarlo **no alcanza**: falta el parámetro y la curva |
| Fade-in / Fade-out de un objeto asignado a un switch | pestaña del objeto en el Switch Container | **0** | los cambios de switch cortan seco |

El caso más traicionero: **un Switch Container "sin configurar" está en Step**, así
que no reacciona a los cambios de switch hasta el próximo Play. Si tu sistema de
zonas no hace nada, es casi seguro esto.

---

## 2 · Dónde se toca cada cosa

| Lo que querés cambiar | Dónde |
|---|---|
| Play Mode de un contenedor | Property Editor → pestaña **General Settings** |
| Loop de un Sound SFX | Property Editor → **General Settings → Loop → Infinite** |
| Loop points y crossfade | **Source Editor**: doble click en la fila del archivo en el **Contents Editor** (no en el objeto) |
| Randomizers (la "pelotita") | el iconito al lado de la propiedad, en el Property Editor |
| Shuffle y "Limit repetition to" | Random Container → General Settings → **Random type** |
| Delay / Trigger Rate | Random Container en **Continuous** → General Settings → **Transitions** |
| Weights y orden de hijos | **Contents Editor** del Random Container |
| Zonas del Blend Track | Blend Container → pestaña **Blend Tracks** |
| Qué objeto va en qué switch | Switch Container → pestaña **Assigned Objects** |
| Continue to play / Fade-in / Fade-out de un switch | Switch Container → Assigned Objects → seleccionar el objeto |
| Use Game Parameter y su curva | **Switch Group** (en Game Syncs), no en el contenedor |
| Curvas RTPC | Property Editor del objeto → pestaña **RTPC** |
| States de un bus | Property Editor del bus → pestaña **States** |
| Efecto en un bus | Property Editor del bus → pestaña **Effects** |
| Aux sends de un objeto | Property Editor → **General Settings → User-Defined Auxiliary Sends** |
| Ver si una voz existe o no | **Profiler (F6)** |

---

## 3 · Por qué cada contenedor es lo que es

### Pisadas · `SW_Steps_Tile` → Switch Container en **Step**

El ritmo lo marca el juego: un Play por movimiento. En Continuous, un solo Play
caminaría para siempre.

Adentro, un **Random en Step** por superficie: el orden no significa nada, son
variaciones intercambiables. Shuffle decide *qué* suena; los randomizers de Pitch y
Volume deciden *cómo* suena. **Los randomizers van en el contenedor, no en cada
hijo**, porque así se heredan y los configurás una sola vez.

> Detalle que vale: con **3 hijos**, "Limit repetition to" tiene que quedar en **1**.
> Si lo subís a 2 queda un único candidato y la elección deja de ser al azar.
> Con 4 o más hijos, 2 está bien (en `RND_Gem_Pickup`, que tiene 4, está en 2).

### Ambiente · `SW_Zone_Ambience` → Switch Container en **Continuous**

El Event se llama una vez y el contenedor **queda escuchando** los cambios de
`Zone`. En Step no pasaría nada hasta el próximo Play, y no hay próximo Play.

### Clima · `BLD_Field` → Blend Container en **Continuous**, con Blend Track

Un Blend Container reproduce **todos** sus hijos a la vez. El Blend Track con
Crossfade los cruza según `SunExposure`.

- Si un Blend Track **no tiene Crossfade tildado ni parámetro asignado**, es sólo un
  agrupador: todos los hijos suenan juntos ignorando el valor.
- Las zonas **tienen que solaparse**. Si sólo se tocan en el borde, no hay nada que
  cruzar: se oye un salto.
- Las zonas **tienen que cubrir todo el rango**. Un tramo sin capa es un tramo mudo.
- **En Blend, el Loop va en el HIJO.** El Blend no repite a sus hijos: un hijo sin
  Loop suena una vez y desaparece.

`BLD_Sun_High`, `BLD_Fauna` y `BLD_Altar` son **Blend agrupadores**: Continuous, sin
Blend Track propio, sólo para que sus hijos suenen siempre juntos y se puedan
mover/mezclar como una sola cosa.

### Fauna · `SW_Fauna` → Switch Container en **Continuous**, con Use Game Parameter

El Switch Group `Fauna` se deriva de `SunExposure` con una curva de escalones. El
switch `Silent` **está vacío a propósito**: con el sol alto los Random de fauna no se
instancian.

Es el patrón que hay que saber: **para apagar algo según un umbral, se usa un Switch
(o un Blend) con el tramo vacío, no un RTPC de volumen a −200 dB.** Con el Switch la
instancia *no existe*; con el RTPC sigue viva, inaudible, gastando una voz — y eso
se ve en el Profiler.

### Intermitentes · `RND_Insects` y `RND_Mushrooms` → Random en **Continuous + Loop**

Esta es la plantilla del "ambiente intermitente", la que más puntos da si se explica
entera:

1. Play Mode **Continuous** + **Loop Infinite** → el contenedor genera su propia
   cadencia.
2. **El Loop va en el CONTENEDOR.** Si lo pusieras en el hijo, el hijo no terminaría
   nunca y el contenedor jamás dispararía el siguiente.
3. Transition **Delay** con randomizer. El Delay cuenta **desde que termina** un
   elemento, así que nunca se pisan. (El **Trigger Rate** es de inicio a inicio: si
   fuera menor que la duración del asset, los superpondría. Sirve para cadencias
   regulares tipo latido o reloj.)
4. **Objetos Silence** (source *Wwise Silence*) con Duration y Random Min/Max, para
   los huecos largos.
5. **Weights** para que el silencio sea más probable que el evento.
6. **Randomizers de Pitch, Volume y Low-pass** en el contenedor: el mismo asset se
   lee a distintas distancias.

> **Importante y poco obvio:** en estos contenedores el Random está en **Standard**
> con **"Limit repetition" DESACTIVADO**. Si estuviera en Shuffle, o con el límite
> activo, el silencio no podría salir dos veces seguidas y **los weights quedarían
> sin efecto**. Shuffle es para variaciones equivalentes (las pisadas, las gemas);
> los weights son para cuando una opción tiene que ser mucho más probable que otra.

### Pulso de turno · `SFX_Turn_Pulse` → one-shot en Step, con RTPC

No es un contenedor: es un Sound SFX disparado por `Play_Turn`. El ritmo lo marca el
jugador. Lo que cambia con `Turns` es el **carácter** del sonido (Pitch y Volume), no
la cadencia.

Si el juego fuera en tiempo real, la receta sería la otra: Random Continuous + Loop +
**Trigger Rate** con un RTPC sobre la Duration de la transición. Conviene tener las
dos claras porque la segunda es la que suele caer en el parcial.

---

## 4 · Switch, State, Blend y RTPC: cuál es cuál

| | Qué hace | Granularidad | Fuera de rango |
|---|---|---|---|
| **Switch** | elige **qué objeto suena** | por Game Object | la instancia **no existe** |
| **State** | **modifica propiedades** de lo que ya suena | **global** | no aplica: no crea ni destruye nada |
| **Blend** | cruza capas de forma continua | por Game Object | en Continuous **no existe**; en Step suena a −∞ |
| **RTPC directo** | ajusta **una propiedad** de forma continua | por Game Object | **sigue sonando inaudible y gasta voz** |

En este proyecto están los cuatro, y cada uno responde una pregunta distinta:

- `Zone` (**Switch**) → *¿qué se oye en esta parte del mapa?*
- `Fauna` (**Switch**) → *¿los bichos existen o no?*
- `Recinto` (**State**) → *¿cómo suena el espacio?*
- `Sun` (**Blend**) → *¿cuánto sol hay?* (continuo, sin escalones)
- `Turns` (**RTPC**) → *¿cuánto apura el tiempo?* (ajuste fino de dos propiedades)

La frase que conviene tener lista: **el State es global y modifica propiedades; el
Switch es por Game Object y elige qué objeto suena. El State no crea ni destruye
instancias.**

---

## 5 · Convención de nombres usada

| Tipo | Prefijo | Ejemplo |
|---|---|---|
| Actor-Mixer | `AMX_` | `AMX_Player` |
| Switch Container | `SW_` | `SW_Steps_Tile` |
| Blend Container | `BLD_` | `BLD_Field` |
| Random Container | `RND_` | `RND_Insects` |
| Sequence Container | `SEQ_` | (todavía no hay) |
| Sound SFX con archivo | `SFX_` | `SFX_Wind_Soft` |
| Sound SFX con Wwise Silence | `SIL_` | `SIL_Insects_Gap` |
| Audio Bus | `Bus_` | `Bus_AMB` |
| Auxiliary Bus | `Aux_` | `Aux_Altar` |
| Effect ShareSet | según el efecto | `Reverb_Altar` |
| Event | `Play_` / `Stop_` / `Set_` | `Play_Footstep` |
| Game Parameter, Switch Group, State Group | PascalCase, sin prefijo | `SunExposure`, `Tile`, `Recinto` |
| Virtual Folder | nombre común | `Actions` |

Reglas que la acompañan:

1. El nombre dice **qué es y para qué sirve**, nunca de dónde salió el archivo.
2. Variaciones numeradas con **dos dígitos** y arrancando en `_01`.
3. **Un Work Unit por sistema** (`Player`, `Ambience`, `Interface`), no uno por
   entrega. Así además dos personas pueden trabajar en paralelo sin pisarse.
4. **Comentario en todo contenedor que tome una decisión.** El campo *Comment* del
   Property Editor se guarda en el `.wwu` y es de donde después sale el informe. En
   este proyecto ya están todos escritos: leelos, son la mitad del trabajo de
   documentación hecho.
5. Un Virtual Folder es **sólo organización visual**: no es un contenedor, no afecta
   el ruteo ni las propiedades.

---

## 6 · Las doce reglas de memoria

1. **Siempre decir el Play Mode.** Si no está especificado, el punto no vale.
2. **Step = el juego marca el ritmo. Continuous = el contenedor lo marca** (y
   habilita Loop y Transitions).
3. Un **Switch Container en Step no reacciona** al cambio de switch hasta el próximo
   Play.
4. En **Random/Sequence Continuous el Loop va en el contenedor**. En **Blend va en el
   hijo**.
5. Si el hijo loopea, **el Delay y el Trigger Rate no sirven**: el contenedor nunca
   dispara el siguiente.
6. **Delay** cuenta desde que *termina* un elemento (nunca se pisan). **Trigger
   Rate** es de inicio a inicio.
7. Un **Blend Track sin Crossfade o sin parámetro es un agrupador**: todos los hijos
   suenan a la vez ignorando el valor.
8. Las **zonas de un Blend Track se solapan** y **cubren todo el rango**.
9. En una curva de **Switch Group, el eje Y es el índice del switch empezando en 1**,
   y el **0 es `None`** = silencio.
10. **Fuera de rango, el sonido no debe existir.** Switch o Blend en Continuous no
    instancian; un RTPC de volumen a −200 dB deja la voz viva.
11. La **misma curva RTPC en padre e hijo se suma**. Dejá una sola.
12. **Shuffle y "Limit repetition" pisan a los weights.** Si querés que manden los
    weights (silencio 50 / evento 25), usá Random **Standard** con el límite
    desactivado.

### Dos que son de señal, no de Wwise

- **El click**: la onda oscila alrededor del eje de amplitud 0, que es el parlante en
  reposo. Si el audio arranca o corta fuera del cruce por cero, el parlante salta de
  golpe y eso se oye como click. Un fade lleva la amplitud a 0 y garantiza empezar y
  terminar en reposo.
- **El Voice Pitch de Wwise es velocidad de reproducción**: cambia altura **y
  duración** a la vez. Nunca digas sólo "cambia el tono". +1200 cents = ×2 de
  velocidad = la mitad de duración.
