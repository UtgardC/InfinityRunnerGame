# Infinity Runner: arquitectura simplificada

Este documento describe el runtime actual de `Assets/_Game` después del recorte del prototipo anterior.

La idea de esta versión es simple:

- escena authorada a mano,
- referencias explícitas en inspector,
- bloques generados desde prefabs,
- obstáculos marcados de forma clara,
- y eventos especiales separados en scripts chicos.

Se eliminaron del flujo principal:

- menú,
- UI runtime,
- clash,
- power ups,
- builder/editor bootstrap,
- y cualquier autoconstrucción de escena por código.

## 1. Núcleo actual

### `GameCoordinator`

Archivo: `Scripts/Runtime/Gameplay/GameCoordinator.cs`

Responsabilidades:

- iniciar la run apenas arranca la escena,
- reiniciar con `R`,
- conectar config, player, score, input y generador,
- resolver contactos con `Person`, `Destructible` y `Death`,
- disparar `GameOver`.

Decisión importante:

- no hace `Find`,
- no crea componentes,
- no crea UI,
- no arma escena.

Si falta una referencia importante, loguea error y no arranca.

### `PlayerRunnerController`

Archivo: `Scripts/Runtime/Gameplay/PlayerRunnerController.cs`

Responsabilidades:

- cambio entre 3 carriles,
- salto scriptado,
- rotación visual de la roca,
- reenviar colisiones y triggers al coordinador.

La roca queda cerca del origen y el mundo se mueve hacia atrás.

### `RunnerInputReader`

Archivo: `Scripts/Runtime/Gameplay/RunnerInputReader.cs`

Responsabilidades:

- teclado:
  - `A` / `Left`
  - `D` / `Right`
  - `W` / `Up`
  - `Space` / `Enter`
  - `R`
- touch:
  - swipe horizontal para carril,
  - tap para salto.

### `RunnerScore`

Archivo: `Scripts/Runtime/Gameplay/RunnerScore.cs`

Responsabilidades:

- distancia,
- bonus por personas,
- bonus por destruibles.

## 2. Generación por prefabs

### `BlockDefinition`

Archivo: `Scripts/Runtime/Config/BlockDefinition.cs`

Cada definición tiene:

- `prefab`
- `allowedStages`
- `weight`

No hay reglas escondidas de clash, rampas, powerups ni combinaciones especiales.

### `BlockMetadata`

Archivo: `Scripts/Runtime/Generation/BlockMetadata.cs`

Cada prefab de bloque debe tener este componente.

Campo clave:

- `length`

Ese largo es el que usa el generador para encadenar y reciclar bloques.

### `WorldGenerator`

Archivo: `Scripts/Runtime/Generation/WorldGenerator.cs`

Responsabilidades:

- mover `worldRoot` hacia atrás,
- mantener `worldRoot` fijo para no acumular error de precisión,
- mantener bloques por delante,
- reciclar bloques con pooling básico,
- elegir bloques válidos por stage,
- cambiar stage según cantidad de bloques generados.

También soporta un bloque inicial opcional:

- `initialBlockDefinition`

Ese bloque:

- aparece una sola vez al comenzar la run,
- sirve para introducir el nivel,
- no suma progreso de dificultad,
- y no entra al pool normal al salir de pantalla.

La progresión ya no depende de clash.

Ahora cada `DifficultyStageConfig` define:

- `stage`
- `speedMetersPerSecond`
- `startAfterBlockCount`

Eso permite algo mucho más directo:

- bloque 0 a N: `Start`
- después: `Middle`
- después: `Late`

## 3. Cómo authorar bloques

Flujo recomendado:

1. Crear un prefab para el segmento.
2. Agregar `BlockMetadata`.
3. Ajustar `length`.
4. Decorarlo con meshes, colliders, personas y obstáculos.
5. Crear un `BlockDefinition` que apunte a ese prefab.
6. Agregar la definición al array de `WorldGenerator`.

