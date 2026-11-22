# Scene 2 — Warteg Lunch & NPC Cast

## Goal

Build Level 2 as an open-front **Warung Tegal** with a seven-dish display, then add five set-dressing NPCs to the warteg and nine to the Flamboyant bazaar. Restore the campaign path `MainVR → Level02_Warteg → Level03_Bazaar`.

## Requirements

- Seven foods only: `nasi_putih`, `rendang`, `ayam_goreng`, `telur_ceplok`, `tempe`, `sayur_singkong`, `kerupuk`; no drink.
- Warteg layout: open front to the street, glass display counter with seven trays, inside benches, cashier/serve table, red-and-cream awning, readable sign.
- Three low-poly NPC bodies: male, female, female with hijab; 1.6–1.75 m; feet pivot at origin; facing Blender −Y.
- One 21-bone generic rig shared by the variants; five clips `Idle`, `Walk`, `Eat`, `Talk`, `Wipe`; no root motion.
- NPC density: 5 in the Warteg (cook, server, three diners) and 9 in the bazaar (walkers, stall visitors, cafe guest, stage audience); set dressing only.
- No external character or environment assets. Author the Warteg and NPCs with the 19-skill Blender subset and the project category agents.
- NPC colliders live on the `NPC` layer and are excluded from the interaction raycast.
- Do not produce a Windows player build.

## Deliverables

| Area | Files |
|---|---|
| NPC Blender source | `Blender/NPC/scripts/build_npc_assets.py`, `Blender/NPC/NPC.blend` |
| NPC FBX | `Assets/Models/Characters/NPC/Npc_Male.fbx`, `Npc_Female.fbx`, `Npc_FemaleHijab.fbx` |
| NPC prefabs | `Assets/Prefabs/NPC/NPC_*.prefab`, `Assets/Animations/NPC/AC_*.controller` |
| NPC runtime | `Assets/Scripts/NPC/NpcAnimator.cs`, `NpcWalker.cs`, `NpcMarker.cs` |
| Warteg Blender source | `Blender/Scene2_Warteg/scripts/build_scene2_assets.py`, `Blender/Scene2_Warteg/Warteg.blend` |
| Warteg FBX | `Assets/Models/Environment/Warteg/WartegShell.fbx`, `WartegCounter.fbx`, `WartegFurniture.fbx`, `WartegProps.fbx` |
| Warteg materials | `Assets/Materials/Warteg/M_Warteg_*.mat` |
| Level 2 scene | `Assets/Scenes/Level02_Warteg.unity` |
| Level data | `Assets/ScriptableObjects/Levels/LD_Level02_Warteg.asset` |
| Builders | `Assets/Editor/NpcKitBuilder.cs`, `Assets/Editor/Scene2WartegBuilder.cs` |
| Scene 3 cast | `Assets/Editor/Scene3BazaarBuilder.cs` |

## Validation

- EditMode: 28 tests pass.
- PlayMode: 19 tests pass, including seven Level 2 foods, one display stand, five Level 2 NPCs, eight Level 3 stands, and nine Level 3 NPCs.
- Scene validator: all three scenes pass, campaign chain is `MainVR → Level02_Warteg → Level03_Bazaar`, NPC layer and animator setup are present.
- No Windows build.
