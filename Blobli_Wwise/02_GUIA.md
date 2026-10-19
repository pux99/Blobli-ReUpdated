# Guía del proyecto — para entender y defender lo que hay

Escrito para alguien que recién arranca con Wwise. Cuatro partes: los enums que
confunden, dónde se toca cada cosa, por qué cada contenedor es lo que es, y las
reglas que conviene saber de memoria.

---

## 1 · Enums de Wwise que están al revés

Esto es lo primero, porque es lo que hace que un proyecto **parezca** bien
configurado sin estarlo. En un `.wwu`, **si una propiedad no está escrita, está en su
valor por defecto**:

| Propiedad (XML) | En la GUI | Default | Ojo |
|---|---|---|---|
| `PlayMechanismStepOrContinuous` | Random/Sequence → Play Mode | **Step** | en el XML **0 = Continuous**, 1 = Step |
| `NormalOrShuffle` | Random → Random type | **Standard** | en el XML **0 = Shuffle**, 1 = Standard |
| `BlendBehavior` | Blend → Play Mode | Step | 0 = Step, 1 = Continuous |
| `SwitchBehavior` | Switch → Play Mode | **Step** | 0 = Step, 1 = Continuous |
| `IsLoopingEnabled` | General Settings → Loop | **apagado** | **nada loopea** si no lo tildás |
| `PlayMechanismSpecialTransitions` | Play Mode → Transitions | **apagado** | no hay Delay ni Trigger Rate por default |
| `RandomAvoidRepeating` / `Count` | Random → Limit repetition to | **activo, 1** | el "no repetir el último" ya viene puesto |
| `Length` del Wwise Silence | Source Editor | **1 s, sin random** | un Silence recién creado es un hueco fijo de 1 s |
| `UseGameParameter` | Switch Group | apagado | tildarlo **no alcanza**: falta el parámetro y la curva |
| Fade-in/out de un objeto asignado a un switch | pestaña del objeto | **0** | los cambios de switch cortan seco |

El caso más traicionero: **un Switch Container "sin configurar" está en Step**, así
que no reacciona a los cambios de switch hasta el próximo Play. Si tu sistema de
zonas no hace nada, es casi seguro esto.

---

## 2 · Dónde se toca cada cosa

| Lo que querés cambiar | Dónde |
|---|---|
| Play Mode de un contenedor | Property Editor → **General Settings** |
| Loop de un Sound SFX | Property Editor → **General Settings → Loop → Infinite** |
| Loop points y crossfade | **Source Editor**: doble click en la fila del archivo en el **Contents Editor** (no en el objeto) |
| Trim Start / Trim End, fades del asset | Source Editor |
| Randomizers (la "pelotita") | el iconito al lado de la propiedad |
| Shuffle y "Limit repetition to" | Random Container → General Settings → **Random type** |
| Delay / Trigger Rate | Random Container en **Continuous** → **Transitions** |
| Weights y orden de hijos | **Contents Editor** del Random |
| Zonas del Blend Track | Blend Container → pestaña **Blend Tracks** |
| Qué objeto va en qué switch | Switch Container → **Assigned Objects** |
| Continue to play / fades de switch | Assigned Objects → seleccionar el objeto |
| Use Game Parameter y su curva | **Switch Group** (en Game Syncs), no en el contenedor |
| Curvas RTPC | Property Editor del objeto → pestaña **RTPC** |
| States de un bus | Property Editor del bus → pestaña **States** |
| Efecto en un bus u objeto | Property Editor → pestaña **Effects** |
| **Render** de un efecto | pestaña Effects → checkbox **Render** del slot |
| Aux sends | Property Editor → **General Settings → User-Defined Auxiliary Sends** |
| Ver si una voz existe o no | **Profiler (F6)** |

---

## 3 · Por qué cada contenedor es lo que es

### Pisadas · `SW_Steps_Tile` → Switch Container en **Step**