## 4. Sistema de colisiones y hazards

### Regla general

Un objeto mata al jugador si cumple una de estas condiciones:

- tiene `RunnerInteractable` con `interactionType = Death`,
- está en la tag `Death`,
- está en la layer `Death`.

Se agregó la tag `Death` y también una layer `Death` al proyecto.

### `RunnerInteractable`

Archivo: `Scripts/Runtime/Interaction/RunnerInteractable.cs`

Tipos actuales:

- `Death`
- `Person`
- `Destructible`

Eso deja un flujo muy claro:

- `Person`: da puntos y desaparece.
- `Destructible`: da puntos y llama a `DestructibleProp` si existe.
- `Death`: termina la run.

### `RunnerCollisionUtility`

Archivo: `Scripts/Runtime/Gameplay/RunnerCollisionUtility.cs`

Es solo una ayuda estática para:

- encontrar un `RunnerInteractable` en la jerarquía del collider,
- decidir si algo cuenta como `Death`.

## 5. Humanos

### `HumanPickup`

Archivo: `Scripts/Runtime/Interaction/HumanPickup.cs`

Versión actual:

- avanza en `local +Z`,
- se aleja de la roca a velocidad fija,
- si un raycast hacia adelante detecta algo marcado como `Death`, se queda quieto.

No tiene animaciones ni bob obligatorio. Eso queda para más adelante.

## 6. Obstáculos especiales

La base común ahora es:

### `DistanceTriggeredObstacle`

Archivo: `Scripts/Runtime/Gameplay/DistanceTriggeredObstacle.cs`

Idea:

- cada obstáculo define un `triggerLeadTime`,
- el trigger real se calcula con la velocidad actual,
- así la animación empieza cuando el jugador está a la distancia correcta para que el evento llegue "justo".

La fórmula práctica es:

- distancia de trigger = `speed * triggerLeadTime`
- con clamp por `minTriggerDistance` y `maxTriggerDistance`

### Implementaciones actuales

#### `FallingPillarObstacle`

- espera `telegraphDuration`,
- cae durante `fallDuration`,
- cambia de collider vertical a collider caído.

#### `DynamicCartObstacle`

- telegraph corto,
- lee el carril actual del jugador,
- cruza hacia ese carril.

#### `CatapultObstacle`

- telegraph,
- anima el brazo,
- lanza un proyectil simple,
- la catapulta puede seguir siendo `Death` por collider/tag/layer.

#### `MovingHazardProjectile`

- mueve el proyectil en una dirección fija,
- se destruye solo por tiempo.

## 7. Escena mínima esperada

La escena debería tener, como mínimo:

- `GameCoordinator`
- `RunnerScore`
- `RunnerInputReader`
- `WorldGenerator`
- `PlayerRunnerController`
- `RunnerCameraController`
- un `RunnerConfig`
- uno o más `DifficultyStageConfig`
- uno o más `BlockDefinition`

Referencias importantes:

- en `GameCoordinator`:
  - `config`
  - `player`
  - `worldGenerator`
  - `score`
  - `inputReader`
  - opcional `cameraController`
- en `WorldGenerator`:
  - `config`
  - `difficultyStages`
  - `blockDefinitions`
  - opcional `worldRoot`
- en `PlayerRunnerController`:
  - `config`
  - opcional `visualRoot`

## 8. Qué se eliminó

Salió completamente del código activo:

- clash,
- power ups,
- rampa divina,
- UI generada por código,
- builder/editor que fabricaba escenas y prefabs,
- pooling interno de pickups runtime,
- autowiring por búsqueda global.

## 9. Siguiente paso razonable

La base ya quedó lista para iterar con prefabs authorados.

El próximo paso lógico es:

1. crear un set chico de bloques reales,
2. conectar los assets en la escena,
3. probar el loop básico de correr, saltar, esquivar y sumar puntos,
4. recién después reintroducir obstáculos más complejos uno por uno.
