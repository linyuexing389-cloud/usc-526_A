# GDD: Analytics Metrics

This section satisfies the requirement for **four analytics metrics** (planned, justified, and described) and documents the **implemented metric** with its **WebGL data collection** and **visualization pipeline**.

---

## 1. Death Cause Distribution *(implemented)*

**Definition.** Each time the player dies, we log: level (scene index), cause of death, time remaining, HP remaining, and timestamp.

**Categories.**
- **Timeout** — timer reached zero.
- **Spikes** — death by touching the red ball (spike hazard).
- **Trap** — death by touching the red wall (trap hazard).

**Justification.** Understanding *why* players die drives balance and level design: if most deaths are Timeout, we can adjust timer or pacing; if Spikes dominate, we can reduce or relocate red balls; if Trap is high, we can tweak red wall placement or telegraphing.

**Implementation.**
- Data is collected in-game (all platforms, including **WebGL**). Each death appends one record to a local log (JSONL + CSV).
- In **WebGL**, an in-game “Export analytics” (or equivalent) action triggers a **CSV download** so the same pipeline (CSV → graph) works for the Web build.
- **Visualization pipeline:** Export CSV from the game (or use the file written to `Application.persistentDataPath` on standalone), then:
  - **Option A:** Open in Excel/Sheets and create a bar chart of counts by `deathCause` (Timeout, Spikes, Trap).
  - **Option B:** Run the provided Python script to generate a bar chart image (see [Visualization Pipeline](#visualization-pipeline) below).

**Data structure (example).**
```json
{"level":2,"sceneName":"cube_map","deathCause":"Spikes","timeRemaining":14.3,"hpRemaining":0,"timestamp":"2025-03-02T12:00:00.000Z"}
```

---

## 2. Level Completion Rate (planned)

**Definition.** For each level, the ratio of sessions that reached the goal (win) vs. sessions that ended in a loss (any death or timeout), over a time window (e.g. per build or per week).

**Justification.** Measures where players get stuck or drop off. Low completion on a specific level signals difficulty spikes or unclear goals and guides iteration.

**Description.**  
- **Collect:** On win → log level index, scene name, timestamp. On loss → already covered by Death Cause (we can infer “did not complete” when the session ends without a win for that level).  
- **Compute:** Completion rate = wins per level / (wins + losses) per level.  
- **Visualization:** Bar or line chart: level (x) vs. completion rate (y); optional second series for attempt count.

**Status:** Planned; not yet implemented.

---

## 3. Time Spent per Level (planned)

**Definition.** For each level, aggregate play time from level load until win or death (e.g. mean, median, p90).

**Justification.** Identifies levels that are too long, too short, or highly variable (frustration or confusion). Informs timer tuning and pacing.

**Description.**  
- **Collect:** On level load → record timestamp; on win or death → log duration = now − load time, plus level index.  
- **Compute:** Per-level statistics (mean/median/p90 duration).  
- **Visualization:** Box plot or bar chart (level vs. duration); optional split by outcome (win vs. death).

**Status:** Planned; not yet implemented.

---

## 4. Hazard Encounter Rate (planned)

**Definition.** Per level, how often players interact with hazards (e.g. number of times the player touches a red ball or red wall, or takes damage from them) per completed run or per minute.

**Justification.** High encounter rate may indicate overly dense hazards or poor readability; low rate may indicate hazards are too easy to avoid or underused. Supports tuning of hazard density and placement.

**Description.**  
- **Collect:** On each damage event from Spikes or Trap, log level, hazard type (Spikes vs. Trap), and optionally timestamp. Optionally log “near miss” (e.g. close approach without damage) if we add triggers.  
- **Compute:** Encounters per level (or per minute) by hazard type; optionally split by “led to death” vs. “survived.”  
- **Visualization:** Grouped bar chart: level (x), counts for Spikes and Trap (y); or time series of encounters over session time.

**Status:** Planned; not yet implemented.

---

## Visualization Pipeline (implemented metric)

For the **Death Cause Distribution** metric:

1. **Data source**
   - **Standalone/Editor:** CSV (and JSONL) written to `Application.persistentDataPath` (see Unity Console for path after first death).
   - **WebGL:** Use the in-game **Export analytics** (or “Download CSV”) so the browser downloads `death_log.csv`.

2. **Generate bar chart**
   - **Excel/Google Sheets:** Open the CSV, insert a pivot chart or count by `deathCause`, then create a bar chart (Timeout, Spikes, Trap).
   - **Python:** From the project root, run:
     ```bash
     python scripts/visualize_death_analytics.py path/to/death_log.csv
     ```
     This produces a bar chart image (e.g. `death_cause_chart.png`) comparing counts for Timeout, Spikes, and Trap.

3. **Use in GDD / report**
   - Attach the generated chart to the GDD or report to show that the pipeline (game → data → graph) is in place and that the implemented metric is gathering data in WebGL.

---

## Summary Table

| Metric                     | Status     | Data collected in WebGL? | Visualization pipeline      |
|----------------------------|------------|---------------------------|-----------------------------|
| Death Cause Distribution   | Implemented| Yes (CSV export)         | CSV → Excel or Python chart |
| Level Completion Rate      | Planned    | —                        | Bar/line by level           |
| Time Spent per Level       | Planned    | —                        | Box/bar by level            |
| Hazard Encounter Rate      | Planned    | —                        | Grouped bar / time series   |