El ritmo lo marca el juego: un Play por movimiento. En Continuous un solo Play
caminaría para siempre.

Adentro, un **Random en Step** por superficie: el orden no significa nada, son
variaciones intercambiables.

> **Con 3 hijos, "Limit repetition to" tiene que quedar en 1.** Si lo subís a 2 queda
> un único candidato y la elección deja de ser al azar. Con 4 o más, 2 está bien —
> `RND_Gem_Pickup` tiene 4 y usa 2.

**Y la decisión que más importa de esta sección:** las pisadas casi no tienen
configuración en Wwise (sólo Shuffle y un randomizer de ±1 dB) porque **el trabajo
está hecho en los archivos**. Se limpiaron antes de importarlos: mono, pasa-altos,
recorte y normalización. Si el asset está bien, el motor no necesita corregirlo.

### Ambiente · `SW_Zone_Ambience` → Switch Container en **Continuous**

El Event se llama una vez y el contenedor **queda escuchando** los cambios de `Zone`.
En Step no pasaría nada hasta el próximo Play, y no hay próximo Play.

### Clima · `BLD_Field` → Blend Container en **Continuous**, con Blend Track

Un Blend reproduce **todos** sus hijos a la vez; el Blend Track con Crossfade los
cruza según `SunExposure`.

- Un Blend Track **sin Crossfade ni parámetro** es sólo un agrupador: todos los hijos
  suenan juntos ignorando el valor.
- Las zonas **tienen que solaparse**. Si sólo se tocan en el borde, no hay nada que
  cruzar: se oye un salto.
- Las zonas **tienen que cubrir todo el rango**. Un tramo sin capa es un tramo mudo.
- **En Blend, el Loop va en el HIJO.** El Blend no repite a sus hijos.

`BLD_Sun_High`, `BLD_Fauna` y `BLD_Altar` son **Blend agrupadores**: Continuous, sin
Blend Track propio, sólo para que sus hijos suenen juntos y se puedan mover como una
sola cosa.

### Fauna · `SW_Fauna` → Switch Container en **Continuous**, con Use Game Parameter

El switch `Silent` **está vacío a propósito**: con el sol alto los Random de fauna no
se instancian.

Ese es el patrón que hay que saber: **para apagar algo según un umbral se usa un
Switch (o un Blend) con el tramo vacío, no un RTPC de volumen a −200 dB.** Con el
Switch la instancia *no existe*; con el RTPC sigue viva, inaudible, gastando una voz
— y eso se ve en el Profiler.

### Intermitentes · `RND_Insects` y `RND_Mushrooms` → Random en **Continuous + Loop**

La plantilla del "ambiente intermitente", la que más puntos da si se explica entera:

1. Play Mode **Continuous** + **Loop Infinite** → el contenedor genera su cadencia.
2. **El Loop va en el CONTENEDOR.** En el hijo, el hijo no terminaría nunca y el
   contenedor jamás dispararía el siguiente.
3. Transition **Delay** con randomizer. El Delay cuenta **desde que termina** un
   elemento, así que nunca se pisan. El **Trigger Rate** es de inicio a inicio y
   sirve para cadencias regulares.
4. **Objetos Silence** (source *Wwise Silence*) con Duration y Random Min/Max.
5. **Weights** para que el silencio sea más probable que el evento.
6. **Randomizers de Pitch, Volume y Low-pass** en el contenedor.

> **Poco obvio:** acá el Random está en **Standard** con **"Limit repetition"
> desactivado**. Con Shuffle, o con el límite activo, el silencio no podría salir dos
> veces seguidas y **los weights quedarían sin efecto**. Shuffle es para variaciones
> equivalentes (pisadas, gemas); los weights son para cuando una opción tiene que ser
> mucho más probable que otra.

### Tensión por turnos · curvas RTPC sobre lo que ya suena

