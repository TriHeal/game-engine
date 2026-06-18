# Tri-Heal (קשר מרפא)

Unity project for **Tri-Heal**, a synchronous real-time multiplayer platform for emotional regulation and trauma processing in children, connecting three roles: therapist, child, and parent. Built with the Metiv (מטיב) clinical association.

## Core Concept

Not a standalone app — a clinical tool used under a licensed therapist's discretion. The therapist runs structured sessions in the clinic, then assigns experiential "homework" missions that sync the child and parent into the same virtual space at home. The goal: keep the therapeutic continuity alive between weekly sessions and give the parent an active, in-the-moment role in their child's co-regulation, instead of helplessness.

Explicitly **not**: an open-ended AI chatbot/therapist (anti-attachment — avoids reinforcing rumination), and not a dry homework/task tracker (must feel like a cozy, motivating game).

## Three Interfaces (Tri-View Architecture), one shared real-time backend

| Interface | Audience | Core role |
|---|---|---|
| Therapist (Web Dashboard) | TF-CBT clinicians | Clinical metrics, live in-session control, assigns multiplayer home missions |
| Child (Mobile/Tablet, Unity) | Ages ~6-12 | Cozy 2D game world, personal avatar, regulation/processing mechanics |
| Parent (Mobile) | Parents/caregivers | "Therapeutic co-pilot" — live guidance cards, shared regulation exercises, response-blocking safeguards |

**This repo (Unity) is specifically the child-facing game client.** Parent/therapist interfaces are web-responsive, built separately.

## Permissions & Privacy Model

- **Therapist**: full admin access to clinical dashboard, session analytics, behavior monitoring tools (e.g. FullStory-style replay of what the child did in-game).
- **Parent**: limited, supportive interface only. Never sees the child's raw clinical content, free-text disclosures, or therapy session details — only shared guidance cards and joint tasks. This boundary protects the therapeutic alliance.
- **Child**: clean game-only experience. No settings, metrics, or access to parent/therapist management logic.

## MVP Scope — "Boat & Stone Tower" flow

One defined linear flow demonstrating clinical + technical feasibility of real-time parent/child and therapist/child sync:

1. **In clinic (therapist <> child, synced):** Co-regulation breathing exercise — both hold a finger on screen, breathing in/out with an expanding/contracting heart shape, to sail a shared wooden boat down a river. River blocked by rocks = an **EDI ("Emotional Debugging") stepper**: therapist types the child's stated **Fact** (engraved as a digital stone), child names the **Interpretation** (their feeling/belief), therapist taps "Separate" to crack the interpretation stone with a visual effect while verbally bridging fact vs. interpretation. Boat continues once understood.
2. **At home, routine practice (parent <> child, synced):** Short recurring goal (e.g. "3 weekly 5-minute joint sails"). Calm, low-stakes repetition builds the synchrony "muscle" via the same breathing-sync boat mechanic; rewards cosmetic boat items.
3. **At home, real-time flooding event (parent <> child, synced):** Child hits a real trigger (e.g. startled by a siren) and is brought into the familiar boat/tower experience rather than something that flags them as "in crisis." Mechanic example: **Validation Shield** — when the child places a "stress stone" naming a trigger, the parent's turn locks until they select/speak a validation phrase (preventing dismissive responses like "it's nothing"); only then can they place a "calming stone" and continue.
4. **Optional parent reflection log:** Parent can manually log what happened/how long it took to reach sync, shared with the therapist.
5. **Closing the clinical loop:** Therapist dashboard surfaces sync metrics (e.g. % improvement in breathing/tap synchrony, time-to-sync during routine vs. during a flooding event) and flagged trigger keywords — never raw transcripts — so the next session opens already focused.

## Key Design Constraints

- **Anti-attachment**: no open AI chat, no infinite conversation loops — flow is a defined linear stepper that always routes the interaction back to the real people in the room (therapist or parent), never to a bot.
- **Validation Shield mechanic** is a conceptual placeholder for a *category* of co-regulation mechanics — exact mechanics will be defined jointly with Metiv's clinical/psychology team before implementation, not hardcoded from this doc.
- **Avatar** for the child is a mediating in-game character (like a therapy-room puppet), not just cosmetic — it explains concepts like fact-vs-interpretation in-game (echoing the "stone that breaks" mechanic).
- Target age range ~6-12 (kindergarten through elementary), with room to extend older later.
- Scope control for the 1-month MVP timeline: parent/therapist UIs are simple web-responsive templates; all graphical/gameplay investment goes into the child's Unity client.
- Real-time sync (turn-taking, simultaneous taps, breathing-rate matching) between two distinct devices is the core technical risk to validate first.

## Unity Project Notes

- Unity 6000.4.10f1, Built-in Render Pipeline (no URP/HDRP configured).
- `Assets/SUIMONO - WATER SYSTEM 2/` — third-party water asset, used for the river/boat visuals. Needs `com.unity.ugui` package (added to `Packages/manifest.json`) for its legacy UI demo scripts.
- Main scene: `Assets/Scenes/main.unity`.
