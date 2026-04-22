# Infinity Runner: runtime actual

Este documento describe la arquitectura activa de `Assets/_Game` despues del recorte del prototipo anterior.

La idea de esta version es simple:

- escena authorada a mano,
- referencias explicitas en inspector,
- bloques generados desde prefabs,
- hazards marcados de forma clara,
- menu y HUD separados en scripts chicos,
- y sin autowiring ni construccion de escena por codigo.

## 1. Flujo general

### `GameCoordinator`

Archivo: `Scripts/Runtime/Gameplay/GameCoordinator.cs`

Es el orquestador principal. Maneja estos estados:

- `Menu`
- `TransitionToRun`
- `Running`
- `GameOver`
- `TransitionToMenu`

Responsabilidades:

- entrar al menu al iniciar la escena,
- arrancar la run desde el menu,
- reiniciar con `R` despues de perder,
- volver al menu desde UI,
- conectar config, player, score, input, camara y generador,
- resolver contactos con `Person`, `Destructible` y `Death`,
- y disparar `GameOver`.

No hace `Find`, no crea componentes y no arma escena.

## 2. Player y controles

### `PlayerRunnerController`

Archivo: `Scripts/Runtime/Gameplay/PlayerRunnerController.cs`

Responsabilidades:

- cambio entre 3 carriles,
- salto scriptado,
- fast fall en el aire,
- rotacion visual de la roca,
- reenviar colisiones y triggers al coordinador.

La roca queda cerca del origen y el mundo se mueve hacia atras.

### `RunnerInputReader`

Archivo: `Scripts/Runtime/Gameplay/RunnerInputReader.cs`

Teclado:

- `A` / `Left`: carril izquierdo
- `D` / `Right`: carril derecho
- `W` / `Up`: salto
- `Space` / `Enter`: salto
- `S` / `Down`: fast fall
- `R`: reinicio desde `GameOver`

Touch:

- swipe horizontal para carril,
- tap para salto.

### `RunnerConfig`

Archivo: `Scripts/Runtime/Config/RunnerConfig.cs`

Variables importantes:

- `laneSpacing`
- `laneChangeDuration`
- `jumpVelocity`
- `gravity`
- `fastFallVelocity`
- `spawnAheadDistance`
- `despawnBehindDistance`

## 3. Score y HUD

### `RunnerScore`

Archivo: `Scripts/Runtime/Gameplay/RunnerScore.cs`

Guarda:

- distancia recorrida,
- score total,
- bonus por personas,
- bonus por destruibles.

Regla actual:

- el score solo crece por interacciones,
- la distancia se acumula aparte,
- y `metersPerSpeedUnit` define cuantos metros suma el runner por cada unidad de velocidad del mundo.

### `GameUI`

Archivo: `Scripts/Runtime/UI/GameUI.cs`

UI runtime minima:

- actualiza un `TMP_Text` para score,
- actualiza un `TMP_Text` para distancia mostrada en millas,
- muestra HUD durante gameplay,
- muestra panel de game over,
- expone `ReturnToMenuButton()` para enlazar al boton de volver al menu.

## 4. Menu e intro

### `MainMenuUI`

Archivo: `Scripts/Runtime/UI/MainMenuUI.cs`

Expone funciones pensadas para botones de Unity UI:

- `StartGameButton()`
- `ExitGameButton()`
- `OpenHowToButton()`
- `CloseHowToButton()`

Tambien maneja:

- `menuRoot`
- `howToPanel`
- `blackScreen` con `CanvasGroup`

### `RunnerCameraController`

Archivo: `Scripts/Runtime/Gameplay/RunnerCameraController.cs`

Tiene dos poses:

- menu
- runner

Puede authorarse de dos formas:

- con pivot de menu (`menuPositionPivot`) y target de menu (`menuLookTarget`),
- o con pivots opcionales para gameplay (`runnerPositionPivot`, `runnerLookTarget`) mas offsets como fallback del runner.

Y soporta:

- `SnapToMenu()`
- `SnapToRunner()`
- `TransitionToRunner(duration)`

La transicion de inicio usa blend de camara mientras la run empieza a acelerar.

Curvas editables en inspector:

- `positionTransitionCurve`
- `rotationTransitionCurve`

La posicion y la rotacion se evaluan por separado para poder ajustar mejor el feeling de entrada.

Regla de tiempo:

- el eje `X` de cada curva se interpreta como segundos reales,
- el eje `Y` se usa como valor de blend,
- y la transicion total dura lo que marque la curva mas larga.

`startTransitionDuration` queda como fallback si una curva no tiene claves validas.

## 5. Generacion por prefabs

### `BlockDefinition`

Archivo: `Scripts/Runtime/Config/BlockDefinition.cs`

Cada definicion tiene:

- `prefab`
- `allowedStages`
- `weight`

### `BlockMetadata`

Archivo: `Scripts/Runtime/Generation/BlockMetadata.cs`

Cada prefab de bloque debe tener este componente.

Campo clave:

- `length`

### `WorldGenerator`

Archivo: `Scripts/Runtime/Generation/WorldGenerator.cs`

Responsabilidades:

- mantener `worldRoot` fijo,
- mover los bloques activos hacia atras,
- mantener bloques por delante,
- reciclar bloques con pooling basico,
- elegir bloques validos por stage,
- cambiar stage segun cantidad de bloques generados,
- soportar preview de menu con bloque inicial,
- y escalar velocidad durante la intro.

Metodos importantes:

- `PrepareMenuPreview()`
- `BeginRunFromPreview(initialSpeedScale)`
- `RestartGameplay(initialSpeedScale)`
- `StopRun()`
- `SetSpeedScale(normalizedScale)`

### Bloque inicial

`initialBlockDefinition` es opcional.

Ese bloque:

- aparece al entrar al menu y al reiniciar la partida,
- sirve para la intro visual,
- no suma progreso de dificultad,
- no entra al pool normal,
- y desaparece cuando queda atras del jugador.

### `PowerUpSpawnPoint`

Archivo: `Scripts/Runtime/Generation/PowerUpSpawnPoint.cs`

Es un placeholder authorado dentro del prefab del bloque.

Cada punto puede definir:

- `spawnAnchor`
- `allowedTypes`

El bloque guarda el punto de aparicion, no el powerup en si.

## 6. Stages

### `DifficultyStageConfig`

Archivo: `Scripts/Runtime/Config/DifficultyStageConfig.cs`

Cada stage define:

- `stage`
- `speedMetersPerSecond`
- `startAfterBlockCount`

La progresion ya no depende de clash.

## 7. Colisiones y hazards

### Regla general

Un objeto mata al jugador si cumple una de estas condiciones:

- tiene `RunnerInteractable` con `interactionType = Death`,
- esta en la tag `Death`,
- esta en la layer `Death`.

### `RunnerInteractable`

Archivo: `Scripts/Runtime/Interaction/RunnerInteractable.cs`

Tipos actuales:

- `Death`
- `Person`
- `Destructible`
- `PowerUp`

### `RunnerCollisionUtility`

Archivo: `Scripts/Runtime/Gameplay/RunnerCollisionUtility.cs`

Ayuda estatica para:

- encontrar `RunnerInteractable` en la jerarquia del collider,
- decidir si algo cuenta como `Death`.

## 8. Power ups

### `PowerUpDefinition`

Archivo: `Scripts/Runtime/Config/PowerUpDefinition.cs`

Cada definicion expone:

- `type`
- `durationSeconds`
- `spawnWeight`
- `pickupPrefab`
- `scoreMultiplierValue`

### `PowerUpPickup`

Archivo: `Scripts/Runtime/Interaction/PowerUpPickup.cs`

Es el recogible real. Lleva la definicion aplicada en runtime y se destruye al recogerse o al limpiarse el bloque reciclado.

### Flujo de spawn

