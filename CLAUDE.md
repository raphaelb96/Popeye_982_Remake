# Popeye 1982 Remake — CLAUDE.md

Projet de cours IAC. Remake 2.5D arcade Popeye (1982). Unity 6 (6000.2.8f1), physique 3D + sprites 2D billboard.
Équipe : **(R) Raphael** (tech art + dev, c'est lui qui code/assigne), **(T) Rotem** (art/sprites), **(M) Michael** (gameplay tuning + audio/VFX + game manager).
Scène active : `Assets/Scenes/Popeye 6.3.26.unity`

> **RÈGLE ABSOLUE** : Ne JAMAIS modifier un fichier sans accord explicite de Raphael. On indique le fichier + ligne + le changement exact, on explique le bug simplement (Raphael aime les explications claires et pas trop longues), et on attend qu'il dise "fais-le"/"applique" pour modifier. Sinon c'est LUI qui modifie. Commentaires de code TOUJOURS en anglais.
> **Workflow bugs** : on les passe un par un. Raphael teste après chaque fix, si rien n'est cassé on continue.
> **Toujours tenir ce CLAUDE.md à jour** après chaque tâche terminée.

---

## 1. Structure du projet

```
Assets/
  Scripts/
    Managers/  GameManager, InputManager, AudioManager, PauseManager, VFXManager
    Player/    PopeyeController, BlutoController, MeleeHitbox, Billboard
               (PopeyeHitbox.cs SUPPRIMÉ — était legacy)
    NPCs/      OliveController, SeaHagController, SeaHagProjectile
    Items/     BottleItem, HeartItem, SpinachItem
    Systems/   LadderTrigger, OneWayPlatform3D, DeathZone, ScreenWrapTrigger
    UI/        UIManager, FloatingText
  Scenes/Popeye 6.3.26.unity     ← scène active
  Art/         sprites + animations (voir section 5)
  Player/      Popeye_animation_clip, Bluto_animation_clip, Animator Controllers
```

**Tags** : `Player1`=Popeye, `Player2`=Bluto, `Item`=hearts/bottles/spinach
**Contrôles** : Popeye = WASD + C (punch) | Bluto = Flèches + Entrée (punch) + Shift droit (throw bouteille) | Pause = Escape/P | UI confirm = Espace/Entrée
**Victoire** : Popeye = 24 cœurs collectés | Bluto = Popeye à 0 HP (3 HP au départ)
**Démarrage** : `GameManager` attend 1s puis fire `OnGameStart` → tous les controllers/NPCs acceptent l'input

---

## 2. Résumé détaillé de chaque script

### Managers/GameManager.cs (singleton `GameManager.Instance`)
- État : `popeyeHearts` (0→24), `popeyeHP` (3→0), `MAX_HEARTS=24`
- Events statiques : `OnGameStart`, `OnDamageTaken`, `OnGameOver(bool popeyeWins)`
- `AddHeart()` : +1 cœur, win si ≥24. `TakeDamage()` : −1 HP, fire OnDamageTaken, lose si ≤0
- `EndGame(bool)` : `Time.timeScale=0`, fire OnGameOver, attend input pour reload scène
- `StartRoundRoutine()` : attend 1s puis fire OnGameStart

### Managers/InputManager.cs (singleton `InputManager.Instance`)
- New Input System, bindings codés en dur dans `SetupInputs()`
- Propriétés lues par les controllers : `PopeyeMove`/`BlutoMove` (Vector2), `PopeyeJumpDown`, `PopeyePunchDown`, `BlutoThrowDown`, `PauseDown`, `UIConfirmDown`, etc. (toutes en `WasPressedThisFrame`)

### Managers/AudioManager.cs
- Souscrit à TOUS les events (GameManager, items, controllers, Olive) pour jouer SFX/musique
- Musique gameplay aléatoire par match, musique spinach 10s, win/lose jingles
- `PlayWithRandomPitch()` pour punch/walk (évite répétition). Clips à assigner dans Inspector (Michael)

### Managers/PauseManager.cs
- `canPause` activé par OnGameStart (via méthode nommée `EnablePause` — FIX bug #9, plus de lambda)
- Toggle pause sur Escape/P : `Time.timeScale` 0/1 + panel on/off
- `QuitGame()` pour bouton UI

### Managers/VFXManager.cs
- Camera shake sur `BlutoController.OnHeavyPunch`
- `SpawnFloatingText(worldPos, text)` instancie le prefab FloatingText (POW!/BAM!)
- ⚠️ PAS singleton → `MeleeHitbox` le trouve via `FindFirstObjectByType` (perf à améliorer un jour)

### Player/PopeyeController.cs `[RequireComponent Rigidbody, CapsuleCollider, Animator]`
- Move : `moveSpeed=5`, `jumpForce=8.5`. Freeze Z + rotations. Flip via localScale.x
- **`[Header("Stun Durations")] bottleStunDuration=2f`** (exposé Inspector — FIX bug #4)
- États : isGrounded, isClimbing, isStunned, isInvincible, canMove
- `TakeDamage(int amount, Vector3 knockback)` : −1 HP via GameManager + knockback + stun 0.5s + anim Hit. **Bloqué si isInvincible/isStunned**
- `ApplyStun(float)` : stun + anim, bloqué si déjà stunné (pas de stacking) ou invincible
- `ActivateSpinachMode()` : isInvincible=true, moveSpeed×2, 10s puis désactive
- `EnableHitbox()`/`DisableHitbox()` appelés par Animation Events
- Events audio : OnPunch, OnJump, OnHit, OnWalk

### Player/BlutoController.cs `[RequireComponent Rigidbody, CapsuleCollider, Animator]`
- Move : `moveSpeed=4`, `jumpForce=8`. Inventaire bouteilles : `currentBottles=5`, `maxBottles=5`
- **`[Header("Stun Durations")]`** (exposés Inspector — FIX bugs #2/#3/#8) :
  - `normalStunDuration=1f`, `spinachStunDuration=5f`, `punchCooldown=1f`
- `punchTimer` décrémenté dans Update, bloque le punch tant que >0 (FIX bug #8)
- `SpawnBottleProjectile()` : appelé par Animation Event sur la clip Throw, −1 bouteille, instancie projectile
- `AddBottle()` : +1 si <max. `ApplyStun(float)` : bloqué si déjà stunné
- ⚠️ `ApplySpinachStun()` SUPPRIMÉE (FIX bug #2) — on utilise `ApplyStun(spinachStunDuration)` depuis MeleeHitbox
- Events : OnHeavyPunch, OnThrowBottle, OnBottleCountChanged, OnJump, OnHit, OnBottleCollected, OnWalk

### Player/MeleeHitbox.cs `[RequireComponent BoxCollider]`
- Hitbox réutilisable (Popeye ET Bluto), config Inspector : `targetTag`, `canDestroyProjectiles`, `hitText`
- Démarre désactivé, activé par Animation Events (EnableHitbox frame 5 / DisableHitbox frame 10)
- `OnTriggerEnter` cases :
  - **Case 1 (targetTag=Player1)** : Bluto frappe Popeye → `popeye.TakeDamage(damageAmount, knockbackDir*8f)` (FIX bug #1, calcule knockback opposé au parent)
  - **Case 1b (targetTag=Player2)** : Popeye frappe Bluto → si isInvincible `ApplyStun(spinachStunDuration)`, sinon `ApplyStun(normalStunDuration)` (FIX bugs #2/#3)
  - Case 2 : détruit BottleItem si canDestroyProjectiles
  - Case 3 : détruit SeaHagProjectile si canDestroyProjectiles
  - Case 4 : Bluto punch spinach → `DestroyByBluto()`
  - **Case 4b : Bluto punch heart → `Destroy(other.gameObject)`** (FIX bug #7)
  - Case 5 : Popeye punch ladder → `DisableLadder()`
- Spawn FloatingText via VFXManager au point de contact

### Player/Billboard.cs
- `LateUpdate` : copie la rotation caméra pour que les sprites face-caméra (2.5D). `offsetRotation` ajustable

### NPCs/OliveController.cs
- Patrouille top tier : `moveSpeed=2`, bounds ±8, alterne walk/idle aléatoirement
- **Heart spawn timer aléatoire (FIX bug #5)** : `[Header("Heart Spawn Timer")] minHeartTime=1.5f, maxHeartTime=2.5f`, `nextHeartTime` regénéré (UnityEngine.Random) à chaque spawn
- `SpawnHeart()` : instancie heart, anim "Heart", tous les 2 hearts spawn bottle, tous les 5 spawn spinach
- Prefabs + spawnPoints à assigner Inspector. Events : OnThrowHeart, OnWalk

### NPCs/SeaHagController.cs + SeaHagProjectile.cs
- SeaHag apparaît off-screen (left/right aléatoire) toutes les 5-12s, tire un projectile
- Projectile : `speed=6`, traverse l'écran, stun 1s les DEUX joueurs (hazard), self-destroy à X=±15
- `Smash()` détruit le projectile (punch Popeye)

### Items/BottleItem.cs
- Dual-mode : pickup (Bluto walk dessus → AddBottle) OU projectile (lancé → stun Popeye)
- **Projectile stun Popeye `bottleStunDuration` (FIX bug #4, 2s)**, fire OnBottleSmashed
- `InitializePickup()` / `InitializeProjectile(dir)`. `Smash()` détruit + event

### Items/HeartItem.cs
- Tombe (`fallSpeed=1`) + flutter sinusoïdal (`flutterSpeed=2`, `flutterMagnitude=0.5`)
- **Dérive horizontale aléatoire (FIX bug #6)** : `horizontalSpeed = UnityEngine.Random.Range(-1.5f,1.5f)` dans Start, `startX += horizontalSpeed*Time.deltaTime` dans Update
- Collecté par Player1 → `GameManager.AddHeart()` + OnHeartCollected. Self-destroy si Y<−10

### Items/SpinachItem.cs
- Collecté par Player1 → fire OnSpinachEaten (active buff + détruit toutes les autres spinach via ClearFromBoard)
- `DestroyByBluto()` : Bluto punch détruit sans déclencher le buff
- Limite 1 seul actif déjà gérée (isCollected + isInvincible guard dans PopeyeController)

### Systems/
- **LadderTrigger** : OnTriggerStay attache au ladder si press up/down (dead zone 0.1). Popeye punch → `DisableLadder()` 3s
- **OneWayPlatform3D** : FixedUpdate gère pass-through (feet sous surface, tolérance 0.2f) ou collision. `FallThrough(col)` 0.4s pour drop volontaire (S/↓)
- **DeathZone** : respawn sur Transform configuré (`popeyeRespawn`/`blutoRespawn`), fallback (0,2,0)/(3,2,0). Zero velocity avant teleport
- **ScreenWrapTrigger** : teleporte à la paire opposée, cooldown 0.5s sur destination (anti bounce-back)

### UI/
- **UIManager** : bottleIcons[5] + heartIcons[3] (show/hide selon count), victory panels, gameOverPanel, TMPro textes. `UpdateHUD()` sur events
- **FloatingText** : flotte vers le haut `floatSpeed=2`, self-destroy après `destroyTime=1`. `SetText(string)`

---

## 3. Les 10 bugs — TOUS CORRIGÉS ✅ (commit fait)

| # | Fichier | Bug | Fix appliqué |
|---|---------|-----|--------------|
| 1 | MeleeHitbox.cs | Bluto punch réduit HP sans stun/knockback | Appelle `popeye.TakeDamage(amount, knockbackDir*8f)` |
| 2 | MeleeHitbox.cs | Popeye punch sans épinards = rien à Bluto | Ajout `else ApplyStun(normalStunDuration)` |
| 3 | BlutoController.cs | Spinach stun 10s au lieu de 5s | `spinachStunDuration=5f` (Inspector) |
| 4 | BottleItem.cs | Bottle stun 0.5s au lieu de 2s | `bottleStunDuration=2f` (Inspector sur Popeye) |
| 5 | OliveController.cs | Cœurs toutes les 2s fixes | Timer aléatoire 1.5-2.5s (min/maxHeartTime Inspector) |
| 6 | HeartItem.cs | Cœurs tombent en ligne droite | Dérive horizontale aléatoire ±1.5 |
| 7 | MeleeHitbox.cs | Bluto ne détruit pas les cœurs | Case 4b `Destroy(heart)` |
| 8 | BlutoController.cs | Pas de cooldown punch Bluto | `punchCooldown=1f` + punchTimer (Inspector) |
| 9 | PauseManager.cs | Lambda unsubscription cassée (fuite mémoire) | Méthode nommée `EnablePause()` |
| 10 | PopeyeHitbox.cs | Fichier legacy inutilisé | Fichier supprimé |

> Note : durées de stun + cooldown exposées dans l'Inspector (Popeye et Bluto GameObjects) → Michael peut régler en Play Mode sans toucher au code.

---

## 4. TÂCHES SCRIPT RESTANTES (Raphael) — pas encore faites

| Tâche | Fichier concerné | Approche suggérée |
|-------|------------------|-------------------|
| Bluto's jump heavier | BlutoController.cs | Baisser jumpForce OU augmenter gravité Rigidbody. Exposer Inspector |
| Bluto plus rapide que Popeye pendant Spinach | BlutoController + PopeyeController | Pendant spinach (Popeye×2=10), booster Bluto temporairement >10 |
| Knockback massif sur Bluto punch + Popeye spinach punch | MeleeHitbox.cs | Exposer force knockback Inspector, valeur différente selon spinach |
| Bluto punch hitbox = massive | Scène (BoxCollider de la hitbox) | Agrandir le BoxCollider enfant de Bluto (pas du code) |
| Popeye's punch stuns do NOT stack | Popeye/Bluto Controller | Déjà géré par guard `isStunned` dans ApplyStun — À VÉRIFIER en test |
| Remove drop "down" sur plateforme du bas | OneWayPlatform3D ou tag plateforme | Ajouter flag `allowDropThrough` sur la plateforme du bas = false |
| Couleur rougeâtre-rose sur Popeye en Spinach | PopeyeController.cs (ActivateSpinachMode) | Modifier SpriteRenderer.color, restaurer à DeactivateSpinachMode |

---

## 5. ASSETS ART — état

### ✅ Déjà intégrés dans Unity
Popeye walk (fait ce soir, sprites dans `Art/PLACEHOLDER (2 FPS)/NEW POPEYE WALK (REPLACEMENT)/`, clip `Player/Popeye_animation_clip` état Walk, 2 FPS), Popeye Idle, Bluto walk, Bluto Idle.

### 🔄 PNG prêts dans Assets/Art — Raphael doit : convertir PNG en Sprite (Inspector → Texture Type = Sprite 2D and UI → Apply) PUIS assigner dans l'anim/prefab
- Flying bottle sprite → remplacer anim `Art/Animations/Flying Bottle/`
- Heart sprite → remplacer anim `Art/Animations/Heart Flutter/`
- Spinach can sprite → prefab Spinach
- POW / BAM sprites → prefab FloatingText / VFXManager
- Popeye hurt → peut faire clignoter (voir si script ou anim)
- Popeye eat spinach → animation
- Bluto HeavyPunch → animation (frames existent : `Art/Animations/Bluto Punch/` 6 frames)
- Bluto ThrowBottle → animation
- Background seaport → remplacer dans la scène (actuel `Art/Sprites/Background.PNG`)

### ❓ À confirmer avec Rotem (existe ou pas ?)
Popeye Climb 1/2, Bluto Climb 1/2, Bluto Stunned 1/2, Bluto Hurt, Sea Hag sprite + animations

### Inventaire sprites/anims existants (Assets/Art)
- `Animations/` : Bluto Punch (6f), Bottle Idle (9f), Flying Bottle (5f), Heart Flutter (2f), Popeye Normal Punch Hit (4f)/Miss (3f), Popeye Spinach Punch (4f), Popeye Spinach Walk (2f), OliveWalk.anim, NPCOlive.controller, HeartSpawn.anim, BasketWave.anim, PotWave.anim
- `PLACEHOLDER (2 FPS)/` : Bluto Normal Walk (2f), Bluto Scared Walk (2f), Olive Normal Walk (2f), Olive Throw Heart (1f), NEW POPEYE WALK (2f)
- `Sprites/` : Background, Bluto Jump, bottle_sprite, Popeye Eat Spinach, Popeye hurt, Popeye Normal/Spinach Jump, Victory Screens (Popeye + Bluto), Icons/

---

## 6. À FAIRE par les autres (pas Raphael)

**Michael (gameplay tuning + audio/VFX)** :
- Régler tous les timers/stuns/cooldown dans l'Inspector en Play Mode
- Olive's heart SFX → random pitch (anti-répétition)
- Assigner les clips audio dans AudioManager
- Assigner caméra + FloatingText prefab dans VFXManager
- Assigner PauseManager panel, SeaHagController GameObject

**Rotem (art)** : tous les sprites/animations marqués ❓ ci-dessus

---

## 7. Notes techniques utiles
- `PopeyeController.TakeDamage(amount, knockback)` = la VRAIE méthode (HP+stun+knockback). NE PAS appeler `GameManager.TakeDamage()` seul.
- `ApplyStun()` (Popeye et Bluto) vérifie déjà `isStunned` → pas de stacking naturel.
- Spinach : `isInvincible=true` protège déjà Popeye des dégâts pendant 10s (résistance OK).
- One-way platform : tolérance 0.2f dans `HandlePlayerCollision()` — si clignotement, ajuster.
- Screen wrap : cooldown 0.5s sur destination.
- Attention `Random` ambigu : toujours `UnityEngine.Random` (sinon erreur compile avec System).
- Animation Events : noms exacts `EnableHitbox`/`DisableHitbox`/`SpawnBottleProjectile` doivent matcher les méthodes.
- Pour remplacer sprite dans une anim : ouvrir la clip dans fenêtre Animation, cliquer chaque keyframe (diamant), réassigner le sprite dans l'Inspector. Vérifier Sample Rate.

---

## 8. Git
- Commit fait pour les 10 bugs. Message court (≤72 car) : `Fix 10 gameplay bugs: combat, stun, Olive timer, hearts, cleanup`
- Outil : GitKraken