No hay ningún objeto nuevo. La tensión se construye modulando el ambiente existente:
el void sube de volumen y se le abre el Low-pass, la fauna baja de nivel y sus
intermitentes se espacian.

**Alargar el `Delay` con una curva RTPC** es la técnica de *Trigger Rate con RTPC*:
cambiás **cuándo vuelve** el sonido sin tocar el sonido. Acá usada al revés de lo
obvio — más lento es más vacío, y más vacío es más tenso.

> **Ojo con las propiedades aditivas.** Volume, Pitch, Lowpass y la Duration de la
> Transition toman la curva RTPC como un **offset que se suma al valor base**, no
> como el valor final. Por eso la curva del Delay va de 0 a +8 y no de 7 a 15.

---

## 4 · Switch, State, Blend y RTPC: cuál es cuál

| | Qué hace | Granularidad | Fuera de rango |
|---|---|---|---|
| **Switch** | elige **qué objeto suena** | por Game Object | la instancia **no existe** |
| **State** | **modifica propiedades** de lo que ya suena | **global** | no aplica: no crea ni destruye nada |
| **Blend** | cruza capas de forma continua | por Game Object | en Continuous **no existe** |
| **RTPC directo** | ajusta **una propiedad** de forma continua | por Game Object | **sigue sonando inaudible y gasta voz** |

En este proyecto están los cuatro, y cada uno responde una pregunta distinta:

- `Zone` (**Switch**) → *¿qué se oye en esta parte del mapa?*
- `Fauna` (**Switch**) → *¿los bichos existen o no?*
- `Recinto` (**State**) → *¿cómo suena el espacio?*
- `Sun` (**Blend**) → *¿cuánto sol hay?* (continuo, sin escalones)
- `Turns` (**RTPC**) → *¿cuánto aprieta el tiempo?*

La frase para tener lista: **el State es global y modifica propiedades; el Switch es
por Game Object y elige qué objeto suena. El State no crea ni destruye instancias.**

Y el argumento que distingue el trabajo: un Game Parameter lo consumen tres cosas —
Switch Group con *Use Game Parameter*, Blend Track, o curva RTPC. **Acá se usan las
tres.** Tener pocos RTPC no es debilidad: es que cada decisión está resuelta con el
mecanismo correcto.

---

## 5 · Convención de nombres

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
| Game Parameter, Switch Group, State Group | PascalCase sin prefijo | `SunExposure`, `Tile`, `Recinto` |
| Virtual Folder | nombre común | `Actions` |

Reglas que la acompañan:

1. El nombre dice **qué es y para qué sirve**, nunca de dónde salió el archivo.
2. Variaciones numeradas con **dos dígitos**, arrancando en `_01`.
3. **Un Work Unit por sistema** (`Player`, `Ambience`, `Interface`), no uno por
   entrega. Así dos personas pueden trabajar en paralelo sin pisarse.
4. **Comentario en todo contenedor que tome una decisión.** El campo *Comment* se
   guarda en el `.wwu` y es de donde sale el informe. En este proyecto ya están
   escritos: leelos, son la mitad de la documentación hecha.
5. Un Virtual Folder es **sólo organización visual**: no afecta ruteo ni propiedades.

---

## 6 · Trabajar de a varios sin romper nada

- **Wwise tiene que estar cerrado** si alguien edita los `.wwu` desde afuera. Si está
  abierto, al guardar pisa los archivos con su copia en memoria y revierte los
  cambios sin avisar.
- Los **caches por usuario** (`*.validationcache`, `*.wsettings`, `.cache/`) están en
  el `.gitignore`. Se regeneran solos y, si se versionan, generan conflictos de merge
  cada vez que alguien abre el proyecto.
- Antes de pushear, abrí el proyecto una vez y mirá el **Log** de Wwise.

---

## 7 · Las doce reglas de memoria