El spawn lo decide `GameCoordinator`.

Regla actual:

- el intento de powerup se programa por cantidad de bloques,
- solo se intenta al spawnear un bloque nuevo,
- el bloque recien generado es el unico candidato,
- si ese bloque no tiene un placeholder valido, el intento queda pendiente para el siguiente bloque compatible,
- y al reciclar un bloque se limpian todos los pickups que hubiese generado.

Powerups implementados en este corte:

- `InvincibleRock`: evita `GameOver` por hazards mientras dura.
- `ScoreMultiplier`: multiplica los puntos ganados por interacciones mientras dura.

## 9. Humanos

### `HumanPickup`

Archivo: `Scripts/Runtime/Interaction/HumanPickup.cs`

Version actual:

- avanza hacia adelante a velocidad fija,
- se aleja de la roca,
- se queda quieto si detecta un hazard `Death` delante,
- al tocar la roca da puntos y desaparece.

## 10. Obstaculos especiales

### `DistanceTriggeredObstacle`

Archivo: `Scripts/Runtime/Gameplay/DistanceTriggeredObstacle.cs`

Base comun para eventos activados por distancia y velocidad.

Formula:

- distancia de trigger = `speed * triggerLeadTime`

Con clamp por:

- `minTriggerDistance`
- `maxTriggerDistance`

Tambien dibuja gizmos de preview usando:

- `gizmoPreviewSpeed`
- `gizmoSphereRadius`
- `gizmoColor`

### Implementaciones actuales

#### `FallingPillarObstacle`

- telegraph,
- caida,
- cambio de collider vertical a collider caido.

#### `DynamicCartObstacle`

- telegraph corto,
- lee el carril actual del jugador,
- cruza al carril objetivo.

#### `CatapultObstacle`

- telegraph,
- puede disparar un `Animator` por trigger,
- usa `fireTriggerName`,
- instancia un proyectil simple,
- y la propia catapulta puede seguir siendo `Death`.

#### `MovingHazardProjectile`

- movimiento lineal,
- autodestruccion por tiempo.

## 11. Wiring minimo de escena

La escena deberia tener, como minimo:

- `GameCoordinator`
- `RunnerScore`
- `RunnerInputReader`
- `WorldGenerator`
- `PlayerRunnerController`
- `RunnerCameraController`
- `MainMenuUI`
- `GameUI`
- cero o mas `PowerUpDefinition`
- un `RunnerConfig`
- uno o mas `DifficultyStageConfig`
- uno o mas `BlockDefinition`

Referencias importantes:

- en `GameCoordinator`:
  - `config`
  - `player`
  - `worldGenerator`
  - `score`
  - `inputReader`
  - `cameraController`
  - `mainMenuUI`
  - `gameUI`
  - opcional `powerUpDefinitions`
  - `minBlocksBetweenPowerUpAttempts`
  - `maxBlocksBetweenPowerUpAttempts`
- en `WorldGenerator`:
  - `config`
  - `difficultyStages`
  - `blockDefinitions`
  - opcional `initialBlockDefinition`
  - opcional `worldRoot`
- en `PlayerRunnerController`:
  - `config`
  - opcional `visualRoot`
- en `GameUI`:
  - `scoreText`
  - `milesText`
  - `hudRoot`
  - `gameOverRoot`
- en `MainMenuUI`:
  - `menuRoot`
  - `howToPanel`
  - `blackScreen`

## 12. Que salio del codigo activo

Sigue fuera del runtime principal:

- clash,
- rampa divina,
- builder/editor que fabricaba escenas,
- autowiring por busqueda global,
- y cualquier logica generica para tapar referencias faltantes.

## 13. Siguiente paso razonable

La base ya esta lista para iterar por inspector y prefabs reales.

Los siguientes pasos naturales son:

1. cablear la UI de menu y HUD en la escena,
2. authorar varios bloques reales con hazards,
3. probar humanos y obstaculos especiales en contexto,
4. y recien despues sumar eventos mas complejos.
