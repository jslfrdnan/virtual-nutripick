# NutriPick VR — Agent & Skill Workflow

This project uses a small, project-specific Blender skill pack and six category subagents.

## Asset pipeline

1. `blender-director` reads the brief and routes the work.
2. `modeling` builds bodies, clothing, Warteg shell, counter, and props.
3. `production` sets materials and palette.
4. `technical` rigs and animates the NPCs.
5. `pipeline` assembles, exports, imports to Unity, and runs QA.
6. `styles` reviews silhouettes and palette.

The **main session** integrates Unity scenes, runs tests, and commits. Agents never commit git.

## Skills

Installed from `https://github.com/arjun988/blender-skills` (MIT). The task subset lives in `.opencode/skills/`:

- Orchestration: `blender-director`
- Modeling: `blender-modeler`, `character-artist`, `character-archetypes`, `environment-artist`, `prop-artist`
- Production: `materials`, `lookdev`
- Technical: `rigging`, `animation`, `lighting`
- Pipeline: `scene-assembly`, `set-dressing`, `asset-optimization`, `export-pipeline`, `unity-export`, `qa-review`
- Styles: `lowpoly-style`, `stylized-style`

## Agents

| Agent | Role | Nested spawn |
|---|---|---|
| `blender-director` | Routes work and holds the QA gate | allowed |
| `modeling` | Character/environment/prop geometry | denied |
| `production` | Materials and lookdev | denied |
| `technical` | Rigging, five clips, lighting | denied |
| `pipeline` | Assembly, export, Unity import, QA | denied |
| `styles` | Low-poly/stylized direction | denied |

All agents pin `opencode-go/deepseek-v4.1-flash` and run in `subagent` mode.

## Blender and Unity contract

- Blender 5.2 LTS, Blender MCP on `localhost:9876`; Unity 6.6 URP.
- 1 unit = 1 meter. Characters 1.6–1.75 m, feet pivot at origin, facing Blender −Y.
- Animated characters: FBX, Generic rig, baked clips, no root motion.
- Static Warteg modules: FBX or GLB, applied scale, −Z forward, Y up.
- Use URP materials in Unity; flat colors, no heavy textures.
- No external character or environment assets; author everything with the skills.
- NPC colliders live on the `NPC` layer and are excluded from the interaction raycast.
- No Windows player build unless the user asks.

## Blender safety

- Do not call `open_mainfile`, `read_factory_settings`, or `read_homefile` on the shared Blender session.
- Clear objects manually; keep one Blender instance.
- One `execute_blender_code` per stage; no pixel loops.
- Blender 5.2 has no `action.fcurves`; use pose-bone keyframe insertion and baked/NLA export.
- Verify with `get_scene_info`, `get_viewport_screenshot`, and a Unity import check.

## Campaign

Scene 1 is `MainVR`, Scene 2 is `Level02_Warteg`, Scene 3 is `Level03_Bazaar`. The chain is MainVR → Level02_Warteg → Level03_Bazaar.
