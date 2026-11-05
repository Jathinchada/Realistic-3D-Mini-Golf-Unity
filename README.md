# REALISTIC 3D MINI GOLF

> *A 3D mini-golf game developed using Unity and C# for Game Mechanics and Design Development.*

---

## ⛳ Project Overview

**REALISTIC 3D MINI GOLF** is a fully playable 3D mini-golf experience set in a relaxing, landscaped outdoor park environment. It features 5 distinct championship courses with bendy putting paths, smooth physical rolling turf, sunken cups, water hazards, decorative scenery, and custom synthesized audio.

### 🌟 Key Features
- **5 Championship Courses:**
  - **Hole 1: Peaceful Garden:** Serpentine putting track surrounded by cherry blossoms, flowering birch trees, and flowerbeds. (Par 2)
  - **Hole 2: Curved Pond Pathway:** Sweeping curve flanked by tranquil ponds and water hazard recovery mechanics. (Par 3)
  - **Hole 3: S-Bend Nature Track:** S-shaped fairway winding through pine and oak clusters, bushes, and granite boulders. (Par 3)
  - **Hole 4: C-Shaped Pond Chicane:** Scenic C-curve hugging a landscaped pond with lily pads and reeds. (Par 3)
  - **Hole 5: Elevated Wooden Bridge:** Dramatic finale crossing a scenic central pond via an elevated wooden bridge with timber trestles. (Par 3)
- **Practice Putting Green:** Dedicated warm-up putting green near the entrance with dual cups.
- **Realistic Ball & Green Physics:** Continuous Dynamic collision detection, weighted 2.5 kg physical golf ball, calibrated rolling turf resistance, and untextured smooth putting surfaces.
- **Sunken Cup Geometry:** Realistic sunken cups (0.58m diameter) with subtle funnel dynamics and celebration particle effects.
- **Audio & Nature Ambiance:** Procedural sound synthesis engine providing putter impacts, turf rolling friction, water splashes, cup sinking rattles, and background music with in-game mute toggle.
- **Full HUD & Scorecard:** Live stroke counter, par tracker, cumulative tournament scorecard (`[Tab]`), map overview (`[M]`), and hole-completion modals.

---

## 🎮 Controls & Shortcuts

| Action | Input (Mouse & Keyboard) | Description |
| :--- | :--- | :--- |
| **Putt / Aim (Slingshot)** | **Click & Drag Left Mouse Button** backward | Drag backward from ball to aim and charge power; release to strike |
| **Putt / Aim (Keyboard)** | **Hold [Spacebar]** | Smoothly charges shot power; release Spacebar to putt |
| **Adjust Aim** | **[A] / [D]** or **[←] / [→]** | Fine-tunes putting angle along the green |
| **Cancel Shot** | **Right Click** | Aborts shot while aiming/charging without taking a stroke |
| **Orbit Camera** | **Hold Right Mouse Button** + Move Mouse | Rotates camera smoothly around the golf ball |
| **Spin Camera** | **[Q] / [E]** | Rotates camera view left / right |
| **Zoom View** | **Mouse Scroll Wheel** | Zooms camera closer or further from the ball |
| **Map Overview Mode** | **[M]** or **[O]** | Toggles top-down bird's-eye camera of the entire park |
| **Championship Scorecard** | **[Tab]** | Opens/closes the 5-course scorecard modal |
| **Reset Ball to Tee** | **[R]** | Resets ball to the active hole's tee |
| **Next Hole** | **[N]** | Advances to next course upon sinking the ball |
| **Pause / Resume** | **[Esc]** or **[P]** | Opens pause menu with restart, settings, and main menu |

---

## 💻 How to Open and Play in Unity

1. Open **Unity Hub**.
2. Click **Add** -> **Add project from disk**.
3. Select this repository folder (`Realistic-3D-Mini-Golf`).
4. Open the project using **Unity 6 (6000.x)** or **Unity 2022.3 LTS**.
5. In the Project window, open:
   - `Assets/Scenes/MainMenu.unity` to launch from the Main Menu, OR
   - `Assets/Scenes/Gameplay.unity` to jump directly into the golf park.
6. Press the **Play [ ▶ ]** button at the top of the editor.

---

## 🧪 Automated Test Suite

The project includes an automated validation suite (`Assets/Scripts/Editor/AntiGravityGolfAutomatedTests.cs`):
- Run anytime via: **Unity Menu -> Anti-Gravity Golf -> Run Complete Automated Validation Suite**
- Verifies 56 distinct test assertions covering scene validity, 3D scenery, ball physics, continuous collision detection, score calculation, water hazards, tee spawning, sunken cup dimensions, and build settings.
- **Validation Status: 56 / 56 Tests Passed (100% Success)**
