# Popeye 1982 Remake — CLAUDE.md

Projet de cours IAC. Remake 2.5D arcade Popeye (1982). Unity, physique 3D, sprites 2D billboard.  
Équipe : **(R) Raphael**, **(T) Rotem**, **(M) Michael**  
Scène active : `Assets/Scenes/Popeye 6.3.26.unity`

> **RÈGLE** : Ne JAMAIS modifier un fichier sans accord explicite de Raphael. Indiquer le changement, attendre confirmation.

---

## Architecture rapide

```
Scripts/Managers/   → GameManager (singleton, events), InputManager (singleton), AudioManager, PauseManager, VFXManager
Scripts/Player/     → PopeyeController, BlutoController, MeleeHitbox, Billboard
                      PopeyeHitbox.cs = LEGACY, à supprimer
Scripts/NPCs/       → OliveController, SeaHagController, SeaHagProjectile
Scripts/Items/      → BottleItem (pickup + projectile), HeartItem, SpinachItem
Scripts/Systems/    → LadderTrigger, OneWayPlatform3D, DeathZone, ScreenWrapTrigger
Scripts/UI/         → UIManager, FloatingText
```

**Tags** : `Player1` (Popeye), `Player2` (Bluto), `Item` (hearts/bottles/spinach)  
**Contrôles** : Popeye = WASD + C (punch) | Bluto = Flèches + Entrée (punch) + Shift droit (throw)  
**Victoire** : Popeye = 24 cœurs | Bluto = Popeye à 0 HP  
**GameStart** : Délai 1s puis `OnGameStart` event → tous les controllers acceptent l'input

---

## Bugs identifiés (10 bugs — session en cours)

| # | Statut | Fichier : ligne | Problème | Fix |
|---|--------|-----------------|----------|-----|
| 1 | ✅ | `MeleeHitbox.cs:65` | Bluto punch réduit HP mais **pas de stun ni knockback** sur Popeye | Appeler `other.GetComponent<PopeyeController>().TakeDamage(1, knockbackDir * force)` |
| 2 | ✅ | `MeleeHitbox.cs:72` | Popeye punch sans épinards = **rien** à Bluto | Stun 1s ajouté + durées exposées dans Inspector |
| 3 | ✅ | `MeleeHitbox.cs:79` | Spinach punch stun = **10s** (devrait être 5s) | Réglé via `spinachStunDuration = 5f` dans Inspector |
| 4 | ✅ | `BottleItem.cs:68` | Stun bouteille = **0.5s** (devrait être 2s) | `bottleStunDuration = 2f` exposé dans Inspector sur Popeye |
| 5 | ✅ | `OliveController.cs:136` | Cœurs toutes les **2s fixes** (devrait être 1.5–2.5s aléatoire) | Timer aléatoire + `minHeartTime/maxHeartTime` dans Inspector |
| 6 | ✅ | `HeartItem.cs` | Cœurs tombent **en ligne droite** (seulement flutter sin) | Dérive horizontale aléatoire ajoutée |
| 7 | ✅ | `MeleeHitbox.cs` | Bluto **ne détruit pas les cœurs** avec son punch | Case 4b ajouté pour `HeartItem` |
| 8 | ✅ | `BlutoController.cs` | **Pas de cooldown** sur punch Bluto (devrait être 1s) | `punchCooldown = 1f` ajouté + exposé dans Inspector |
| 9 | ✅ | `PauseManager.cs:25` | Lambda unsubscription **ne fonctionne pas** → fuite mémoire | Remplacé par méthode nommée `EnablePause()` |
| 10 | ✅ | `PopeyeHitbox.cs` | Fichier **legacy inutilisé**, risque de conflit | Fichier supprimé |

Légende : ⏳ à faire | ✅ corrigé et testé | ❌ abandonné

---

## Ce qui n'est PAS encore fait (checklist non barrée)

**Gameplay** (section 5 du .docx) :
- Knockback massif sur punch Bluto et Spinach-punch
- Hitbox punch Bluto = massive
- Bluto plus rapide que Popeye pendant spinach
- Drop désactivé sur plateforme du bas
- Olive timer aléatoire ← bug #5
- Cœurs trajectoire variée ← bug #6
- Bluto détruit Hearts+Spinach ← bug #7 (Hearts)
- Cooldown punch Bluto ← bug #8
- Stun Bluto 1s par punch Popeye ← bug #2
- Spinach stun = 5s ← bug #3
- Bouteilles = 2s stun ← bug #4
- Couleur rougeâtre sur Popeye en spinach mode

**Art / Animations** (T = Rotem) : Idle, Climb, Jump/Fall pour Popeye+Bluto, HeartFlutter, HeavyPunch, Stunned, ThrowBottle, Hurt (Bluto)

**GFX** : Fonts, layers de profondeur, skybox port, éclairage par layer, portraits victory screens, instructions UI, VFX POW/BAM améliorés, plateformes plus épaisses, bouteilles/barils plus gros

**Wiring Inspector** (à assigner dans Unity) : AudioManager clips, VFXManager prefabs+caméra, PauseManager panel, SeaHagController gameobject

---

## Notes pour reprendre rapidement

- `PopeyeController.TakeDamage(int amount, Vector3 knockback)` gère stun + knockback + HP — Bluto punch doit appeler ça, pas directement `GameManager.TakeDamage()`
- `ApplyStun()` vérifie déjà `isStunned` → pas de stacking naturel
- Spinach : `isInvincible=true` protège déjà Popeye des dégâts pendant les 10s
- One-way platform : tolérance 0.2f dans `HandlePlayerCollision()` — si clignotement, ajuster
- Screen wrap : cooldown 0.5s sur trigger destination pour éviter bounce-back
- `VFXManager` n'est pas singleton → `FindFirstObjectByType` à chaque punch (perf)