1. **Siempre decir el Play Mode.** Si no está especificado, el punto no vale.
2. **Step = el juego marca el ritmo. Continuous = el contenedor lo marca** (y habilita
   Loop y Transitions).
3. Un **Switch Container en Step no reacciona** al cambio de switch hasta el próximo
   Play.
4. En **Random/Sequence Continuous el Loop va en el contenedor**. En **Blend va en el
   hijo**.
5. Si el hijo loopea, **el Delay y el Trigger Rate no sirven**.
6. **Delay** cuenta desde que *termina* un elemento. **Trigger Rate** es de inicio a
   inicio: es el que define una cadencia y el que se pone bajo RTPC para acelerar
   algo sin tocarle el Pitch.
7. Las propiedades **aditivas** toman la curva RTPC como **offset sobre el valor
   base**, no como valor final.
8. Un **Blend Track sin Crossfade o sin parámetro es un agrupador**.
9. Las **zonas de un Blend Track se solapan** y **cubren todo el rango**.
10. En una curva de **Switch Group el eje Y es el índice del switch empezando en 1**,
    y el **0 es `None`** = silencio.
11. **Fuera de rango, el sonido no debe existir.** Switch o Blend en Continuous no
    instancian; un RTPC de volumen a −200 dB deja la voz viva.
12. La **misma curva RTPC en padre e hijo se suma**. Dejá una sola.

### Tres que son de señal, no de Wwise

- **El click**: la onda oscila alrededor del eje de amplitud 0, que es el parlante en
  reposo. Si el audio arranca o corta fuera del cruce por cero, el parlante salta de
  golpe y eso se oye como click. Un fade lleva la amplitud a 0.
- **El Voice Pitch de Wwise es velocidad de reproducción**: cambia altura **y
  duración**. +1200 cents = ×2 de velocidad = la mitad de duración.
- **Los graves decaen más lento.** Un sonido con la energía concentrada por debajo de
  los 120 Hz se percibe como retumbe o sala aunque no tenga nada de reverb. Por eso
  un pasa-altos puede sonar a "le sacaste el eco".
- **Un filtro puede dejar su propia cola, y el culpable suele ser el ORDEN, no la
  frecuencia.** Un Butterworth de 2º orden tiene Q = 0.707: está levemente
  sub-amortiguado, así que **sobrepasa y oscila** antes de asentarse. Si le pedís que
  remueva mucha energía (por ejemplo un pasa-altos sobre un sonido con el 70 % de su
  contenido en graves), esa oscilación queda audible como una vibración o un final
  "saturado", a una frecuencia cercana a la de corte. Mover el corte **no** lo
  arregla. Un filtro de **primer orden** tiene un solo polo real, su respuesta al
  impulso es una exponencial monótona y **no puede oscilar**: ese es el arreglo.
  Cómo detectarlo: mirá qué frecuencia domina la cola del archivo. Si coincide con tu
  frecuencia de corte, lo que estás escuchando es el filtro y no el sonido.
- **Un fade-in más largo que el ataque se come el ataque.** En un sonido percusivo el
  transitorio vive en los primeros milisegundos: un fade-in de 2 ms lo atenúa. Si el
  archivo ya arranca bien por debajo del pico, con 0.3 ms alcanza.
- **El ajuste de una reverb depende de qué le entra.** Una cola muy amortiguada sobre
  una fuente filtrada puede volverse inaudible, porque la reverb se come justo la
  banda que al sonido le quedó.
- **Mismo RMS no es misma sonoridad.** El RMS trata todas las frecuencias por igual y
  no mira cuánto dura el sonido; el oído hace las dos cosas. Un sonido corto y grave
  se percibe mucho más bajo que uno largo y brillante con idéntico RMS — acá fueron
  **8.5 dB** de diferencia. Para emparejar lo que se escucha se mide en **LUFS**
  (ITU-R BS.1770), que aplica un filtro que aproxima la sensibilidad del oído e
  integra sobre una ventana fija.